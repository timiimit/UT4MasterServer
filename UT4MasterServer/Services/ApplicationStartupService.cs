using Microsoft.Extensions.Options;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public class ApplicationStartupService : IHostedService
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
			await statisticsService.CreateIndexes();

			logger.LogInformation("Initializing MongoDB CloudStorage.");
			await cloudStorageService.UpdateSystemfiles();

			logger.LogInformation("Initializing MongoDB Clients.");
			await clientService.Update(new Client(
				ClientIdentification.Launcher.ID,
				ClientIdentification.Launcher.Secret,
				nameof(ClientIdentification.Launcher) + " (our website)"
			));
			await clientService.Update(new Client(
				ClientIdentification.Game.ID,
				ClientIdentification.Game.Secret,
				nameof(ClientIdentification.Game)
			));
			await clientService.Update(new Client(
				ClientIdentification.ServerInstance.ID,
				ClientIdentification.ServerInstance.Secret,
				nameof(ClientIdentification.ServerInstance)
			));
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
