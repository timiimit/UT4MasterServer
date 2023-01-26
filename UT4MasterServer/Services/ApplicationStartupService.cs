using Microsoft.Extensions.Options;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public sealed class ApplicationStartupService : IHostedService
	{
		private readonly ILogger<ApplicationStartupService> logger;
		private readonly StatisticsService statisticsService;
		private readonly CloudStorageService cloudStorageService;
		private readonly ClientService clientService;

		public ApplicationStartupService(
			ILogger<ApplicationStartupService> logger, ILogger<StatisticsService> statsLogger,
			IOptions<ApplicationSettings> settings)
		{
			this.logger = logger;
			var db = new DatabaseContext(settings);
			statisticsService = new StatisticsService(statsLogger, db);
			cloudStorageService = new CloudStorageService(db);
			clientService = new ClientService(db);
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Configuring MongoDB indexes.");
			await statisticsService.CreateIndexesAsync();

			logger.LogInformation("Initializing MongoDB CloudStorage.");
			await cloudStorageService.EnsureSystemfilesExistAsync();

			logger.LogInformation("Initializing MongoDB Clients.");
			await clientService.UpdateDefaultClientsAsync();
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
