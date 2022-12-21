using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;
using MongoDB.Driver.Core.Operations;

namespace UT4MasterServer.Services;

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

	public async Task<Account?> GetAccountAsync(EpicID id)
	{
		var cursor = await accountCollection.FindAsync(account => account.ID == id);
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<Account?> GetAccountAsync(string username)
	{
		var cursor = await accountCollection.FindAsync(account => account.Username == username);
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<Account?> GetAccountAsync(string username, string password)
	{
		var cursor = await accountCollection.FindAsync(account =>
			account.Username == username &&
			account.Password == GetPasswordHash(password)
		);
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<List<Account>> GetAccountsAsync(List<EpicID> ids)
	{
		var result = await accountCollection.FindAsync(account => ids.Contains(account.ID));
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
