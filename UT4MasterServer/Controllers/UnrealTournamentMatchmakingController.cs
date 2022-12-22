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

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } };
			var server = JsonSerializer.Deserialize<GameServer>(await ReadBodyAsStringAsync(1024), options);
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
				logger.LogWarning($"GameServer is connecting from ipv6 address ({ipAddress}). This is unknown territory. Make sure everything works as expected!");
				// let's continue to hope ipv6 works
			}
			server.ServerAddress = ipAddress.ToString();
			server.Started = false;
			server.TotalPlayers = 0;

			// TODO: figure out trusted keys & determine trust level
			//server.Attributes.Set("UT_SERVERTRUSTLEVEL", (int)GameServerTrust.Untrusted);

			matchmakingService.Add(user.Session.ID, server);

			return Ok();
		}

		[HttpPut("session/{id}")]
		public async Task<IActionResult> UpdateGameServer(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } };
			var server = JsonSerializer.Deserialize<GameServer>(await ReadBodyAsStringAsync(1024), options);

			return Ok();
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

			if (!matchmakingService.Heartbeat(user.Session.ID, EpicID.FromString(id)))
				return BadRequest();

			return NoContent();
		}

		[HttpPost("session/{id}/players")]
		public async Task<IActionResult> UpdateGameServerPlayers(string id)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();



			var options = new JsonSerializerOptions() { Converters = { new EpicIDJsonConverter() } };
			var serverOnlyWithPlayers = JsonSerializer.Deserialize<GameServer>(await ReadBodyAsStringAsync(1024), options);

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

			var list = matchmakingService.List(filter);

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
			foreach (var server in list)
			{
				arr.Add(server.ToJson(true));
			}

			return Json(arr);
		}

		[HttpPost("session/{id}/join")]
		public IActionResult PlayerJoinGameServer(string id, [FromQuery(Name = "accountId")] string accountID)
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			return NoContent(); // correct response
		}

		#endregion
	}
}
