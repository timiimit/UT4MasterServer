using MongoDB.Driver;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public class SessionService
{
	private readonly IMongoCollection<Session> sessionCollection;

	public SessionService(DatabaseContext dbContext)
	{
		sessionCollection = dbContext.Database.GetCollection<Session>("sessions");
	}

	public async Task<Session?> CreateSessionAsync(EpicID accountID, EpicID clientID, SessionCreationMethod method)
	{
		var session = new Session(EpicID.GenerateNew(), accountID, clientID, method);

		bool isOnlyOneSessionAllowed =
			method == SessionCreationMethod.Password ||
			method == SessionCreationMethod.AuthorizationCode ||
			method == SessionCreationMethod.ExchangeCode;

		bool isInsertRequired = !isOnlyOneSessionAllowed;

		if (isOnlyOneSessionAllowed)
		{
			// since _id field is immutable, we need to delete old entry

			var filter = new ExpressionFilterDefinition<Session>(
				x => x.AccountID == accountID &&
				x.ClientID == clientID &&
				x.CreationMethod == method);

			await sessionCollection.DeleteManyAsync(filter);
		}

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
			s.RefreshToken.Value == refreshToken
		);
		var session = await cursor.SingleOrDefaultAsync();
		if (session == null)
			return null;

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
			Builders<Session>.Filter.Lt(x => x.RefreshToken.ExpirationTime, now);

		var expiredAccessToken =
			Builders<Session>.Filter.Lt(x => x.AccessToken.ExpirationTime, now);

		var result = await sessionCollection.DeleteManyAsync(expiredRefreshToken & expiredAccessToken);
		if (result.IsAcknowledged)
			return (int)result.DeletedCount;
		return -1;
	}



	private async Task<Session?> InvalidateExpiredSession(Session? session)
	{
		if (session == null)
			return null;

		if (!session.HasExpired)
			return session;

		await sessionCollection.DeleteOneAsync(s => s.AccessToken == session.AccessToken);
		return null;
	}
}
