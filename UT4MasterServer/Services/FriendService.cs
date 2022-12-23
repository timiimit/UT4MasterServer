

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class FriendService
{
	private readonly IMongoCollection<FriendRequest> friendCollection;

	public FriendService(DatabaseContext dbContext, IOptions<DatabaseSettings> settings)
	{
		friendCollection = dbContext.Database.GetCollection<FriendRequest>("friends");
	}

	public async Task<bool> SendFriendRequestAsync(EpicID from, EpicID to)
	{
		bool friendRequestAccepted = false;

		// check if there is already inbound request
		var cursor = await friendCollection.FindAsync(x => x.Sender == to && x.Receiver == from && x.Status == FriendStatus.Pending);
		var friend = await cursor.FirstOrDefaultAsync();
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
			// if there was no inbound request initially, create a pending outgoing request
			friend = new FriendRequest();
			friend.Sender = from;
			friend.Receiver = to;
			friend.Status = FriendStatus.Pending;

			await friendCollection.InsertOneAsync(friend);
		}
		return true;
	}

	public async Task<bool> CancelFriendRequestAsync(EpicID accountID, EpicID acceptsFrom)
	{
		var result = await friendCollection.DeleteOneAsync(x =>
			((x.Sender == accountID && x.Receiver == acceptsFrom) ||
			(x.Sender == acceptsFrom && x.Receiver == accountID)) &&
			x.Status != FriendStatus.Blocked);

		return result.IsAcknowledged; // TODO: is this correct return to confirm whether friend was removed?
	}

	public async Task<bool> BlockAccountAsync(EpicID accountID, EpicID blockedAccount)
	{
		// remove any kind of connection
		await friendCollection.DeleteOneAsync(x =>
			(x.Sender == accountID && x.Receiver == blockedAccount) ||
			(x.Sender == blockedAccount && x.Receiver == accountID)
		);

		// create block request
		var friend = new FriendRequest();
		friend.Sender = accountID;
		friend.Receiver = blockedAccount;
		friend.Status = FriendStatus.Blocked;

		await friendCollection.InsertOneAsync(friend);
		return true;
	}

	public async Task<bool> UnblockAccountAsync(EpicID accountID, EpicID unblockedAccount)
	{
		var result = await friendCollection.DeleteOneAsync(x =>
			x.Sender == accountID &&
			x.Receiver == unblockedAccount &&
			x.Status == FriendStatus.Blocked);

		return result.IsAcknowledged; // TODO: is this correct return to confirm whether friend was removed?
	}

	public async Task<List<FriendRequest>> GetFriendsAsync(EpicID accountID)
	{
		// get both pending and accepted friend requests
		var cursor = await friendCollection.FindAsync(x =>
			(x.Sender == accountID || x.Receiver == accountID) &&
			x.Status != FriendStatus.Blocked);
		return await cursor.ToListAsync();
	}

	public async Task<List<FriendRequest>> GetBlockedUsersAsync(EpicID accountID)
	{
		var cursor = await friendCollection.FindAsync(x =>
			x.Sender == accountID &&
			x.Status == FriendStatus.Blocked);
		return await cursor.ToListAsync();
	}
}
