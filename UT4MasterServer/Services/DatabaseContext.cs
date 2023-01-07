using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class DatabaseContext
{
	private readonly MongoClient client;
	public IMongoDatabase Database { get; private set; }

	public DatabaseContext(IOptions<DatabaseSettings> settings)
	{
		client = new MongoClient(settings.Value.ConnectionString);
		Database = client.GetDatabase(settings.Value.DatabaseName);

		var statisticsCollection = Database.GetCollection<Statistic>("statistics");
		var statisticsIndexes = new List<CreateIndexModel<Statistic>>()
		{
			new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.AccountId)),
			new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.CreatedAt))
		};
		statisticsCollection.Indexes.CreateMany(statisticsIndexes);
	}
}
