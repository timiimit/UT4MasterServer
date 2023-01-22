using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Enums;
using UT4MasterServer.Models;
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
	private readonly StatisticsService statisticsService;
	private readonly MatchmakingService matchmakingService;

	public UnrealTournamentStatsController(
		ILogger<UnrealTournamentStatsController> logger,
		StatisticsService statisticsService,
		MatchmakingService matchmakingService) : base(logger)
	{
		this.statisticsService = statisticsService;
		this.matchmakingService = matchmakingService;
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/daily
	[HttpGet("accountId/{id}/bulk/window/daily")]
	public async Task<IActionResult> GetDailyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Daily);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/weekly
	[HttpGet("accountId/{id}/bulk/window/weekly")]
	public async Task<IActionResult> GetWeeklyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Weekly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/monthly
	[HttpGet("accountId/{id}/bulk/window/monthly")]
	public async Task<IActionResult> GetMonthlyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Monthly);
		return Ok(result);
	}

	// Examples:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk/window/alltime
	[HttpGet("accountId/{id}/bulk/window/alltime")]
	public async Task<IActionResult> GetAllTimeAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		var result = await statisticsService.GetAllTimeAccountStatisticsAsync(accountId);
		return Ok(result);
	}

	// Example:
	// /ut/api/stats/accountId/0b0f09b400854b9b98932dd9e5abe7c5/bulk?ownertype=1
	[HttpPost("accountId/{id}/bulk")]
	public async Task<IActionResult> CreateAccountStatistics(
		string id,
		[FromQuery] OwnerType ownerType,
		[FromBody] StatisticBase statisticBase)
	{
		var accountId = EpicID.FromString(id);

		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		if (user.Session.AccountID != accountId)
		{
			bool isMultiplayerMatch = await matchmakingService.DoesSessionOwnGameServerWithPlayerAsync(user.Session.ID, accountId);

#if DEBUG
			// in debug we allow anyone to post stats for easier testing
#else
			if (!isMultiplayerMatch)
				return Unauthorized(); // Only servers can post stats
#endif
		}

		await statisticsService.CreateAccountStatisticsAsync(accountId, ownerType, statisticBase);
		return Ok();
	}
}
