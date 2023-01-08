using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class DatabaseContext
{
	private readonly MongoClient client;
	public IMongoDatabase Database { get; private set; }

	public DatabaseContext(IOptions<ApplicationSettings> settings)
	{
		client = new MongoClient(settings.Value.DatabaseConnectionString);
		Database = client.GetDatabase(settings.Value.DatabaseName);
	}
}
