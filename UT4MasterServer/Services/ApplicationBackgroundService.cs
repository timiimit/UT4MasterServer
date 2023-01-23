using Microsoft.Extensions.Options;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class ApplicationBackgroundCleanupService : IHostedService, IDisposable
{
	private readonly ILogger<ApplicationStartupService> logger;
	private readonly ApplicationSettings settings;
	private readonly IServiceProvider services;

	private Timer? tmrExpiredSessionDeletor;
	private DateTime? lastDateDeleteOldStatisticsExecuted;
	private DateTime? lastDateMergeOldStatisticsExecuted;

	public ApplicationBackgroundCleanupService(
		ILogger<ApplicationStartupService> logger,
		IOptions<ApplicationSettings> settings,
		IServiceProvider services
		)
	{
		this.logger = logger;
		this.settings = settings.Value;
		this.services = services;
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

			var matchmakingService = scope.ServiceProvider.GetRequiredService<MatchmakingService>();
			deleteCount = await matchmakingService.RemoveAllStaleAsync();
			if (deleteCount > 0)
				logger.LogInformation("Background task deleted {DeleteCount} stale game servers.", deleteCount);

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
		var currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(settings.DeleteOldStatisticsTimeZone));
		if (currentTime.Hour == settings.DeleteOldStatisticsHour &&
		   (!lastDateDeleteOldStatisticsExecuted.HasValue || lastDateDeleteOldStatisticsExecuted.Value.Date != currentTime.Date))
		{
			lastDateDeleteOldStatisticsExecuted = currentTime.Date;

			var statisticsService = scope.ServiceProvider.GetRequiredService<StatisticsService>();
			await statisticsService.DeleteOldStatisticsAsync(settings.DeleteOldStatisticsBeforeDays, false);
		}
	}

	/// <summary>
	/// This job is executed each day, and it merges statistics older than 7 days into single record per day per account
	/// </summary>
	private async Task MergeOldStatisticsAsync(IServiceScope scope)
	{
		var currentTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(settings.MergeOldStatisticsTimeZone));
		if (currentTime.Hour == settings.MergeOldStatisticsHour &&
		   (!lastDateMergeOldStatisticsExecuted.HasValue || lastDateMergeOldStatisticsExecuted.Value.Date != currentTime.Date))
		{
			lastDateMergeOldStatisticsExecuted = currentTime.Date;

			var statisticsService = scope.ServiceProvider.GetRequiredService<StatisticsService>();
			await statisticsService.MergeOldStatisticsAsync();
		}
	}

	#endregion
}
