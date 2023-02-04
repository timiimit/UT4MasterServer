using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Request;
using UT4MasterServer.Models.DTO.Response;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/game/v2/ratings")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class RatingsController : JsonAPIController
{
	private readonly RatingsService ratingsService;

	public RatingsController(ILogger<RatingsController> logger, RatingsService ratingsService) : base(logger)
	{
		this.ratingsService = ratingsService;
	}

	[HttpPost("account/{id}/mmrbulk")]
	public async Task<IActionResult> MmrBulk(string id, [FromBody] MMRBulkRequest ratings)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}

		var accountId = EpicID.FromString(id);

		var result = await ratingsService.GetRatingsAsync(accountId, ratings);

		return Ok(result);
	}

	[HttpGet("account/{id}/mmr/{ratingType}")]
	public async Task<IActionResult> Mmr(string id, string ratingType)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}
		var accountId = EpicID.FromString(id);
		if (!Rating.AllowedRatingTypes.Contains(ratingType))
		{
			logger.LogWarning("Unsupported rating type requested: {RatingType}, {AccountID}.", ratingType, accountId);
		}

		var result = await ratingsService.GetRatingAsync(accountId, ratingType);

		return Ok(result);
	}

	// THIS IS RETURNING SOME RESULTS
	// REQUEST: GET /ut/api/game/v2/ratings/account/0b0f09b400854b9b98932dd9e5abe7c5/league/RankedDuelSkillRating 
	// RESPONSE: {"tier":2,"division":0,"points":13,"isInPromotionSeries":false,"promotionMatchesAttempted":3,"promotionMatchesWon":3,"placementMatchesAttempted":10}
	[HttpGet("account/{id}/league/{leagueName}")]
	public IActionResult LeagueRating(string id, string leagueName)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}

		var league = new LeagueResponse();
		// TODO: for now we just send default/empty values
		return Ok(league);
	}

	// ONLY SENDING ONE PLAYER ALWAYS
	// REQUEST:	 { "members": [ { "accountId": "0b0f09b400854b9b98932dd9e5abe7c5", "score": 0, "isBot": false }], "socialPartySize": 1 }
	// RESPONSE: { "rating":1666 }
	[HttpPost("team/elo/{ratingType}")]
	public async Task<IActionResult> JoinQuickplay(string ratingType, [FromBody] RatingTeam ratingTeam)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}
		if (!Rating.AllowedRatingTypes.Contains(ratingType))
		{
			return BadRequest($"'{ratingType}' is not supported rating type.");
		}

		var response = await ratingsService.GetAverageTeamRatingAsync(ratingType, ratingTeam);

		return Ok(response);
	}

	[HttpPost("team/match_result")]
	public async Task<IActionResult> MatchResult([FromBody] RatingMatch ratingMatch)
	{
		if (!Rating.AllowedRatingTypes.Contains(ratingMatch.RatingType))
		{
			return BadRequest($"'{ratingMatch.RatingType}' is not supported rating type.");
		}

		if (Rating.DmRatingTypes.Contains(ratingMatch.RatingType))
		{
			await ratingsService.UpdateDeathmatchRatingsAsync(ratingMatch);
		}
		else
		{
			await ratingsService.UpdateTeamsRatingsAsync(ratingMatch);
		}

		return NoContent();
	}
}
