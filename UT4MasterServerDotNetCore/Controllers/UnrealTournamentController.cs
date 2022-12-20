using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;

namespace UT4MasterServer.Controllers
{
    [ApiController]
    [Route("ut/api/game/v2")]
    [AuthorizeBearer]
    [Produces("application/json")]
    public class UnrealTournamentController : JsonAPIController
	{
        private readonly ILogger<SessionController> logger;

        public UnrealTournamentController(ILogger<SessionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Route("profile/{id}/client/QueryProfile")]
        public IActionResult QueryProfile(string id)
        {
            return Ok(new { });
        }

        [HttpPost]
        [Route("ratings/account/{id}/mmrbulk")]
        public IActionResult MmrBulk(string id)
        {
            // TODO: Figure out empty responses
            // Empty JSON crashes the game, NoContentResult or "{}" does not.
            // Not sure if ActionResult<string> with "{}" is legit since it's treated by proxys as a pure string rather than JSON
            // Maybe ActionResult with `return Ok(new {});` is more correct. Certainly not for this endpoint...
            return new NoContentResult();
        }

        [HttpGet]
        [Route("ratings/account/{id}/league/{leagueName}")]
        public IActionResult LeagueRating(string id, string leagueName)
        {
            return Ok(new { });
        }
    }
}
