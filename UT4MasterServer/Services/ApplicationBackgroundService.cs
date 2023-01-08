using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public class ApplicationBackgroundCleanupService : IHostedService, IDisposable
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
			tmrExpiredSessionDeletor = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
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
				int deleteCount = 0;

				using (var scope = services.CreateScope())
				{
					var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
					deleteCount = await sessionService.RemoveAllExpiredSessionsAsync();
				}

				logger.LogInformation($"Deleted {deleteCount} expired sessions in a background task.");
			});
		}

		public void Dispose()
		{
			tmrExpiredSessionDeletor?.Dispose();
		}
	}
}
