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

		public AccountService(IOptions<UT4EverDatabaseSettings> settings)
		{
			var mongoClient = new MongoClient(settings.Value.ConnectionString);
			var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
			accountCollection = mongoDatabase.GetCollection<Account>(settings.Value.AccountCollectionName);
		}

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



		private static string GetPasswordHash(string password)
		{
			var bytes = Encoding.UTF8.GetBytes(password);
			var hashedBytes = SHA512.HashData(bytes);
			var passwordHash = Convert.ToHexString(hashedBytes).ToLower();
			return passwordHash;
		}
	}
}
