using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class SessionService
{
	private readonly IMongoCollection<Code> codeCollection;
	private readonly IMongoCollection<Session> sessionCollection;

	public SessionService(IOptions<UT4EverDatabaseSettings> settings)
	{
		var mongoClient = new MongoClient(settings.Value.ConnectionString);
		var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
		codeCollection = mongoDatabase.GetCollection<Code>(settings.Value.CodeCollectionName);
		sessionCollection = mongoDatabase.GetCollection<Session>(settings.Value.SessionCollectionName);
	}

	#region Codes

	public async Task<Code?> CreateCodeAsync(CodeKind kind, EpicID accountID, EpicID clientID)
	{
		var code = new Code(accountID, clientID, Token.Generate(TimeSpan.FromMinutes(5)), kind);
		await codeCollection.InsertOneAsync(code);
		return code;
	}

	public async Task<Code?> TakeCodeAsync(CodeKind kind, string code)
	{
		return await codeCollection.FindOneAndDeleteAsync(x => x.Token.Value == code);
	}

	#endregion

	#region Sessions

	public async Task<Session?> CreateSessionAsync(EpicID accountID, EpicID clientID, SessionCreationMethod method)
	{
		var session = new Session(EpicID.GenerateNew(), accountID, clientID, method);
		await sessionCollection.InsertOneAsync(session);
		return session;
	}

	public async Task<Session?> GetSessionAsync(EpicID account, EpicID client)
	{
		var cursor = await sessionCollection.FindAsync(s =>
			s.AccountID == account &&
			s.ClientID == client
		);
		if (!cursor.Any())
			return null;

		return await InvalidateExpiredSession(await cursor.SingleAsync());
	}

	public async Task<Session?> GetSessionAsync(EpicID id)
	{
		var cursor = await sessionCollection.FindAsync(s => 
			s.ID == id
		);
		if (!cursor.Any())
			return null;
		return await InvalidateExpiredSession(await cursor.SingleAsync());
	}

	public async Task<Session?> GetSessionAsync(string accessToken)
	{
		var cursor = await sessionCollection.FindAsync(s => 
			s.AccessToken.Value == accessToken
		);
		if (!cursor.Any())
			return null;
		return await InvalidateExpiredSession(await cursor.SingleAsync());
	}

	public async Task<Session?> RefreshSessionAsync(string refreshToken)
	{
		var cursor = await sessionCollection.FindAsync(s =>
			s.RefreshToken.Value == refreshToken
		);
		if (!cursor.Any())
			return null;

		var session = await cursor.SingleAsync();
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

	/// <summary>
	/// Removes all sessions on specified client except the one which requested this action
	/// </summary>
	/// <param name="clientID">client containing multiple sessions</param>
	/// <param name="sessionID">session to not remove</param>
	/// <returns></returns>
	public async Task RemoveOtherSessionsAsync(EpicID clientID, EpicID sessionID)
	{
		await sessionCollection.DeleteManyAsync(x => x.ClientID == clientID && x.ID != sessionID);
	}

	public async Task<Session?> InvalidateExpiredSession(Session? session)
	{
		if (session == null)
			return null;

		if (!session.HasExpired)
			return session;

		await sessionCollection.DeleteOneAsync(s => s.AccessToken == session.AccessToken);
		return null;
	}

	#endregion
}
