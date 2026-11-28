using MongoDB.Driver;
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

	public async Task<bool> SendFriendRequestAsync(EpicID from, EpicID to)
	{
		// "to" has blocked "from", no relationship may be created
		var blockedByReceiver = await friendCollection.CountDocumentsAsync(
			x => x.Sender == to && x.Receiver == from && x.Status == FriendStatus.Blocked);
		if (blockedByReceiver > 0)
		{
			return false;
		}

		var friendRequestAccepted = false;

		// check if there is already inbound request
		IAsyncCursor<FriendRequest>? cursor = await friendCollection.FindAsync(x => x.Sender == to && x.Receiver == from && x.Status == FriendStatus.Pending);
		FriendRequest? friend = await cursor.FirstOrDefaultAsync();
		if (friend != null && friend.Status == FriendStatus.Pending)
		{
			// "to" already sent "from" a friend request, accept it
			await friendCollection.UpdateOneAsync(
				x => x.Sender == to && x.Receiver == from,
				Builders<FriendRequest>.Update.Set(f => f.Status, FriendStatus.Accepted));
			friendRequestAccepted = true;
		}

		// remove outgoing block if it exists
		await friendCollection.DeleteOneAsync(
			x => x.Sender == from && x.Receiver == to && x.Status == FriendStatus.Blocked);

		if (!friendRequestAccepted)
		{
			// a request or friendship between the two already exists, do not create a second record
			var existing = await friendCollection.CountDocumentsAsync(x =>
				(x.Sender == from && x.Receiver == to ||
				 x.Sender == to && x.Receiver == from) &&
				x.Status != FriendStatus.Blocked);
			if (existing > 0)
			{
				return true;
			}

			// if there was no inbound request initially, create a pending outgoing request
			friend = new FriendRequest
			{
				Sender = from,
				Receiver = to,
				Status = FriendStatus.Pending
			};

			await friendCollection.InsertOneAsync(friend);
		}
		return true;
	}

	public async Task<bool> CancelFriendRequestAsync(EpicID accountID, EpicID acceptsFrom)
	{
		DeleteResult? result = await friendCollection.DeleteOneAsync(x =>
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
