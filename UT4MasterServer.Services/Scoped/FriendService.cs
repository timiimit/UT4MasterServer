using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common;

namespace UT4MasterServer.Services.Scoped;

public sealed class FriendService
{
	private readonly IMongoCollection<FriendRequest> friendCollection;

	public FriendService(DatabaseContext dbContext)
	{
		friendCollection = dbContext.Database.GetCollection<FriendRequest>("friends");
	}

	/// <summary>
	/// Creates the unique index that allows only one request or friendship per pair of accounts.
	/// Records written before <see cref="FriendRequest.PairKey"/> existed are migrated first:
	/// duplicates for the same pair are deleted, keeping one record.
	/// </summary>
	public async Task CreateIndexesAsync()
	{
		await MigratePairKeysAsync();

		// blocks have no PairKey, so they are not part of the index and stay independent per direction
		await friendCollection.Indexes.CreateOneAsync(new CreateIndexModel<FriendRequest>(
			Builders<FriendRequest>.IndexKeys.Ascending(x => x.PairKey),
			new CreateIndexOptions<FriendRequest>
			{
				Unique = true,
				PartialFilterExpression = Builders<FriendRequest>.Filter.Exists(x => x.PairKey)
			}));
	}

	private async Task MigratePairKeysAsync()
	{
		// nothing to do once every request and friendship carries a PairKey
		var legacyCount = await friendCollection.CountDocumentsAsync(
			x => x.Status != FriendStatus.Blocked && x.PairKey == null);
		if (legacyCount == 0)
		{
			return;
		}

		// FriendRequest has no ID member, read raw documents to get "_id" of each record
		IMongoCollection<BsonDocument> rawCollection = friendCollection.Database.GetCollection<BsonDocument>(
			friendCollection.CollectionNamespace.CollectionName);
		List<BsonDocument> documents = await rawCollection.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();

		var pairs = documents
			.Select(document => (ID: document["_id"], Record: BsonSerializer.Deserialize<FriendRequest>(document)))
			.Where(x => x.Record.Status != FriendStatus.Blocked)
			.GroupBy(x => GetPairKey(x.Record.Sender, x.Record.Receiver));

		foreach (var pair in pairs)
		{
			var records = pair.ToList();
			var keeper = records.Find(x => x.Record.Status == FriendStatus.Accepted);
			if (keeper.Record == null)
			{
				keeper = records[0];
			}

			// pending requests in both directions mean both accounts asked, that is a friendship
			var reciprocal = records.Any(x => x.Record.Sender != keeper.Record.Sender);
			FriendStatus status = reciprocal ? FriendStatus.Accepted : keeper.Record.Status;

			// Order matters, each step leaves the data recoverable if the next one never runs:
			// 1. save the derived status, so the acceptance decision survives once the reciprocal request is deleted
			if (status != keeper.Record.Status)
			{
				await friendCollection.UpdateOneAsync(
					Builders<FriendRequest>.Filter.Eq("_id", keeper.ID),
					Builders<FriendRequest>.Update.Set(x => x.Status, status));
			}

			// 2. delete duplicates
			foreach (var duplicate in records.Where(x => x.ID != keeper.ID))
			{
				await friendCollection.DeleteOneAsync(Builders<FriendRequest>.Filter.Eq("_id", duplicate.ID));
			}

			// 3. assign PairKey last, when no other record of the pair can collide with it in the unique index
			await friendCollection.UpdateOneAsync(
				Builders<FriendRequest>.Filter.Eq("_id", keeper.ID),
				Builders<FriendRequest>.Update.Set(x => x.PairKey, pair.Key));
		}
	}

	private static string GetPairKey(EpicID a, EpicID b)
	{
		var first = a.ToString();
		var second = b.ToString();
		return string.CompareOrdinal(first, second) <= 0 ? $"{first}:{second}" : $"{second}:{first}";
	}

	/// <summary>
	/// Turns a pending request from <paramref name="sender"/> to <paramref name="receiver"/> into a friendship.
	/// Single update, so it either happens fully or not at all.
	/// </summary>
	private async Task<bool> AcceptPendingRequestAsync(EpicID sender, EpicID receiver)
	{
		UpdateResult result = await friendCollection.UpdateOneAsync(
			x => x.Sender == sender && x.Receiver == receiver && x.Status == FriendStatus.Pending,
			Builders<FriendRequest>.Update.Set(x => x.Status, FriendStatus.Accepted));
		return result.MatchedCount > 0;
	}

