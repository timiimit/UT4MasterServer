using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization;

namespace UT4MasterServer.Services
{
	public class AccountService
	{
		private readonly IMongoCollection<Account> accountCollection;
		private readonly IMongoCollection<CommonCode> authorizationCodeCollection;
		private readonly IMongoCollection<CommonCode> exchangeCodeCollection;
		private readonly IMongoCollection<Session> sessionCollection;

		public AccountService(IOptions<UT4EverDatabaseSettings> settings)
		{
			var mongoClient = new MongoClient(settings.Value.ConnectionString);
			var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
			accountCollection = mongoDatabase.GetCollection<Account>(settings.Value.AccountCollectionName);
			authorizationCodeCollection = mongoDatabase.GetCollection<CommonCode>(settings.Value.AuthorizationCodeCollectionName);
			exchangeCodeCollection = mongoDatabase.GetCollection<CommonCode>(settings.Value.ExchangeCodeCollectionName);
			sessionCollection = mongoDatabase.GetCollection<Session>(settings.Value.SessionCollectionName);

			//BsonClassMap<Account>.GetRegisteredClassMaps()
			//BsonClassMap.RegisterClassMap<Account>(acc =>
			//{
			//	acc.AutoMap();
			//	acc.UnmapProperty(x => x.ID);
			//	acc.MapIdProperty(x => x.ID);
			//});
		}

		#region Accounts

		public async Task CreateAccountAsync(string username, string password)
		{
			var newAccount = new Account()
			{
				ID = EpicID.GenerateNew(),
				Username = username,
				Password = GetPasswordHash(password)
			};

			await accountCollection.InsertOneAsync(newAccount);
		}

		public async Task<Account> GetAccountAsync(EpicID id)
		{
			return await accountCollection.Find(account => account.ID == id).FirstOrDefaultAsync();
		}

		public async Task<Account> GetAccountAsync(string username)
		{
			return await accountCollection.Find(account => account.Username == username).FirstOrDefaultAsync();
		}

		public async Task<Account> GetAccountAsync(string username, string password)
		{
			return await accountCollection.Find(account =>
				account.Username == username &&
				account.Password == GetPasswordHash(password)
			).FirstOrDefaultAsync();
		}

		public async Task<List<Account>> GetAccountsAsync(List<EpicID> ids)
		{
			var filter = Builders<Account>.Filter.In("AccountID", ids);
			var result = await accountCollection.FindAsync(filter);
			return await result.ToListAsync();
		}

		public async Task UpdateAccountAsync(Account updatedAccount)
		{
			// we never want to change the ID, so ID can be implied from 'updatedAccount'
			await accountCollection.ReplaceOneAsync(user => user.ID == updatedAccount.ID, updatedAccount);
		}

		public async Task RemoveAccountAsync(EpicID id)
		{
			await accountCollection.DeleteOneAsync(user => user.ID == id);
		}

		#endregion

		#region Codes

		public async Task<CommonCode?> CreateAuthorizationCodeAsync(EpicID sessionID)
		{
			// sign in process - step 1
			// use session to create authorization code

			var session = await GetSessionAsync(sessionID);
			if (session == null)
				return null;

			var code = new CommonCode(session.AccountID, session.ClientID, Token.Generate(TimeSpan.FromMinutes(5)));

			await authorizationCodeCollection.InsertOneAsync(code);

			return code;
		}

		public async Task<CommonCode?> GetAuthorizationCodeAsync(string authorizationCode)
		{
			return await authorizationCodeCollection.Find(code => code.Token.Value == authorizationCode).FirstOrDefaultAsync();
		}

		public async Task<CommonCode?> CreateExchangeCodeAsync(EpicID sessionID)
		{
			// sign in process - step 3
			// use session to create exchange code

			var session = await GetSessionAsync(sessionID);
			if (session == null)
				return null;

			var code = new CommonCode(session.AccountID, session.ClientID, Token.Generate(TimeSpan.FromMinutes(5)));

			await exchangeCodeCollection.InsertOneAsync(code);

			return code;
		}

