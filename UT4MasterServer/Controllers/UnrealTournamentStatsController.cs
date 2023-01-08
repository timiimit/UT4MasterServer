using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.DTOs;
using UT4MasterServer.Enums;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

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
	private readonly StatisticsService statisticsService;

	public UnrealTournamentStatsController(ILogger<UnrealTournamentStatsController> logger, StatisticsService statisticsService)
	{
		this.logger = logger;
		this.statisticsService = statisticsService;
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/daily
	[HttpGet("accountId/{id}/bulk/window/daily")]
	public async Task<IActionResult> GetDailyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatistics(accountId, StatisticWindow.Daily);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/weekly
	[HttpGet("accountId/{id}/bulk/window/weekly")]
	public async Task<IActionResult> GetWeeklyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatistics(accountId, StatisticWindow.Weekly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/monthly
	[HttpGet("accountId/{id}/bulk/window/monthly")]
	public async Task<IActionResult> GetMonthlyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatistics(accountId, StatisticWindow.Monthly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/alltime
	[HttpGet("accountId/{id}/bulk/window/alltime")]
	public async Task<IActionResult> GetAllTimeAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAllTimeAccountStatistics(accountId);
		return Ok(result);
	}

	// Example:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk?ownertype=1
	[HttpPost("accountId/{id}/bulk")]
	public async Task<IActionResult> CreateAccountStatistics(
		string id,
		[FromQuery] OwnerType ownerType,
		[FromBody] StatisticBulkDto statisticBulkDto)
	{
		var accountId = EpicID.FromString(id);

		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		if (user.Session.AccountID != accountId)
			return Unauthorized(); // Users can post their own stats only

		await statisticsService.CreateAccountStatistics(accountId, ownerType, statisticBulkDto);
		return Ok();
	}
}
