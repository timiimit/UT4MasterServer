using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Models.Requests;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/game/v2/ratings")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class UnrealTournamentRatingController : JsonAPIController
{
	private readonly RatingsService ratingsService;

	public UnrealTournamentRatingController(ILogger<SessionController> logger, RatingsService ratingsService) : base(logger)
	{
		this.ratingsService = ratingsService;
	}

	[HttpPost("account/{id}/mmrbulk")]
	public async Task<IActionResult> MmrBulk(string id, [FromBody] MMRBulk ratings)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var accountId = EpicID.FromString(id);

		var result = await ratingsService.GetRatingsAsync(accountId, ratings);

		return Json(result);
	}

	[HttpGet("account/{id}/mmr/{ratingType}")]
	public async Task<IActionResult> Mmr(string id, string ratingType)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var accountId = EpicID.FromString(id);

		var result = await ratingsService.GetRatingAsync(accountId, ratingType);

		return Json(result);
	}

	// THIS IS RETURNING SOME RESULTS
	// REQUEST: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedDuelSkillRating 
	// RESPONSE: {"tier":2,"division":0,"points":13,"isInPromotionSeries":false,"promotionMatchesAttempted":3,"promotionMatchesWon":3,"placementMatchesAttempted":10}
	[HttpGet("account/{id}/league/{leagueName}")]
	public IActionResult LeagueRating(string id, string leagueName)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var league = new League();
		// TODO: for now we just send default/empty values
		return Ok(league);
	}

	// ONLY SENDING ONE PLAYER ALWAYS
	// REQUEST:	 { "members": [ { "accountId": "0b0f09b400854b9b98932dd9e5abe7c5", "score": 0, "isBot": false }], "socialPartySize": 1 }
	// RESPONSE: { "rating":1666 }
	[HttpPost("team/elo/{ratingType}")]
	public IActionResult JoinQuickplay(string ratingType, [FromBody] RatingTeam body)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		// TODO: calculate proper rating for this team

		return Ok(new RatingResponse() { Rating = 1500 });
	}

	[HttpPost("team/match_result")]
	public async Task<IActionResult> MatchResult([FromBody] RatingMatch ratingMatch)
	{
		switch (ratingMatch.RatingType)
		{
			case "SkillRating":
				await ratingsService.UpdateTeamsRatingsAsync(ratingMatch, x => x.SkillRating);
				break;

			case "TDMSkillRating":
				await ratingsService.UpdateTeamsRatingsAsync(ratingMatch, x => x.TDMSkillRating);
				break;

			case "CTFSkillRating":
				await ratingsService.UpdateTeamsRatingsAsync(ratingMatch, x => x.CTFSkillRating);
				break;

			case "ShowdownSkillRating":
				await ratingsService.UpdateTeamsRatingsAsync(ratingMatch, x => x.ShowdownSkillRating);
				break;

			case "FlagRunSkillRating":
				await ratingsService.UpdateTeamsRatingsAsync(ratingMatch, x => x.FlagRunSkillRating);
				break;

			case "DMSkillRating":
				await ratingsService.UpdateDeathmatchRatingsAsync(ratingMatch);
				break;

			default:
				return BadRequest("Unknown rating type.");
		}

		return NoContent();
	}
}
