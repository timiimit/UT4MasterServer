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

	public async Task<TrustedGameServer?> GetAsync(EpicID id)
	{
		var options = new FindOptions<TrustedGameServer>() { Limit = 1 };
		var cursor = await collection.FindAsync(x => x.ID == id, options);
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task UpdateAsync(TrustedGameServer server)
	{
		var options = new ReplaceOptions() { IsUpsert = true };
		await collection.ReplaceOneAsync(x => x.ID == server.ID, server, options);
	}

	public async Task<List<TrustedGameServer>> ListAsync()
	{
		var result = await collection.FindAsync(x => true);
		return await result.ToListAsync();
	}

	public async Task<bool?> RemoveAsync(EpicID id)
	{
		var result = await collection.DeleteOneAsync(x => x.ID == id);
		if (!result.IsAcknowledged)
			return null;

		return result.DeletedCount > 0;
	}
}
