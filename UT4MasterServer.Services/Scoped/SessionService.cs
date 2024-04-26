using MongoDB.Driver;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common;

namespace UT4MasterServer.Services.Scoped;

public sealed class SessionService
{
	private readonly IMongoCollection<Session> sessionCollection;

	public SessionService(DatabaseContext dbContext)
	{
		sessionCollection = dbContext.Database.GetCollection<Session>("sessions");
	}

	public async Task<Session?> CreateSessionAsync(EpicID accountID, EpicID clientID, SessionCreationMethod method)
	{
		var session = new Session(EpicID.GenerateNew(), accountID, clientID, method);

		// set number of allowed sessions
		int maxNumberOfSessions;
		bool limitSessionsPerClient;
		switch (method)
		{
			case SessionCreationMethod.AuthorizationCode:
			case SessionCreationMethod.Password:
				maxNumberOfSessions = 5;
				limitSessionsPerClient = false;
				break;
			case SessionCreationMethod.ClientCredentials:
				maxNumberOfSessions = -1;
				limitSessionsPerClient = true;
				break;
			case SessionCreationMethod.ExchangeCode:
				maxNumberOfSessions = 1;
				limitSessionsPerClient = true;
				break;
			default:
				maxNumberOfSessions = 1;
				limitSessionsPerClient = false;
				break;
		}

		if (maxNumberOfSessions > 0)
		{
			// find sessions that should be deleted

			var f = Builders<Session>.Filter;

			var filter =
				f.Eq(x => x.AccountID, accountID) &
				f.Eq(x => x.CreationMethod, method);

			if (limitSessionsPerClient)
			{
				filter &= f.Eq(x => x.ClientID, clientID);
			}

			var options = new FindOptions<Session>
			{
				// sort from newest to oldest
				Sort = Builders<Session>.Sort.Descending(x => x.RefreshToken!.ExpirationTime),

				// get only ID
				Projection = Builders<Session>.Projection.Include(x => x.ID),

				// skip those that are okay
				Skip = maxNumberOfSessions
			};

			var cursor = await sessionCollection.FindAsync(filter, options);
			var sessionsToDelete = await cursor.ToListAsync();

			// delete excess sessions
			if (sessionsToDelete.Count > 0)
			{
				await sessionCollection.DeleteManyAsync(f.In(x => x.ID, sessionsToDelete.Select(x => x.ID)));
			}
		}

		// create new session
		await sessionCollection.InsertOneAsync(session);

		return session;
	}

	public async Task<Session?> GetSessionAsync(EpicID account, EpicID client)
	{
		var cursor = await sessionCollection.FindAsync(s =>
			s.AccountID == account &&
			s.ClientID == client
		);
		return await InvalidateExpiredSession(await cursor.SingleOrDefaultAsync());
	}

	public async Task<Session?> GetSessionAsync(EpicID id)
	{
		var cursor = await sessionCollection.FindAsync(s => s.ID == id);
		return await InvalidateExpiredSession(await cursor.SingleOrDefaultAsync());
	}

	public async Task<Session?> GetSessionAsync(string accessToken)
	{
		var cursor = await sessionCollection.FindAsync(s =>
			s.AccessToken.Value == accessToken
		);
		return await InvalidateExpiredSession(await cursor.SingleOrDefaultAsync());
	}

	public async Task<Session?> RefreshSessionAsync(string refreshToken)
	{
		var cursor = await sessionCollection.FindAsync(s =>
			s.RefreshToken!.Value == refreshToken
		);
		var session = await cursor.SingleOrDefaultAsync();
		if (session == null)
		{
			return null;
		}

		session.Refresh();
		await UpdateSessionAsync(session);
		return session;
	}

	public async Task UpdateSessionAsync(Session updatedSession)
	{
		// we never want to change the ID, so ID can be implied from 'updatedSession'
		await sessionCollection.ReplaceOneAsync(x => x.ID == updatedSession.ID, updatedSession);
	}

	public async Task RemoveSessionAsync(EpicID id)
	{
		await sessionCollection.DeleteOneAsync(x => x.ID == id);
	}

	public async Task RemoveSessionsWithFilterAsync(EpicID includeClientID, EpicID includeAccountID, EpicID excludeSessionID)
	{
		await sessionCollection.DeleteManyAsync(x =>
			(includeClientID.IsEmpty || x.ClientID == includeClientID) &&
			(includeAccountID.IsEmpty || x.AccountID == includeAccountID) &&
			(excludeSessionID.IsEmpty || x.ID != excludeSessionID)
		);
	}

	public async Task<int> RemoveAllExpiredSessionsAsync()
	{
		var now = DateTime.UtcNow;

		var expiredRefreshToken =
			Builders<Session>.Filter.Exists(x => x.RefreshToken, false) |
			Builders<Session>.Filter.Lt(x => x.RefreshToken!.ExpirationTime, now);

		var expiredAccessToken =
			Builders<Session>.Filter.Lt(x => x.AccessToken.ExpirationTime, now);

		var result = await sessionCollection.DeleteManyAsync(expiredRefreshToken & expiredAccessToken);
		if (result.IsAcknowledged)
		{
			return (int)result.DeletedCount;
		}

		return -1;
	}



	private async Task<Session?> InvalidateExpiredSession(Session? session)
	{
		if (session == null)
		{
			return null;
		}

		if (!session.HasExpired)
		{
			return session;
		}

		await sessionCollection.DeleteOneAsync(s => s.AccessToken == session.AccessToken);
		return null;
	}
}
