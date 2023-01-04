using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;

namespace UT4MasterServer.Controllers;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/stats")]
[AuthorizeBearer]
[Produces("application/json")]
public class UnrealTournamentStatsController : JsonAPIController
{
	private readonly ILogger<UnrealTournamentStatsController> logger;

	public UnrealTournamentStatsController(ILogger<UnrealTournamentStatsController> logger)
	{
		this.logger = logger;
	}

	[HttpGet("accountId/{id}/bulk/window/{category}")]
	public IActionResult Stats(string id, string leagueName, string category)
	{
		// TODO: Check id and return ErrorResponse if invalid: "Could not convert fakeAccountId to a UUID because it was not in a valid format: Invalid UUID string: fakeAccountId" (errors.com.epicgames.modules.stats.invalid_account_id)

		// TODO: return Stats

		/*
		 * EPIC example response for 'weekly' (first 2 objects):
		 *
		    {
		        "name": "RocketHits",
		        "value": 15725,
		        "window": "weekly",
		        "ownerType": 1
		    },
		    {
		        "name": "ShockRifleShots",
		        "value": 40,
		        "window": "weekly",
		        "ownerType": 1
		    },
		 */

		switch (category.ToLower())
		{
			case "alltime":
				break;
			case "monthly":
				break;
			case "weekly":
				break;
			case "daily":
				break;
			default:
				return NotFound(null);
		}

		var league = new League();
		// for now we just send default/empty values
		return Json(league);
	}

	[HttpPost("accountId/{id}/bulk")]
	public IActionResult Stats(string id, [FromQuery(Name = "ownerType")] int? ownerType)
	{
		// TODO: Missing implementation, this endpoint uploads stats

		// Request body after completing singleplayer challenge: {"MatchesPlayed":1,"TimePlayed":480,"Wins":1,"Kills":14,"Deaths":6,"SpreeKillLevel0":1,"ShockBeamKills":1,"ShockComboKills":7,"LinkBeamKills":1,"FlakShardKills":1,"FlakShellKills":2,"RocketKills":2,"EnforcerDeaths":1,"ShockBeamDeaths":1,"FlakShardDeaths":1,"SniperDeaths":2,"SniperHeadshotDeaths":1,"ShockRifleShots":117,"LinkShots":66,"MinigunShots":24,"FlakShots":30,"RocketShots":19,"SniperShots":10,"ShockRifleHits":4950.75439453125,"LinkHits":1032.14306640625,"MinigunHits":1000,"FlakHits":513.07794189453125,"RocketHits":177.00001525878906,"SniperHits":300,"ShieldBeltCount":2,"ArmorVestCount":8,"ArmorPadsCount":5,"HelmetCount":3,"BestShockCombo":7.2227811813354492,"RunDist":235007.859375,"InAirDist":243375.703125,"NumDodges":159,"NumWallDodges":35,"NumJumps":10,"NumLiftJumps":4,"NumFloorSlides":1,"NumWallRuns":13,"SlideDist":426.73013305664063,"WallRunDist":5063.228515625}

		if (ownerType != 1)
		{
			logger.LogWarning($"Unexpected ownerType at '{Request.GetDisplayUrl()}'");
		}


		return NoContent();
	}
}
