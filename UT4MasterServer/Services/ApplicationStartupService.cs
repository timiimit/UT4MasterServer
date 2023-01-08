using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
	public class ApplicationStartupService : IHostedService
	{
		private readonly ILogger<ApplicationStartupService> logger;
		private readonly MongoClient client;
		public IMongoDatabase Database { get; private set; }

		public ApplicationStartupService(ILogger<ApplicationStartupService> logger, IOptions<DatabaseSettings> settings)
		{
			this.logger = logger;
			client = new MongoClient(settings.Value.ConnectionString);
			Database = client.GetDatabase(settings.Value.DatabaseName);
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Configuring MongoDB indexes.");

			var statisticsCollection = Database.GetCollection<Statistic>("statistics");
			var statisticsIndexes = new List<CreateIndexModel<Statistic>>()
			{
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.AccountID)),
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.CreatedAt))
			};
			await statisticsCollection.Indexes.CreateManyAsync(statisticsIndexes, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
