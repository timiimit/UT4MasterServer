using MongoDB.Driver;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public class MatchmakingService
{
	private readonly IMongoCollection<GameServer> serverCollection;

	public MatchmakingService(DatabaseContext dbContext)
	{
		serverCollection = dbContext.Database.GetCollection<GameServer>("servers");
	}

	public async Task<bool> Add(GameServer server)
	{
		var options = new CountOptions() { Limit = 1 };
		long count = await serverCollection.CountDocumentsAsync(x => x.SessionID == server.SessionID, options);

		// each session is only allowed a single server
		if (count > 0)
			return false;

		await serverCollection.InsertOneAsync(server);
		return true;
	}

	public async Task<bool> Update(GameServer server)
	{
		var result = await serverCollection.ReplaceOneAsync(x => x.SessionID == server.SessionID && x.ID == server.ID, server);

		return result.IsAcknowledged;
	}

	public async Task<bool> Remove(EpicID sessionID, EpicID serverID)
	{
		var result = await serverCollection.DeleteOneAsync(x => x.SessionID == sessionID && x.ID == serverID);
		return result.IsAcknowledged;
	}

	public async Task<GameServer?> Get(EpicID sessionID, EpicID serverID)
	{
		var server = await Get(sessionID);
		if (server == null)
			return null;

		if (server.ID != serverID)
			return null;

		return server;
	}

	public async Task<GameServer?> Get(EpicID sessionID)
	{
		var cursor = await serverCollection.FindAsync(x => x.SessionID == sessionID);
		return await cursor.FirstOrDefaultAsync();
	}

	public async Task<List<GameServer>> List(GameServerFilter inputFilter)
	{
		await RemoveStale();

		var filter =
			Builders<GameServer>.Filter.Eq(x => x.Started, true);

		if (inputFilter.BuildUniqueId != null)
			filter &= Builders<GameServer>.Filter.Eq(x => x.BuildUniqueID, inputFilter.BuildUniqueId);

		//if (inputFilter.OpenPlayersRequired != null)
		//	filter &= Builders<GameServer>.Filter.Eq(x => x.MaxPublicPlayers - x.PublicPlayers.Count, inputFilter.OpenPlayersRequired);

		foreach (var condition in inputFilter.Criteria)
		{
			filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Contains(condition.Key), true);

			switch (condition.Type)
			{
				case "EQUAL":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Eq(condition.Key, condition.Value), true);
					break;
				case "NOT_EQUAL":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Eq(condition.Key, condition.Value), false);
					break;
				case "LESS_THAN":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Lt(condition.Key, condition.Value), true);
					break;
				case "LESS_THAN_OR_EQUAL":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Lte(condition.Key, condition.Value), true);
					break;
				case "GREATER_THAN":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Lte(condition.Key, condition.Value), false);
					break;
				case "GREATER_THAN_OR_EQUAL":
					filter &= Builders<GameServer>.Filter.Eq(x => x.Attributes.Lt(condition.Key, condition.Value), false);
					break;
			}
		}

		var options = new FindOptions<GameServer>()
		{
			Limit = inputFilter.MaxResults,
			AllowPartialResults = true,
			//MaxAwaitTime = TimeSpan.FromSeconds(0.1)
		};
		var cursor = await serverCollection.FindAsync(filter, options);
		return await cursor.ToListAsync();
	}

	public async Task<int> RemoveStale()
	{
		var now = DateTime.UtcNow; // use the same value for all checks in this call
		var staleAfter = TimeSpan.FromMinutes(1);
#if !DEBUG
		staleAfter = TimeSpan.FromMinutes(5);
#endif

		var result = await serverCollection.DeleteManyAsync(x => x.LastUpdated < now - staleAfter);
		if (result.IsAcknowledged)
		{
			return (int)result.DeletedCount;
		}
		return -1;
	}
}
