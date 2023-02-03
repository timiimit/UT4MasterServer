using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Controllers.Epic;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Services.Scoped;
using Microsoft.Extensions.Logging;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/game/v2/ratings")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class RatingController : JsonAPIController
{
    private readonly AccountService accountService;

    public RatingController(ILogger<SessionController> logger, AccountService accountService) : base(logger)
    {
        this.accountService = accountService;
    }

    [HttpPost("account/{id}/mmrbulk")]
    public IActionResult MmrBulk(string id, [FromBody] MMRBulk ratings)
    {
        if (User.Identity is not EpicUserIdentity user)
            return Unauthorized();

        for (int i = 0; i < ratings.RatingTypes.Count; i++)
        {
            ratings.Ratings.Add(1500);
            ratings.NumGamesPlayed.Add(0);
        }

        return Ok(ratings);
    }

    [HttpGet("account/{id}/mmr/{ratingType}")]
    public IActionResult Mmr(string id, string ratingType)
    {
        if (User.Identity is not EpicUserIdentity user)
            return Unauthorized();

        // TODO: return only one type of rating

        // proper response: {"rating":1844,"numGamesPlayed":182}
        JObject obj = new JObject()
        {
            { "rating", 1500 },
            { "numGamesPlayed", 0 }
        };

        return Json(obj);
    }

    [HttpGet("account/{id}/league/{leagueName}")]
    public IActionResult LeagueRating(string id, string leagueName)
    {
        if (User.Identity is not EpicUserIdentity user)
            return Unauthorized();

        var league = new League();
        // TODO: for now we just send default/empty values
        return Ok(league);
    }

    [HttpPost("team/elo/{ratingType}")]
    public IActionResult JoinQuickplay(string ratingType, [FromBody] RatingTeam body)
    {
        if (User.Identity is not EpicUserIdentity user)
            return Unauthorized();

        // TODO: calculate proper rating for this team

        return Ok(new Rating() { RatingValue = 1500 });
    }

    [HttpPost("team/match_result")]
    public IActionResult MatchResult([FromBody] RatingMatch body)
    {
        // TODO: update ELO rating

        return NoContent(); // Response: correct response
    }
}