	public async Task<bool> SendFriendRequestAsync(EpicID from, EpicID to)
	{
		// "to" has blocked "from", no relationship may be created
		var blockedByReceiver = await friendCollection.CountDocumentsAsync(
			x => x.Sender == to && x.Receiver == from && x.Status == FriendStatus.Blocked);
		if (blockedByReceiver > 0)
		{
			return false;
		}

		// remove outgoing block if it exists
		await friendCollection.DeleteManyAsync(
			x => x.Sender == from && x.Receiver == to && x.Status == FriendStatus.Blocked);

		// "to" already sent "from" a friend request, accept it
		if (await AcceptPendingRequestAsync(to, from))
		{
			return true;
		}

		// Create the pending request only if the pair has no request or friendship yet.
		// It is a single upsert on PairKey, so two concurrent calls cannot both insert.
		var pairKey = GetPairKey(from, to);
		var inserted = false;
		try
		{
			UpdateResult result = await friendCollection.UpdateOneAsync(
				x => x.PairKey == pairKey,
				Builders<FriendRequest>.Update
					.SetOnInsert(x => x.Sender, from)
					.SetOnInsert(x => x.Receiver, to)
					.SetOnInsert(x => x.Status, FriendStatus.Pending),
				new UpdateOptions { IsUpsert = true });
			inserted = result.UpsertedId != null;
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			// lost the race against a concurrent request for the same pair
		}

		if (!inserted)
		{
			// A record for the pair already exists.
			// If it is a request that "to" sent in the meantime, accept it instead of leaving it pending.
			await AcceptPendingRequestAsync(to, from);
		}

		return true;
	}

	public async Task<bool> CancelFriendRequestAsync(EpicID accountID, EpicID acceptsFrom)
	{
		// DeleteMany so that duplicates written before the unique index existed cannot leave a relationship behind
		DeleteResult? result = await friendCollection.DeleteManyAsync(x =>
			(x.Sender == accountID && x.Receiver == acceptsFrom ||
			 x.Sender == acceptsFrom && x.Receiver == accountID) &&
			x.Status != FriendStatus.Blocked);

		return result.IsAcknowledged; // TODO: is this correct return to confirm whether friend was removed?
	}

	public async Task<bool> BlockAccountAsync(EpicID accountID, EpicID blockedAccount)
	{
		// remove every request and friendship between the two, plus any earlier block of the same account.
		// a block made by the other account is kept.
		await friendCollection.DeleteManyAsync(x =>
			(x.Sender == accountID && x.Receiver == blockedAccount) ||
			(x.Sender == blockedAccount && x.Receiver == accountID && x.Status != FriendStatus.Blocked)
		);

		// create block request
		var friend = new FriendRequest
		{
			Sender = accountID,
			Receiver = blockedAccount,
			Status = FriendStatus.Blocked
		};

		await friendCollection.InsertOneAsync(friend);
		return true;
	}

	public async Task<bool> UnblockAccountAsync(EpicID accountID, EpicID unblockedAccount)
	{
		DeleteResult? result = await friendCollection.DeleteOneAsync(x =>
			x.Sender == accountID &&
			x.Receiver == unblockedAccount &&
			x.Status == FriendStatus.Blocked);

		return result.IsAcknowledged; // TODO: is this correct return to confirm whether friend was removed?
	}

	public async Task<List<FriendRequest>> GetFriendsAsync(EpicID accountID)
	{
		// get both pending and accepted friend requests
		IAsyncCursor<FriendRequest>? cursor = await friendCollection.FindAsync(x =>
			(x.Sender == accountID || x.Receiver == accountID) &&
			x.Status != FriendStatus.Blocked);
		return await cursor.ToListAsync();
	}

	public async Task<List<FriendRequest>> GetBlockedUsersAsync(EpicID accountID)
	{
		IAsyncCursor<FriendRequest>? cursor = await friendCollection.FindAsync(x =>
			x.Sender == accountID &&
			x.Status == FriendStatus.Blocked);
		return await cursor.ToListAsync();
	}

	/// <summary>
	/// Blocks in either direction: accounts blocked by <paramref name="accountID"/> and accounts that blocked it.
	/// </summary>
	public async Task<List<FriendRequest>> GetBlocksInvolvingAsync(EpicID accountID)
	{
		IAsyncCursor<FriendRequest>? cursor = await friendCollection.FindAsync(x =>
			(x.Sender == accountID || x.Receiver == accountID) &&
			x.Status == FriendStatus.Blocked);
		return await cursor.ToListAsync();
	}

	public async Task RemoveAllByAccountAsync(EpicID accountID)
	{
		await friendCollection.DeleteManyAsync(x => x.Sender == accountID || x.Receiver == accountID);
	}
}
