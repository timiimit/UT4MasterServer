using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

public sealed class AccountService
{
	private readonly IMongoCollection<Account> accountCollection;

	public AccountService(DatabaseContext dbContext, IOptions<ApplicationSettings> settings)
	{
		accountCollection = dbContext.Database.GetCollection<Account>("accounts");
	}

	public async Task CreateAccountAsync(string username, string email, string password)
	{
		var newAccount = new Account
		{
			ID = EpicID.GenerateNew(),
			Username = username,
			Email = email
		};
		newAccount.Password = PasswordHelper.GetPasswordHash(newAccount.ID, password);

		await accountCollection.InsertOneAsync(newAccount);
	}

	public async Task<Account?> GetAccountByEmailAsync(string email)
	{
		var cursor = await accountCollection.FindAsync(account => account.Email == email);
		return await cursor.SingleOrDefaultAsync();
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

	public async Task<IEnumerable<Account>> SearchAccountsAsync(string usernameQuery)
	{
		var cursor = await accountCollection.FindAsync(account => account.Username.ToLower().Contains(usernameQuery.ToLower()));
		return await cursor.ToListAsync();
	}

	public async Task<Account?> GetAccountUsernameOrEmailAsync(string username)
	{
		var account = await GetAccountAsync(username);
		if (account == null)
		{
			account = await GetAccountByEmailAsync(username);
			if (account == null)
				return null;
		}

		return account;
	}

	public async Task<IEnumerable<Account>> GetAccountsAsync(IEnumerable<EpicID> ids)
	{
		var result = await accountCollection.FindAsync(account => ids.Contains(account.ID));
		return await result.ToListAsync();
	}

	public async Task<IEnumerable<Account>> GetAllAccountsAsync()
	{
		var result = await accountCollection.FindAsync(account => true);
		return await result.ToListAsync();
	}

	public async Task UpdateAccountAsync(Account updatedAccount)
	{
		// we never want to change the ID, so ID can be implied from 'updatedAccount'
		await accountCollection.ReplaceOneAsync(user => user.ID == updatedAccount.ID, updatedAccount);
	}

	public async Task UpdateAccountPasswordAsync(Account updatedAccount, string password)
	{
		updatedAccount.Password = PasswordHelper.GetPasswordHash(updatedAccount.ID, password);
		await accountCollection.ReplaceOneAsync(user => user.ID == updatedAccount.ID, updatedAccount);
	}

	public async Task RemoveAccountAsync(EpicID id)
	{
		await accountCollection.DeleteOneAsync(user => user.ID == id);
	}
}

