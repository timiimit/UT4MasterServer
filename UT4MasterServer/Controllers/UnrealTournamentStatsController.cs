using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;

namespace UT4MasterServer.Controllers;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api")]
[AuthorizeBearer]
[Produces("application/json")]
public class UnrealTournamentStatsController : JsonAPIController
{
	[HttpGet("stats/accountId/{id}/bulk/window/{category}")]
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
}
