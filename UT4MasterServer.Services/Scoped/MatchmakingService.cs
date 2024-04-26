using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Requests;
using UT4MasterServer.Common;
using Microsoft.Extensions.Logging;
using UT4MasterServer.Services.Singleton;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services.Scoped;

public sealed class MatchmakingService
{
	private readonly ILogger<MatchmakingService> logger;
	private readonly IMongoCollection<GameServer> serverCollection;
	private readonly TimeSpan StaleAfter = TimeSpan.FromMinutes(1);

	private readonly RuntimeInfoService runtimeInfoService;

	public MatchmakingService(DatabaseContext dbContext, ILogger<MatchmakingService> logger, RuntimeInfoService runtimeInfoService)
	{
		this.logger = logger;
		serverCollection = dbContext.Database.GetCollection<GameServer>("servers");
		this.runtimeInfoService = runtimeInfoService;
	}

	public async Task<bool> DoesExistWithSessionAsync(EpicID sessionID)
	{
		var options = new CountOptions() { Limit = 1 };
		var count = await serverCollection.CountDocumentsAsync(x => x.SessionID == sessionID, options);

		return count > 0;
	}

	public async Task<bool> DoesExistAsync(EpicID serverID)
	{
		var options = new CountOptions() { Limit = 1 };
		var count = await serverCollection.CountDocumentsAsync(x => x.ID == serverID, options);

		return count > 0;
	}

	public async Task<bool> AddAsync(GameServer server)
	{
		await serverCollection.InsertOneAsync(server);
		return true;
	}

	public async Task<bool> UpdateAsync(GameServer server)
	{
		var result = await serverCollection.ReplaceOneAsync(x => x.ID == server.ID, server);

		return result.IsAcknowledged;
	}

	public async Task UpdateTrustLevelAsync(EpicID clientID, GameServerTrust trustLevel)
	{
		var filter = Builders<GameServer>.Filter.Eq(x => x.OwningClientID, clientID);
		var update = Builders<GameServer>.Update.Set($"{nameof(GameServer.Attributes)}.{GameServerAttributes.UT_SERVERTRUSTLEVEL_i}", trustLevel);

		var result = await serverCollection.UpdateOneAsync(filter, update);
	}

	public async Task UpdateServerNameAsync(EpicID clientID, string serverName)
	{
		var filter = Builders<GameServer>.Filter.Eq(x => x.OwningClientID, clientID);
		var update = Builders<GameServer>.Update.Set($"{nameof(GameServer.Attributes)}.{GameServerAttributes.UT_SERVERNAME_s}", serverName);

		var result = await serverCollection.UpdateOneAsync(filter, update);
	}

	public async Task<bool> RemoveAsync(EpicID serverID)
	{
		var result = await serverCollection.DeleteOneAsync(x => x.ID == serverID);
		if (!result.IsAcknowledged)
		{
			return false;
		}

		return result.DeletedCount > 0;
	}

	public async Task<GameServer?> GetAsync(EpicID id)
	{
		var cursor = await serverCollection.FindAsync(x => x.ID == id);
		return await cursor.FirstOrDefaultAsync();
	}

