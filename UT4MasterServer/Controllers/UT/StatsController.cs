using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// ut-public-service-prod10.ol.epicgames.com
/// </summary>
[ApiController]
[Route("ut/api/stats")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class StatsController : JsonAPIController
{
	private readonly StatisticsService statisticsService;
	private readonly MatchmakingService matchmakingService;
	private readonly TrustedGameServerService trustedGameServerService;

	public StatsController(
		ILogger<StatsController> logger,
		StatisticsService statisticsService,
		MatchmakingService matchmakingService,
		TrustedGameServerService trustedGameServerService) : base(logger)
	{
		this.statisticsService = statisticsService;
		this.matchmakingService = matchmakingService;
		this.trustedGameServerService = trustedGameServerService;
	}

	[HttpGet("accountId/{id}/bulk/window/daily")]
	public async Task<IActionResult> GetDailyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		List<StatisticDTO>? result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Daily);
		return Ok(result);
	}

	[HttpGet("accountId/{id}/bulk/window/weekly")]
	public async Task<IActionResult> GetWeeklyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		List<StatisticDTO>? result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Weekly);
		return Ok(result);
	}

	[HttpGet("accountId/{id}/bulk/window/monthly")]
	public async Task<IActionResult> GetMonthlyAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		List<StatisticDTO>? result = await statisticsService.GetAggregateAccountStatisticsAsync(accountId, StatisticWindow.Monthly);
		return Ok(result);
	}

	[HttpGet("accountId/{id}/bulk/window/alltime")]
	public async Task<IActionResult> GetAllTimeAccountStatistics(string id)
	{
		var accountId = EpicID.FromString(id);
		List<StatisticDTO>? result = await statisticsService.GetAllTimeAccountStatisticsAsync(accountId);
		return Ok(result);
	}

	[HttpPost("accountId/{id}/bulk")]
	public async Task<IActionResult> CreateAccountStatistics(
		string id,
		[FromQuery] OwnerType ownerType,
		[FromBody] StatisticBase statisticBase)
	{
		var accountId = EpicID.FromString(id);

		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		if (user.Session.AccountID != accountId)
		{
			var isMultiplayerMatch = await matchmakingService.DoesClientOwnGameServerWithPlayerAsync(user.Session.ClientID, accountId);

			// NOTE: In debug we allow anyone to post stats for easier testing.
			//       Normally only trusted servers are allowed to post stats

#if !DEBUG
			if (!isMultiplayerMatch)
				return Unauthorized();
#endif

			TrustedGameServer? trusted = await trustedGameServerService.GetAsync(user.Session.ClientID);
			var isTrustedMatch = (int)(trusted?.TrustLevel ?? GameServerTrust.Untrusted) < 2;

#if !DEBUG
			if (!isTrustedMatch)
				return Unauthorized();
#endif
		}

		await statisticsService.CreateAccountStatisticsAsync(accountId, ownerType, statisticBase);
		return Ok();
	}
}
