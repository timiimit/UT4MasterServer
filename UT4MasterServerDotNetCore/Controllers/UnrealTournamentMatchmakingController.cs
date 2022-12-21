using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

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

		[AuthorizeBasic]
		[HttpPost("session")]
		public IActionResult StartGameServer()
		{
			if (User.Identity is not EpicUserIdentity client)
				return Unauthorized();

			// TODO: parse game server
			var server = new GameServer("", "", "");

			matchmakingService.Add(client.Session.ID, server);

			return Ok();
		}

		[HttpPut("session/{id}")]
		public IActionResult UpdateGameServer(string id)
		{

			return Ok();
		}

		[HttpPost("session/matchMakingRequest")]
		public IActionResult ListGameServers([FromBody] GameServerFilter filter)
		{

			var list = new GameServer[]
			{
				new GameServer("we", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("cracked", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("the", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("code", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("and", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("entered", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("the", "[DS]dallastn-22938", "192.223.24.243"),
				new GameServer("matrix", "[DS]dallastn-22938", "192.223.24.243"),
			};

			var arr = new JArray();
			foreach (var server in list)
			{
				arr.Add(server.ToJson(true));
			}

			return Json(arr);
		}

		[HttpPost("session/{hubID}/join")] // value in hubID is a guess
		public IActionResult JoinHub(string hubID, [FromQuery] string accountID)
		{
			return NoContent(); // correct response
		}
	}
}