		public async Task<CommonCode?> GetExchangeCodeAsync(string authorizationCode)
		{
			return await exchangeCodeCollection.Find(code => code.Token.Value == authorizationCode).FirstOrDefaultAsync();
		}

		#endregion

		#region Sessions

		public async Task<Session?> CreateSessionWithCredentialsAsync(string username, string password)
		{
			var account = await GetAccountAsync(username, password);
			if (account == null)
				return null;

			// not sure if epic acts as if this is SessionCreationMethod.ClientCredentials, but it makes sense
			var session = new Session(EpicID.GenerateNew(), account.ID, ClientIdentification.Launcher.ID, SessionCreationMethod.ClientCredentials);
			await sessionCollection.InsertOneAsync(session);
			return session;
		}

		public async Task<Session?> CreateSessionWithAuthorizationCodeAsync(string authorizationCode, EpicID clientID)
		{
			// sign in process - step 2
			// use authorization code to create session inside new client

			var code = await GetAuthorizationCodeAsync(authorizationCode);
			if (code == null)
				return null;

			var session = new Session(EpicID.GenerateNew(), code.AccountID, clientID, SessionCreationMethod.AuthorizationCode);
			await sessionCollection.InsertOneAsync(session);
			return session;
		}

		public async Task<Session?> CreateSessionWithExchangeCodeAsync(string exchangeCode, EpicID clientID)
		{
			// sign in process - step 4
			// use exchange code to create session inside new client

			var code = await GetExchangeCodeAsync(exchangeCode);
			if (code == null)
				return null;

			var session = new Session(EpicID.GenerateNew(), code.AccountID, clientID, SessionCreationMethod.ExchangeCode);
			await sessionCollection.InsertOneAsync(session);
			return session;
		}

		public async Task<Session> GetSessionAsync(EpicID account, EpicID client)
		{
			return await sessionCollection.Find(session =>
				session.AccountID == account &&
				session.ClientID == client
			).FirstOrDefaultAsync();
		}

		public async Task<Session> GetSessionAsync(EpicID id)
		{
			return await sessionCollection.Find(session => session.ID == id).FirstOrDefaultAsync();
		}

		public async Task<Session> GetSessionAsync(string accessToken)
		{
			return await sessionCollection.Find(session => session.AccessToken.Value == accessToken).FirstOrDefaultAsync();
		}

		public async Task<List<Session>> GetSessionsAsync(List<EpicID> ids)
		{
			var filter = Builders<Session>.Filter.In("SessionID", ids);
			var result = await sessionCollection.FindAsync(filter);
			return await result.ToListAsync();
		}

		public async Task UpdateSessionAsync(Session updatedSession)
		{
			// we never want to change the ID, so ID can be implied from 'updatedSession'
			await sessionCollection.ReplaceOneAsync(session => session.ID == updatedSession.ID, updatedSession);
		}

		public async Task RemoveSessionAsync(EpicID id)
		{
			await sessionCollection.DeleteOneAsync(session => session.ID == id);
		}

		/// <summary>
		/// Removes all sessions on specified client except the one which requested this action
		/// </summary>
		/// <param name="clientID">client containing multiple sessions</param>
		/// <param name="sessionID">session to not remove</param>
		/// <returns></returns>
		public async Task RemoveOtherSessionsAsync(EpicID clientID, EpicID sessionID)
		{
			await sessionCollection.DeleteManyAsync(session => session.ClientID == clientID && session.ID != sessionID);
		}

		#endregion


		string GetPasswordHash(string password)
		{
			// hash password
			var bytes = Encoding.UTF8.GetBytes(password);
			var hashedBytes = SHA512.HashData(bytes);
			var passwordHash = Convert.ToHexString(hashedBytes);
			return passwordHash;
		}
	}
}
