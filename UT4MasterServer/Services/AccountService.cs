using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;

namespace UT4MasterServer.Services;

public class AccountService
{
	private readonly IMongoCollection<Account> accountCollection;
	private readonly bool allowPasswordGrant;

	public AccountService(DatabaseContext dbContext, IOptions<DatabaseSettings> settings)
	{
		accountCollection = dbContext.Database.GetCollection<Account>("accounts");
		allowPasswordGrant = settings.Value.AllowPasswordGrantType;
	}

	public async Task CreateAccountAsync(string username, string password)
	{
		var newAccount = new Account();
		newAccount.ID = EpicID.GenerateNew();
		newAccount.Username = username;
		newAccount.Password = GetPasswordHash(newAccount.ID, password);

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
		// look for account just with username
		var account = await GetAccountAsync(username);
		if (account == null)
			return null;

		// now verify that password is correct
		if (password != GetPasswordHash(account.ID, password))
		{
			if (!allowPasswordGrant)
				return null;

			// when user uses the website, password is never transmitted to us, only it's hash.
			// when user logs into the game via the stock in-game login window, password IS transmitted to us.
			// here we try to handle the latter, the less secure way of password transmission

			// put password into the form as it would be in, if it were transmitted from our website
			password = GetPasswordHash(password);

			// hash the password with 
			if (password != GetPasswordHash(account.ID, password))
				return null;
		}

		return account;
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



	private static string GetPasswordHash(EpicID accountID, string password)
	{
		// we combine both accountID and password to create a hash.
		// this way NO ONE can tell which users have the same password.
		string combined = accountID + password;

		return GetPasswordHash(combined);
	}

	private static string GetPasswordHash(string password)
	{
		var bytes = Encoding.UTF8.GetBytes(password);
		var hashedBytes = SHA512.HashData(bytes);
		var passwordHash = Convert.ToHexString(hashedBytes).ToLower();
		return passwordHash;
	}
}