	public async Task<List<GameServer>> ListAsync(GameServerFilterRequest inputFilter)
	{
		// Begin removing stale GameServers
		var taskStaleRemoval = RemoveAllStaleAsync();

		// Build BsonDocument representing Find filter
		var doc = new BsonDocument();

		//// include GameServers that have started
		//doc.Add(new BsonElement(nameof(GameServer.Started), true));

		if (DateTime.UtcNow - runtimeInfoService.StartupTime > StaleAfter)
		{
			// exclude stale GameServers that haven't been removed from db yet
			doc.Add(new BsonElement(nameof(GameServer.LastUpdated), new BsonDocument("$gt", DateTime.UtcNow - StaleAfter)));
		}
		else
		{
			// master server just started running. we don't know the status of servers.
			// assume everyone in db is still live and serve them to clients.
		}

		// include GameServers whose BuildUniqueId matches criteria
		if (inputFilter.BuildUniqueId != null)
		{
			doc.Add(new BsonElement(nameof(GameServer.BuildUniqueID), inputFilter.BuildUniqueId));
		}

		foreach (var condition in inputFilter.Criteria)
		{
			// TODO: use skipped conditions to query dynamic value and sort results
			//       (UTMatchmakingGather.cpp - search for SETTING_NEEDSSORT)
			if (condition.Key == "NEEDS" || condition.Key == "NEEDSSORT")
			{
				continue;
			}

			string? comparisonKeyword = null;
			switch (condition.Type)
			{
				case "EQUAL": comparisonKeyword = "$eq"; break;
				case "NOT_EQUAL": comparisonKeyword = "$ne"; break;
				case "LESS_THAN": comparisonKeyword = "$lt"; break;
				case "LESS_THAN_OR_EQUAL": comparisonKeyword = "$lte"; break;
				case "GREATER_THAN": comparisonKeyword = "$gt"; break;
				case "GREATER_THAN_OR_EQUAL": comparisonKeyword = "$gte"; break;
				case "DISTANCE":
					// TODO: Figure out what this should do (UTMatchmakingGather.cpp - search for SETTING_NEEDSSORT)

					break;
			}

			if (comparisonKeyword == null)
			{
				logger.LogWarning("Matchmaking search criteria contains unknown condition type '{Condition}' with key '{Key}' and value '{Value}'", condition.Type, condition.Key, condition.Value);
				continue;
			}

			BsonElement? compElem = null;
			if (condition.Value.ValueKind == JsonValueKind.String)
			{
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetString());
			}
			else if (condition.Value.ValueKind == JsonValueKind.Number)
			{
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetInt32());
			}
			else if (condition.Value.ValueKind == JsonValueKind.True || condition.Value.ValueKind == JsonValueKind.False)
			{
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetBoolean());
			}

			if (compElem != null)
			{
				var attrCheck = new BsonElement($"{nameof(GameServer.Attributes)}.{condition.Key}", new BsonDocument(compElem.Value));
				doc.Add(attrCheck);
			}
		}

		// Limit number of results. if request retrieves more results,
		// then caller should make filter more strict.
		if (inputFilter.MaxResults > 100)
		{
			inputFilter.MaxResults = 100;
		}

		var options = new FindOptions<GameServer>()
		{
			Limit = inputFilter.MaxResults,
			AllowPartialResults = true,
			MaxAwaitTime = TimeSpan.FromSeconds(1.0) // if this takes longer, server is toast
		};

		var filter = new BsonDocumentFilterDefinition<GameServer>(doc);

		// Wait for stale GameServer removal to finish
		await taskStaleRemoval;

		var cursor = await serverCollection.FindAsync(filter, options);
		var ret = await cursor.ToListAsync();

		if (inputFilter.OpenPlayersRequired != null)
		{
			// TODO: include this condition in db query
			ret.RemoveAll(x => x.MaxPublicPlayers - x.PublicPlayers.Count < inputFilter.OpenPlayersRequired);
		}

		// TODO: handle body.RequireDedicated and body.MaxCurrentPlayers

		return ret;
	}

	public async Task<int> RemoveAllStaleAsync()
	{
		var now = DateTime.UtcNow; // Use the same value for all checks in this call

		// Start removing stale servers only after some time has passed.
		// This allows game servers from before our reboot to send a heartbeat again and continue operating normally.
		if (runtimeInfoService.StartupTime > now - StaleAfter * 2)
		{
			return 0;
		}

		var result = await serverCollection.DeleteManyAsync(
			Builders<GameServer>.Filter.Lt(x => x.LastUpdated, now - StaleAfter)
		);

		if (result.IsAcknowledged)
		{
			return (int)result.DeletedCount;
		}
		return -1;
	}

	[Obsolete("SessionID can change without our knowledge while the match is going on. Consider using DoesClientOwnGameServerWithPlayerAsync instead.")]
	public async Task<bool> DoesSessionOwnGameServerWithPlayerAsync(EpicID sessionID, EpicID accountID)
	{
		var filterSession = Builders<GameServer>.Filter.Eq(x => x.SessionID, sessionID);
		var filterPrivatePlayers = Builders<GameServer>.Filter.AnyEq(x => x.PrivatePlayers, accountID);
		var filterPublicPlayers = Builders<GameServer>.Filter.AnyEq(x => x.PublicPlayers, accountID);
		var options = new CountOptions()
		{
			Limit = 1
		};

		var result = await serverCollection.CountDocumentsAsync(filterSession & (filterPrivatePlayers | filterPublicPlayers), options);
		return result > 0;
	}

	public async Task<bool> DoesClientOwnGameServerWithPlayerAsync(EpicID clientID, EpicID accountID)
	{
		var filterSession = Builders<GameServer>.Filter.Eq(x => x.OwningClientID, clientID);
		var filterPrivatePlayers = Builders<GameServer>.Filter.AnyEq(x => x.PrivatePlayers, accountID);
		var filterPublicPlayers = Builders<GameServer>.Filter.AnyEq(x => x.PublicPlayers, accountID);
		var options = new CountOptions()
		{
			Limit = 1
		};

		var result = await serverCollection.CountDocumentsAsync(filterSession & (filterPrivatePlayers | filterPublicPlayers), options);
		return result > 0;
	}
}
