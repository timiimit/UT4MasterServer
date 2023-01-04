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
[Route("ut/api/game/v2/wait_times")]
[AuthorizeBearer]
[Produces("application/json")]
public class UnrealTournamentWaitTimesController : JsonAPIController
{

	[HttpPost("estimate")]
	public IActionResult QuickplayWaitEstimate()
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		// TODO: QuickplayWaitEstimate

		// Response: [{"ratingType":"DMSkillRating","averageWaitTimeSecs":15.833333333333334,"numSamples":6},
		// {"ratingType":"FlagRunSkillRating","averageWaitTimeSecs":15.0,"numSamples":7}]
		return Ok();
	}

	[HttpGet("report/{ratingType}/{unkownNumber}")]
	public IActionResult QuickplayWaitReport()
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		return NotFound(null);
	}
}
