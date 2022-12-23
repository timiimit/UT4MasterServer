using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection.Emit;
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
	}
}
