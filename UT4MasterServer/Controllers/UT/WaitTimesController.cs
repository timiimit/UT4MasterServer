using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/game/v2/wait_times")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class WaitTimesController : JsonAPIController
{
	private readonly MatchmakingWaitTimeEstimateService service;

	public WaitTimesController(
		ILogger<WaitTimesController> logger,
		MatchmakingWaitTimeEstimateService service) : base(logger)
	{
		this.service = service;
	}

	[HttpGet("estimate")]
	public IActionResult QuickplayWaitEstimate()
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}

		// Per UTMcpUtils.cpp:186-223: response must be a bare JSON array of
		// FWaitTimeInfo. Content-Type is compared by exact string match —
		// emit "application/json" without a charset suffix or the client
		// logs "Error: 1" (HTTP 200 but Content-Type mismatch).
		List<UT4MasterServer.Models.DTO.Responses.WaitTimeEstimateResponse> times = service.GetWaitTimes();
		if (times.Count == 0)
		{
			times.Add(new UT4MasterServer.Models.DTO.Responses.WaitTimeEstimateResponse(
				"FlagRunSkillRating", 0.0, 1));
		}
		var json = System.Text.Json.JsonSerializer.Serialize(times);
		return Content(json, "application/json");
	}

	[HttpGet("report/{ratingType}/{timeWaited}")]
	public IActionResult QuickplayWaitReport(string ratingType, double timeWaited)
	{
		if (User.Identity is not EpicUserIdentity)
		{
			return Unauthorized();
		}

		service.AddWaitTime(ratingType, timeWaited);

		return NoContent();
	}
}
