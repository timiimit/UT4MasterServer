using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
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
public class UnrealTournamentRatingController : JsonAPIController
{
	private readonly ILogger<SessionController> logger;
	private readonly AccountService accountService;

	public UnrealTournamentRatingController(ILogger<SessionController> logger, AccountService accountService)
	{
		this.logger = logger;
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

		return Json(ratings);
	}

	[HttpGet("account/{id}/mmr/{ratingType}")]
	public IActionResult Mmr(string id, string ratingType, [FromBody] MMRBulk ratings)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		throw new NotImplementedException();

		// TODO: return only one type of rating

		// proper response: {"rating":1844,"numGamesPlayed":182}
		for (int i = 0; i < ratings.RatingTypes.Count; i++)
		{
			ratings.Ratings.Add(1500);
			ratings.NumGamesPlayed.Add(0);
		}

		return Json(ratings);
	}

	[HttpGet("account/{id}/league/{leagueName}")]
	public IActionResult LeagueRating(string id, string leagueName)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var league = new League();
		// TODO: for now we just send default/empty values
		return Json(league);
	}

	[HttpPost("team/elo/{ratingType}")]
	public IActionResult JoinQuickplay(string ratingType, [FromBody] string body)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		/*
		INPUT body:

		{
			"members": [
				{
					"accountId": "64bf8c6d81004e88823d577abe157373",
					"score": 0,
					"isBot": false
				}
			],
			"socialPartySize": 1
		}

		*/

		// Response: {"rating":1500}

		return Ok();
	}
}
