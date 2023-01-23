using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public sealed class ApplicationBackgroundCleanupService : IHostedService, IDisposable
	{
		private readonly ILogger<ApplicationStartupService> logger;
		private readonly IServiceProvider services;

		private Timer? tmrExpiredSessionDeletor;

		public ApplicationBackgroundCleanupService(
			ILogger<ApplicationStartupService> logger, ILogger<StatisticsService> statsLogger,
			IOptions<ApplicationSettings> settings,
			IServiceProvider services
			)
		{
			this.logger = logger;
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
				using (var scope = services.CreateScope())
				{
					var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
					var deleteCount = await sessionService.RemoveAllExpiredSessionsAsync();
					if (deleteCount > 0)
						logger.LogInformation("Background task deleted {DeleteCount} expired sessions.", deleteCount);

					var matchmakingService = scope.ServiceProvider.GetRequiredService<MatchmakingService>();
					deleteCount = await matchmakingService.RemoveAllStaleAsync();
					if (deleteCount > 0)
						logger.LogInformation("Background task deleted {DeleteCount} stale game servers.", deleteCount);
				}

			});
		}

		public void Dispose()
		{
			tmrExpiredSessionDeletor?.Dispose();
		}
	}
}
