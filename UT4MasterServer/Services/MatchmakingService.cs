using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public class MatchmakingService
{
	private readonly ILogger<MatchmakingService> logger;
	private readonly IMongoCollection<GameServer> serverCollection;
	private TimeSpan StaleAfter = TimeSpan.FromMinutes(1);

	public MatchmakingService(DatabaseContext dbContext, ILogger<MatchmakingService> logger)
	{
		this.logger = logger;
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
		if (!result.IsAcknowledged)
			return false;

		return result.DeletedCount > 0;
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
		// Begin removing stale GameServers
		var taskStaleRemoval = RemoveStale();

		// Build BsonDocument representing Find filter
		var doc = new BsonDocument();

		// include GameServers that have started
		doc.Add(new BsonElement(nameof(GameServer.Started), true));

		// exclude stale GameServers that haven't been removed from db yet
		doc.Add(new BsonElement(nameof(GameServer.LastUpdated), new BsonDocument("$gt", DateTime.UtcNow - StaleAfter)));

		// include GameServers whose BuildUniqueId matches criteria
		if (inputFilter.BuildUniqueId != null)
			doc.Add(new BsonElement(nameof(GameServer.BuildUniqueID), inputFilter.BuildUniqueId));

		if (inputFilter.OpenPlayersRequired != null)
		{
			// TODO: The following expression was not tested

			// PublicPlayers.Count
			var publicPlayerCount = new BsonDocument("$count", "$PublicPlayers");

			// [ MaxPublicPlayers, PublicPlayers.Count ]
			var subtractedValues = new BsonArray(new BsonValue[] { "$MaxPublicPlayers", publicPlayerCount });

			// MaxPublicPlayers - PublicPlayers.Count
			var subtraction = new BsonDocument("$subtract", subtractedValues);

			// [ MaxPublicPlayers - PublicPlayers.Count, OpenPlayersRequired ]
			var comparedValues = new BsonArray(new BsonValue[] { subtraction, inputFilter.OpenPlayersRequired });

			// (MaxPublicPlayers - PublicPlayers.Count) >= OpenPlayersRequired
			var comparison = new BsonDocument("$gte", comparedValues);

			// create final expression
			doc.Add(new BsonElement("$expr", comparison));
		}

		foreach (var condition in inputFilter.Criteria)
		{
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
					// TODO: Figure out what this should do. the only known occurance:
					//       { "type": "DISTANCE", "key": "NEEDSSORT_i", "value": -2147483648 },

					break;
			}

			if (comparisonKeyword == null)
			{
				logger.LogWarning($"Matchmaking search criteria contains unknown condition type '{condition.Type}' with key '{condition.Key}' and value '{condition.Value}'");
				continue;
			}

			BsonElement? compElem = null;
			if (condition.Value.ValueKind == JsonValueKind.String)
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetString());
			else if (condition.Value.ValueKind == JsonValueKind.Number)
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetInt32());
			else if (condition.Value.ValueKind == JsonValueKind.True || condition.Value.ValueKind == JsonValueKind.False)
				compElem = new BsonElement(comparisonKeyword, condition.Value.GetBoolean());

			if (compElem != null)
			{
				var attrCheck = new BsonElement($"{nameof(GameServer.Attributes)}.{condition.Key}", new BsonDocument(compElem.Value));
				doc.Add(attrCheck);
			}
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
		return await cursor.ToListAsync();
	}

	public async Task<int> RemoveStale()
	{
		var now = DateTime.UtcNow; // Use the same value for all checks in this call

		// Start removing stale servers only after some time has passed.
		// This allows game servers from before reboot to send a heartbeat again and continue operating normally.
		if (Program.StartupTime < now - StaleAfter * 2)
			return 0;

		var result = await serverCollection.DeleteManyAsync(
			Builders<GameServer>.Filter.Lt(x => x.LastUpdated, now - StaleAfter)
		);

		if (result.IsAcknowledged)
		{
			return (int)result.DeletedCount;
		}
		return -1;
	}
}
