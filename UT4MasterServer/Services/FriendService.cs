

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
		// TODO: logic in order of operation should be something like this:
		// - if "from" -> "to" is blocked, unblock
		// - if "to" -> "from" is pending, just accept it and return
		// - create "from" -> "to" pending friend request


		var friend = new FriendRequest();
		friend.Sender = from;
		friend.Receiver = to;
		friend.Status = FriendStatus.Pending;

		await friendCollection.InsertOneAsync(friend);
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
