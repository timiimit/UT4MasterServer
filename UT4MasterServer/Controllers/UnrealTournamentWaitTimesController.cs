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
	private readonly MatchmakingWaitTimeEstimateService service;

	public UnrealTournamentWaitTimesController(
		ILogger<UnrealTournamentWaitTimesController> logger,
		MatchmakingWaitTimeEstimateService service) : base(logger)
	{
		this.service = service;
	}

	[HttpGet("estimate")]
	public IActionResult QuickplayWaitEstimate()
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		return Ok(service.GetWaitTimes());
	}

	[HttpGet("report/{ratingType}/{timeWaited}")]
	public IActionResult QuickplayWaitReport(string ratingType, double timeWaited)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		service.AddWaitTime(ratingType, timeWaited);

		return NotFound();
	}
}
