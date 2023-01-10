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
	private readonly AccountService accountService;

	public UnrealTournamentRatingController(ILogger<SessionController> logger, AccountService accountService) : base(logger)
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

		return Json(ratings);
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
		return Json(league);
	}

	[HttpPost("team/elo/{ratingType}")]
	public IActionResult JoinQuickplay(string ratingType, [FromBody] string body)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();


		// TODO: handle request
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

	[HttpPost("team/match_result")]
	public IActionResult MatchResult()
	{

		// TODO: handle request
		// INPUT:
		/*

		{
	"ratingType": "FlagRunSkillRating",
	"matchInfo":
	{
		"sessionId": "8f06d9db71ca4dee8fc0663cbc953b02",
		"redScore": 1,
		"matchLengthSeconds": 526
	},
	"redTeam":
	{
		"members": [
			{
				"accountId": "Acolyte",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Judas",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"score": 0,
				"isBot": false
			},
			{
				"accountId": "Thannis",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Skirge",
				"score": 0,
				"isBot": true
			}
		],
		"socialPartySize": 1
	},
	"blueTeam":
	{
		"members": [
			{
				"accountId": "Trollface",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Leeb",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Gaargod",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Kragoth",
				"score": 0,
				"isBot": true
			},
			{
				"accountId": "Garog",
				"score": 0,
				"isBot": true
			}
		],
		"socialPartySize": 1
	}
}

		*/



		return NoContent(); // Response: correct response
	}
}
