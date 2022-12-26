#define USE_LOCALHOST_TEST

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Text;
using System.Text.Json;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api/matchmaking")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class UnrealTournamentMatchmakingController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;
		private readonly MatchmakingService matchmakingService;
		private const int MAX_READ_SIZE = 1024 * 4;

		public UnrealTournamentMatchmakingController(ILogger<SessionController> logger, MatchmakingService matchmakingService)
        {
            this.logger = logger;
			this.matchmakingService = matchmakingService;

		}

		#region Endpoints for Game Servers

		[HttpPost("session")]
		public async Task<IActionResult> StartGameServer()
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter(), new GameServerAttributesJsonConverter() } };
			var body = await ReadBodyAsStringAsync(MAX_READ_SIZE);
			var server = JsonSerializer.Deserialize<GameServer>(body, options);
			if (server == null)
				return BadRequest();

			server.ID = EpicID.GenerateNew();
			server.LastUpdated = DateTime.UtcNow;

			var ipAddress = HttpContext.Connection.RemoteIpAddress;
			if (ipAddress == null)
			{
				// TODO
				// wtf!? why can this be null???
				logger.LogCritical($"Could not determine ip address of remote GameServer, this issue needs to be resolved!");
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
			{
				// ipv6 does not seem to work
				logger.LogWarning($"GameServer is connecting from ipv6 address ({ipAddress})! mapping to ipv4...");
				ipAddress = ipAddress.MapToIPv4();
			}
			server.ServerAddress = ipAddress.ToString();
			server.Started = false;

			// TODO: figure out trusted keys & determine trust level
			server.Attributes.Set("UT_SERVERTRUSTLEVEL_i", (int)GameServerTrust.Untrusted);

			matchmakingService.Add(user.Session.ID, server);

			return Json(server.ToJson(false));
		}

		[HttpPut("session/{id}")]
		public async Task<IActionResult> UpdateGameServer(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter(), new GameServerAttributesJsonConverter() } };
			var body = await ReadBodyAsStringAsync(MAX_READ_SIZE);
			var update = JsonSerializer.Deserialize<GameServer>(body, options);
			if (update == null)
				return BadRequest();

			var old = matchmakingService.Get(user.Session.ID, update.ID);
			if (old == null)
				return BadRequest();

			old.Update(update);

			matchmakingService.Update(user.Session.ID, old);

			return Json(old.ToJson(false));
		}

		[HttpPost("session/{id}/start")]
		public IActionResult NotifyGameServerIsReady(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var serverID = EpicID.FromString(id);

			var server = matchmakingService.Get(user.Session.ID, serverID);
			if (server == null)
				return BadRequest();

			server.Started = true;
			
			matchmakingService.Update(user.Session.ID, server);

			return NoContent();
		}

		[HttpPost("session/{id}/heartbeat")]
		public IActionResult GameServerHeartbeat(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var server = matchmakingService.Get(user.Session.ID, EpicID.FromString(id));
			if (server == null)
				return BadRequest();

			server.LastUpdated = DateTime.UtcNow;
			matchmakingService.Update(user.Session.ID, server);

			return NoContent();
		}

		[HttpPost("session/{id}/players")]
		public async Task<IActionResult> UpdateGameServerPlayers(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter(), new GameServerAttributesJsonConverter() } };
			var serverOnlyWithPlayers = JsonSerializer.Deserialize<GameServer>(await ReadBodyAsStringAsync(MAX_READ_SIZE), options);

			if (serverOnlyWithPlayers == null)
				return NoContent();

			var serverID = EpicID.FromString(id);

			var old = matchmakingService.Get(user.Session.ID, serverID);
			if (old == null)
				return NoContent();

			old.PublicPlayers = serverOnlyWithPlayers.PublicPlayers;
			old.PrivatePlayers = serverOnlyWithPlayers.PrivatePlayers;

			matchmakingService.Update(serverID, old);

			return Json(old.ToJson(false));
		}

		[HttpDelete("session/{id}/players")]
		public async Task<IActionResult> RemovePlayer(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter(), new GameServerAttributesJsonConverter() } };
			var players = JsonSerializer.Deserialize<EpicID[]>(await ReadBodyAsStringAsync(MAX_READ_SIZE), options);
			if (players == null)
				return BadRequest();

			var server = matchmakingService.Get(user.Session.ID, EpicID.FromString(id));
			if (server == null)
				return BadRequest();

			foreach (var player in players)
			{
				server.PublicPlayers.Remove(player);
				server.PrivatePlayers.Remove(player);
			}

			return Json(server.ToJson(false));
		}

		#endregion

		#region Endpoints for Clients

		[AllowAnonymous]
		[HttpPost("session/matchMakingRequest")]
		public IActionResult ListGameServers([FromBody] GameServerFilter filter)
		{
			if (User.Identity is not EpicUserIdentity user)
			{
				// allow any third-party project to easily access hub list without any authentication
			}

			var servers = matchmakingService.List(filter);

			//var list = new GameServer[]
			//{
			//	new GameServer("we", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("cracked", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("the", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("code", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("and", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("entered", "[DS]dallastn-22938", "192.223.24.243"),
			//	new GameServer("the", "[DS]dallastn-22938", "192.223.24.243"), // does not show, due to duplicate data
			//	new GameServer("matrix", "[DS]dallastn-22938", "192.223.24.243"),
			//};

			var arr = new JArray();
			foreach (var server in servers)
			{
#if DEBUG && USE_LOCALHOST_TEST
				server.ServerAddress = "127.0.0.1";
#endif

				arr.Add(server.ToJson(true));
			}

			return Json(arr);
		}

		[HttpPost("session/{id}/join")]
		public IActionResult PlayerJoinGameServer(string id, [FromQuery(Name = "accountId")] string accountID)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			EpicID eid = EpicID.FromString(id);

			return NoContent(); // correct response
		}

		#endregion
	}
}
