using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public class ApplicationStartupService : IHostedService
	{
		private readonly ILogger<ApplicationStartupService> logger;
		private readonly StatisticsService statisticsService;
		private readonly CloudStorageService cloudStorageService;

		public ApplicationStartupService(
			ILogger<ApplicationStartupService> logger, ILogger<StatisticsService> statsLogger,
			IOptions<ApplicationSettings> settings)
		{
			this.logger = logger;
			var db = new DatabaseContext(settings);
			statisticsService = new StatisticsService(statsLogger, db);
			cloudStorageService = new CloudStorageService(db);
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Configuring MongoDB indexes.");
			await statisticsService.CreateIndexes();

			logger.LogInformation("Configuring CloudStorage.");
			await cloudStorageService.UpdateSystemfiles();
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
