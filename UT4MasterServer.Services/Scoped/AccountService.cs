using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

public sealed class AccountService
{
	private readonly IMongoCollection<Account> accountCollection;

	public AccountService(DatabaseContext dbContext, IOptions<ApplicationSettings> settings)
	{
		accountCollection = dbContext.Database.GetCollection<Account>("accounts");
	}

	public async Task CreateIndexesAsync()
	{
		var indexKeys = Builders<Account>.IndexKeys;
		var indexes = new[]
		{
			new CreateIndexModel<Account>(indexKeys.Ascending(f => f.Username)),
			new CreateIndexModel<Account>(indexKeys.Ascending(f => f.Email))
		};
		await accountCollection.Indexes.CreateManyAsync(indexes);
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

	public async Task<PagedResponse<Account>> SearchAccountsAsync(string usernameQuery, AccountFlags flagsMask = (AccountFlags)~0, int skip = 0, int limit = 50)
	{
		FilterDefinition<Account> filter = new ExpressionFilterDefinition<Account>(
			account => account.Username.ToLower().Contains(usernameQuery.ToLower())
		);

		if (flagsMask != (AccountFlags)~0)
		{
			filter &= Builders<Account>.Filter.BitsAnySet(x => x.Flags, (long)flagsMask);
		}

		var options = new FindOptions<Account>()
		{
			Skip = skip,
			Limit = limit
		};

		var taskCount = accountCollection.CountDocumentsAsync(filter);
		var taskCursor = accountCollection.FindAsync(filter, options);

		var count = await taskCount;
		var cursor = await taskCursor;

		return new PagedResponse<Account>()
		{
			Data = await cursor.ToListAsync(),
			Count = count
		};
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

	[Obsolete("This should never be used due to being expensive")]
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

	public async Task<AccountFlags?> GetAccountFlagsAsync(EpicID accountID)
	{
		var filter = Builders<Account>.Filter.Eq(x => x.ID, accountID);
		var options = new FindOptions<Account>()
		{
			Projection = Builders<Account>.Projection.Include(x => x.Flags)
		};
		var result = await accountCollection.FindAsync(filter, options);
		var account = await result.FirstOrDefaultAsync();
		if (account is null)
		{
			return null;
		}

		return account.Flags;
	}

	public async Task UpdateAccountFlagsAsync(EpicID accountID, AccountFlags flags)
	{
		var filter = Builders<Account>.Filter.Eq(x => x.ID, accountID);
		var update = Builders<Account>.Update.Set(x => x.Flags, flags);
		await accountCollection.UpdateOneAsync(filter, update);
	}

	public async Task UpdateAccountPasswordAsync(EpicID accountID, string password)
	{
		password = PasswordHelper.GetPasswordHash(accountID, password);

		var filter = Builders<Account>.Filter.Eq(x => x.ID, accountID);
		var update = Builders<Account>.Update.Set(x => x.Password, password);
		await accountCollection.UpdateOneAsync(filter, update);
	}

	public async Task RemoveAccountAsync(EpicID id)
	{
		await accountCollection.DeleteOneAsync(user => user.ID == id);
	}
}

