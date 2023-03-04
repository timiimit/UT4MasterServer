using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer.Services.Hosted;

public sealed class ApplicationBackgroundCleanupService : IHostedService, IDisposable
{
	private readonly ILogger<ApplicationStartupService> logger;
	private readonly StatisticsSettings statisticsSettings;
	private readonly IServiceProvider services;

	private Timer? tmrExpiredSessionDeletor;
	private DateTime? lastDateDeleteOldStatisticsExecuted;
	private DateTime? lastDateMergeOldStatisticsExecuted;

	public ApplicationBackgroundCleanupService(
		ILogger<ApplicationStartupService> logger,
		IOptions<StatisticsSettings> statisticsSettings,
		IServiceProvider services)
	{
		this.logger = logger;
		this.services = services;
		this.statisticsSettings = statisticsSettings.Value;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
#if DEBUG
		var period = TimeSpan.FromMinutes(1);
#else
		var period = TimeSpan.FromMinutes(30);
#endif
		tmrExpiredSessionDeletor = new Timer(DoWork, null, TimeSpan.Zero, period);
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		tmrExpiredSessionDeletor?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}

	private void DoWork(object? state)
	{
		Task.Run(async () =>
		{
			using var scope = services.CreateScope();

			var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
			var deleteCount = await sessionService.RemoveAllExpiredSessionsAsync();
			if (deleteCount > 0)
				logger.LogInformation("Background task deleted {DeleteCount} expired sessions.", deleteCount);

			var codeService = scope.ServiceProvider.GetRequiredService<CodeService>();
			deleteCount = await codeService.RemoveAllExpiredCodesAsync();
			if (deleteCount > 0)
				logger.LogInformation("Background task deleted {DeleteCount} expired codes.", deleteCount);

			var matchmakingService = scope.ServiceProvider.GetRequiredService<MatchmakingService>();
			deleteCount = await matchmakingService.RemoveAllStaleAsync();
			if (deleteCount > 0)
				logger.LogInformation("Background task deleted {DeleteCount} stale game servers.", deleteCount);

			var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupService>();
			await cleanupService.RemoveNonVerifiedAccountsAsync();

			await DeleteOldStatisticsAsync(scope);
			await MergeOldStatisticsAsync(scope);
		});
	}

	public void Dispose()
	{
		tmrExpiredSessionDeletor?.Dispose();
	}

	#region Jobs

	/// <summary>
	/// This job is executed each day, and it deletes statistics older than X days including the flagged ones
	/// The days are specified in the appsettings
	/// </summary>
	private async Task DeleteOldStatisticsAsync(IServiceScope scope)
	{
		var currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(statisticsSettings.DeleteOldStatisticsTimeZone));
		if (currentTime.Hour == statisticsSettings.DeleteOldStatisticsHour &&
		   (!lastDateDeleteOldStatisticsExecuted.HasValue || lastDateDeleteOldStatisticsExecuted.Value.Date != currentTime.Date))
		{
			lastDateDeleteOldStatisticsExecuted = currentTime.Date;

			var statisticsService = scope.ServiceProvider.GetRequiredService<StatisticsService>();
			await statisticsService.DeleteOldStatisticsAsync(statisticsSettings.DeleteOldStatisticsBeforeDays, false);
		}
	}

	/// <summary>
	/// This job is executed each day, and it merges statistics older than 7 days into single record per day per account
	/// </summary>
	private async Task MergeOldStatisticsAsync(IServiceScope scope)
	{
		var currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(statisticsSettings.MergeOldStatisticsTimeZone));
		if (currentTime.Hour == statisticsSettings.MergeOldStatisticsHour &&
		   (!lastDateMergeOldStatisticsExecuted.HasValue || lastDateMergeOldStatisticsExecuted.Value.Date != currentTime.Date))
		{
			lastDateMergeOldStatisticsExecuted = currentTime.Date;

			var statisticsService = scope.ServiceProvider.GetRequiredService<StatisticsService>();
			await statisticsService.MergeOldStatisticsAsync();
		}
	}

	#endregion
}
