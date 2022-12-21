using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class UnrealTournamentMatchmakingController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;
		private readonly AccountService accountService;

		public UnrealTournamentMatchmakingController(ILogger<SessionController> logger, AccountService accountService)
        {
            this.logger = logger;
			this.accountService = accountService;

		}

		[HttpPost("matchmaking/session/matchMakingRequest")]
		public IActionResult ListHubs([FromBody] string body)
		{
			/*
			
			INPUT example 1:
			{
				"criteria": [
					{
						"type": "NOT_EQUAL",
						"key": "UT_GAMEINSTANCE_i",
						"value": 1
					},
					{
						"type": "NOT_EQUAL",
						"key": "UT_RANKED_i",
						"value": 1
					}
				],
				"buildUniqueId": "256652735",
				"maxResults": 10000
			}

			INPUT example 2:
			{
				"criteria": [
					{
						"type": "EQUAL",
						"key": "UT_SERVERVERSION_s",
						"value": "3525360"
					},
					{
						"type": "EQUAL",
						"key": "UT_SERVERINSTANCEGUID_s",
						"value": "022854C6190A1605003202180546F2E7"
					}
				],
				"buildUniqueId": "256652735",
				"maxResults": 1
			}

			Response is in <repo_root>/UT4MasterServer/HubListResponse.json
			Model for response is already there in Hub.cs

			*/
			return Ok();
		}

		[HttpPost("matchmaking/session/{hubID}/join")] // value in hubID is a guess
		public IActionResult JoinHub(string hubID, [FromQuery] string accountID)
		{
			return NoContent(); // correct response
		}
	}
}
