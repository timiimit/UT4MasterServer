#define USE_LOCALHOST_TEST

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Request;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Common.Enums;
using System.Text.Json.Nodes;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/matchmaking")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class MatchmakingController : JsonAPIController
{
	private readonly MatchmakingService matchmakingService;
	private readonly TrustedGameServerService trustedGameServerService;

	private readonly IOptions<ApplicationSettings> configuration;

	public MatchmakingController(
		ILogger<MatchmakingController> logger,
		IOptions<ApplicationSettings> configuration,
		MatchmakingService matchmakingService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService) : base(logger)
	{
		this.configuration = configuration;
		this.matchmakingService = matchmakingService;
		this.trustedGameServerService = trustedGameServerService;
	}

	#region Endpoints for Game Servers

	[HttpPost("session")]
	public async Task<IActionResult> CreateGameServer([FromBody] GameServer server)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var ipClient = GetClientIP(configuration);
		if (ipClient == null)
		{
			logger.LogError("Could not determine IP Address of remote machine.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
		if (ipClient.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
		{
			logger.LogWarning("Client is using IPv6. GameServer might not be accessible by others.");
		}

		server.SessionID = user.Session.ID;
		server.OwningClientID = user.Session.ClientID;
#if DEBUG
		server.SessionAccessToken = user.AccessToken;
#endif
		server.ID = EpicID.GenerateNew();
		server.LastUpdated = DateTime.UtcNow;

		server.ServerAddress = ipClient.ToString();
		server.Started = false;

		GameServerTrust trust = GameServerTrust.Untrusted;
		var trusted = await trustedGameServerService.GetAsync(server.OwningClientID);
		if (trusted != null)
		{
			trust = trusted.TrustLevel;
		}

#if DEBUG && true
		trust = GameServerTrust.Epic;
#endif

		server.Attributes.Set("UT_SERVERTRUSTLEVEL_i", (int)trust);

		if (await matchmakingService.DoesExistWithSessionAsync(server.SessionID))
		{
			// each session may only create a single game server
			return BadRequest();
		}

		await matchmakingService.AddAsync(server);

		return Ok(server.ToJson(false));
	}

	[HttpPut("session/{id}")]
	public async Task<IActionResult> UpdateGameServer(string id, [FromBody] GameServer updatedServer)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var serverID = EpicID.FromString(id);

		if (updatedServer.ID != serverID)
			return UnknownSessionId(id);

		updatedServer.SessionID = user.Session.ID;
		updatedServer.OwningClientID = user.Session.ClientID;

		var server = await matchmakingService.GetAsync(serverID);
		if (server == null)
			return UnknownSessionId(id);

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

		server.Update(updatedServer);

		await matchmakingService.UpdateAsync(server);

		return Ok(server.ToJson(false));
	}

	[HttpGet("session/{id}")]
	public async Task<IActionResult> GetGameServer(string id)
	{
		// NOTE: this method is called by client game after server resets session
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var serverID = EpicID.FromString(id);

		var server = await matchmakingService.GetAsync(serverID);
		if (server == null)
			return UnknownSessionId(id);

		return Ok(server.ToJson(false));
	}

	[HttpDelete("session/{id}")]
	public async Task<IActionResult> DeleteGameServer(string id)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var serverID = EpicID.FromString(id);

		var server = await matchmakingService.GetAsync(serverID);
		if (server == null)
			return UnknownSessionId(id);

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

		bool wasDeleted = await matchmakingService.RemoveAsync(EpicID.FromString(id));

		// TODO: unknown actual responses but these seem to work

		if (!wasDeleted)
			return UnknownSessionId(id);

		return Ok();
	}

	[HttpPost("session/{id}/start")]
	public async Task<IActionResult> NotifyGameServerIsReady(string id)
	{
		return await ChangeGameServerStarted(id, true);
	}

	[HttpPost("session/{id}/stop")]
	public async Task<IActionResult> NotifyGameServerHasStopped(string id)
	{
		return await ChangeGameServerStarted(id, false);
	}

	[HttpPost("session/{id}/heartbeat")]
	public async Task<IActionResult> GameServerHeartbeat(string id)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var server = await matchmakingService.GetAsync(EpicID.FromString(id));
		if (server == null)
			return UnknownSessionId(id);

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

#if false
		// HACK: server will create new session 2h before current one expires.
		//       to guarantee us a way to track a single server between sessions
		//       we start sending failed heartbeat before server would
		//       have created it. by doing so we force the server to:
		//         - create new session token
		//         - use new session token to stop old server
		//         - use new session token to start new server

		if (user.Session.CreationMethod == SessionCreationMethod.ClientCredentials &&
			user.Session.AccessToken.ExpirationDuration < TimeSpan.FromHours(2) + TimeSpan.FromSeconds(25))
		{
			return NotFound(new ErrorResponse
			{
				ErrorCode = "errors.com.epicgames.common.oauth.unauthorized_client",
				ErrorMessage = $"Sorry your client is not allowed to use the grant type client_credentials. Please download and use UT4UU",
				NumericErrorCode = 1015,
				OriginatingService = "com.epicgames.account.public",
				Intent = "prod",
				ErrorDescription = $"Sorry your client is not allowed to use the grant type client_credentials. Please download and use UT4UU",
				Error = "unauthorized_client",
			});

			//return UnknownSessionId(id); // trigger server recreation
		}
#endif

		server.LastUpdated = DateTime.UtcNow;
		await matchmakingService.UpdateAsync(server);

		return NoContent();
	}

	[HttpPost("session/{id}/players")]
	public async Task<IActionResult> UpdateGameServerPlayers(string id, [FromBody] GameServer serverOnlyWithPlayers)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var serverID = EpicID.FromString(id);

		var server = await matchmakingService.GetAsync(serverID);
		if (server == null)
			return NoContent();

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

		// handle player list update
		foreach (var player in serverOnlyWithPlayers.PublicPlayers)
		{
			if (!server.PublicPlayers.Where(x => x == player).Any())
			{
				server.PublicPlayers.Add(player);
			}
			if (server.PrivatePlayers.Where(x => x == player).Any())
			{
				server.PrivatePlayers.Remove(player);
			}
		}
		foreach (var player in serverOnlyWithPlayers.PrivatePlayers)
		{
			if (!server.PrivatePlayers.Where(x => x == player).Any())
			{
				server.PrivatePlayers.Add(player);
			}
			if (server.PublicPlayers.Where(x => x == player).Any())
			{
				server.PublicPlayers.Remove(player);
			}
		}

		await matchmakingService.UpdateAsync(server);

		return Ok(server.ToJson(false));
	}

	[HttpDelete("session/{id}/players")]
	public async Task<IActionResult> RemovePlayer(string id, [FromBody] EpicID[] players)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var server = await matchmakingService.GetAsync(EpicID.FromString(id));
		if (server == null)
			return UnknownSessionId(id);

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

		foreach (var player in players)
		{
			server.PublicPlayers.Remove(player);
			server.PrivatePlayers.Remove(player);
		}

		await matchmakingService.UpdateAsync(server);

		return Ok(server.ToJson(false));
	}

	#endregion

	#region Endpoints for Clients

	[AllowAnonymous]
	[HttpPost("session/matchMakingRequest")]
	public async Task<IActionResult> ListGameServers([FromBody] GameServerFilterRequest filter)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			logger.LogInformation($"'{Request.HttpContext.Connection.RemoteIpAddress}' accessed GameServer list without authentication");
		}

		var servers = await matchmakingService.ListAsync(filter);

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

		var arr = new JsonArray();
		foreach (var server in servers)
		{
#if DEBUG && USE_LOCALHOST_TEST
			server.ServerAddress = "127.0.0.1";
#endif

			arr.Add(server.ToJson(true));
		}

		return Ok(arr);
	}

	/// <summary>
	/// This action is for convenience of users to be able to easily retrieve a list of hubs and/or servers.
	/// </summary>
	/// <param name="showHubs">whether to return hubs</param>
	/// <param name="showServers">whether to return servers</param>
	/// <returns></returns>
	[AllowAnonymous]
	[HttpGet("session/matchMakingRequest")]
	public async Task<IActionResult> ListGameServers([FromQuery] bool? showHubs, [FromQuery] bool? showServers)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			logger.LogInformation($"'{Request.HttpContext.Connection.RemoteIpAddress}' accessed GameServer list without authentication");
		}

		// TODO: implement query filters
		return await ListGameServers(new GameServerFilterRequest());
	}

	[HttpPost("session/{id}/join")]
	public async Task<IActionResult> PlayerJoinGameServer(string id, [FromQuery(Name = "accountId")] string accountID)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		EpicID eid = EpicID.FromString(id);

		if (!await matchmakingService.DoesExistAsync(eid))
			return UnknownSessionId(id);

		// TODO: we should verify that specific user has joined specific GameServer
		//       instead of just relying on GameServer blindly believing that user
		//       really is who he says he is.
		//       Then we can probably deny user's entry to GameServer by not sending data
		//       in QueryProfile request (just a guess).

		return NoContent(); // correct response
	}

	#endregion

	[NonAction]
	private NotFoundObjectResult UnknownSessionId(string id)
	{
		return NotFound(new ErrorResponse
		{
			ErrorCode = "errors.com.epicgames.modules.matchmaking.unknown_session",
			ErrorMessage = $"unknown session id {id}",
			MessageVars = new[] { id },
			NumericErrorCode = 12101,
			OriginatingService = "utservice",
			Intent = "prod10",
		});
	}

	[NonAction]
	private async Task<IActionResult> ChangeGameServerStarted(string id, bool started)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var serverID = EpicID.FromString(id);

		var server = await matchmakingService.GetAsync(serverID);
		if (server == null)
			return UnknownSessionId(id);

		if (server.OwningClientID != user.Session.ClientID)
			Unauthorized();

		server.Started = started;

		// Don't immediately remove it, let it become stale.
		await matchmakingService.UpdateAsync(server);

		return NoContent();
	}
}
