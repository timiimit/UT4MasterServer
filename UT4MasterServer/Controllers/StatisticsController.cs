using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.DTOs;
using UT4MasterServer.Enums;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("ut/api/stats")]
//[AuthorizeBearer]
[Produces("application/json")]
public sealed class StatisticsController : JsonAPIController
{
	private readonly ILogger<StatisticsController> logger;
	private readonly StatisticsService statisticsService;

	public StatisticsController(StatisticsService statisticsService, ILogger<StatisticsController> logger)
	{
		this.logger = logger;
		this.statisticsService = statisticsService;
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/daily
	[HttpGet("accountId/{accountId}/bulk/window/daily")]
	public async Task<IActionResult> GetDailyAccountStatistics(string accountId)
	{
		var result = await statisticsService.GetDailyAccountStatistics(accountId);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/weekly
	[HttpGet("accountId/{accountId}/bulk/window/weekly")]
	public async Task<IActionResult> GetWeeklyAccountStatistics(string accountId)
	{
		var result = await statisticsService.GetAggregateAccountStatistics(accountId, StatisticWindow.Weekly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/monthly
	[HttpGet("accountId/{accountId}/bulk/window/monthly")]
	public async Task<IActionResult> GetMonthlyAccountStatistics(string accountId)
	{
		var result = await statisticsService.GetAggregateAccountStatistics(accountId, StatisticWindow.Monthly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/alltime
	[HttpGet("accountId/{accountId}/bulk/window/alltime")]
	public async Task<IActionResult> GetAllTimeAccountStatistics(string accountId)
	{
		var result = await statisticsService.GetAllTimeAccountStatistics(accountId);
		return Ok(result);
	}

	// Example:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk?ownertype=1
	[HttpPost("{accountId}/bulk")]
	public async Task<IActionResult> CreateAccountStatistics(
		string accountId,
		[FromQuery] OwnerType ownerType,
		[FromBody] StatisticBulkDto statisticBulkDto)
	{
		await statisticsService.CreateAccountStatistics(accountId, ownerType, statisticBulkDto);
		return Ok();
	}
}
