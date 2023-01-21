using MongoDB.Driver;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public class TrustedGameServerService
{
	private readonly IMongoCollection<TrustedGameServer> collection;

	public TrustedGameServerService(DatabaseContext dbContext)
	{
		collection = dbContext.Database.GetCollection<TrustedGameServer>("trustedservers");
	}

	public async Task<TrustedGameServer?> Get(EpicID id)
	{
		var options = new FindOptions<TrustedGameServer>() { Limit = 1 };
		var cursor = await collection.FindAsync(x => x.ID == id, options);
		return await cursor.SingleOrDefaultAsync();
	}
}
