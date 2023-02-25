using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common.Exceptions;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

public sealed class AccountService
{
	private readonly IMongoCollection<Account> accountCollection;
	private readonly ApplicationSettings applicationSettings;
	private readonly AwsSesClient awsSesClient;

	public AccountService(
		DatabaseContext dbContext,
		IOptions<ApplicationSettings> applicationSettings,
		AwsSesClient awsSesClient)
	{
		this.applicationSettings = applicationSettings.Value;
		accountCollection = dbContext.Database.GetCollection<Account>("accounts");
		this.awsSesClient = awsSesClient;
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
			Email = email,
			ActivationLinkGUID = Guid.NewGuid().ToString(),
			ActivationLinkExpiration = DateTime.UtcNow.AddMinutes(5),
		};
		newAccount.Password = PasswordHelper.GetPasswordHash(newAccount.ID, password);

		await accountCollection.InsertOneAsync(newAccount);

		await SendActivationLinkAsync(email, newAccount.ID, newAccount.ActivationLinkGUID);
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

	public async Task<Account?> GetAccountUsernameAndFlagsAsync(EpicID accountID)
	{
		var filter = Builders<Account>.Filter.Eq(x => x.ID, accountID);
		var options = new FindOptions<Account>()
		{
			Projection = Builders<Account>.Projection
				.Include(x => x.Username)
				.Include(x => x.Flags)
		};
		var result = await accountCollection.FindAsync(filter, options);
		return await result.FirstOrDefaultAsync();
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

	public async Task<List<EpicID>> GetNonVerifiedAccountsAsync()
	{
		var filter = (Builders<Account>.Filter.BitsAnyClear(f => f.Flags, (long)AccountFlags.EmailVerified) |
					  Builders<Account>.Filter.Exists(f => f.Flags, false)) &
					  Builders<Account>.Filter.Lt(f => f.LastLoginAt, DateTime.UtcNow.AddMonths(-1));
		var accountsIDs = await accountCollection.Find(filter).Project(p => p.ID).ToListAsync();
		return accountsIDs;
	}

	public async Task ActivateAccountAsync(EpicID accountID, string guid)
	{
		var filter = Builders<Account>.Filter.Eq(f => f.ID, accountID) &
					 Builders<Account>.Filter.Eq(f => f.ActivationLinkGUID, guid) &
					 Builders<Account>.Filter.Gt(f => f.ActivationLinkExpiration, DateTime.UtcNow) &
					(Builders<Account>.Filter.BitsAnyClear(f => f.Flags, (long)AccountFlags.EmailVerified) |
					 Builders<Account>.Filter.Exists(f => f.Flags, false));
		var account = await accountCollection.Find(filter).FirstOrDefaultAsync();

		if (account is null)
		{
			throw new AccountActivationException("Account activation failed: requested account not found, activation link expired or account in a wrong status.");
		}

		var updateDefinition = Builders<Account>.Update
			.BitwiseOr(s => s.Flags, AccountFlags.EmailVerified)
			.Unset(u => u.ActivationLinkGUID)
			.Unset(u => u.ActivationLinkExpiration);
		await accountCollection.UpdateOneAsync(filter, updateDefinition);
	}

	public async Task ResendActivationLinkAsync(string email)
	{
		var filter = Builders<Account>.Filter.Eq(f => f.Email, email) &
					(Builders<Account>.Filter.BitsAnyClear(f => f.Flags, (long)AccountFlags.EmailVerified) |
					 Builders<Account>.Filter.Exists(f => f.Flags, false));
		var account = await accountCollection.Find(filter).FirstOrDefaultAsync();

		if (account is null)
		{
			throw new NotFoundException("Email not found or already verified.");
		}

		var activationGUID = Guid.NewGuid().ToString();

		var updateDefinition = Builders<Account>.Update
			.Set(s => s.ActivationLinkGUID, activationGUID)
			.Set(s => s.ActivationLinkExpiration, DateTime.UtcNow.AddMinutes(5));
		await accountCollection.UpdateOneAsync(filter, updateDefinition);

		await SendActivationLinkAsync(email, account.ID, activationGUID);
	}

	public async Task InitiateResetPasswordAsync(string email)
	{
		var filter = Builders<Account>.Filter.Eq(f => f.Email, email) &
					 Builders<Account>.Filter.Eq(f => f.Status, AccountStatus.Active);
		var account = await accountCollection.Find(filter).FirstOrDefaultAsync();

		if (account is null)
		{
			throw new NotFoundException("Email not found or account in a wrong status.");
		}

		var guid = Guid.NewGuid().ToString();

		var updateDefinition = Builders<Account>.Update
			.Set(s => s.ResetLinkGUID, guid)
			.Set(u => u.ResetLinkExpiration, DateTime.UtcNow.AddMinutes(5));
		await accountCollection.UpdateOneAsync(filter, updateDefinition);

		await SendResetPasswordLinkAsync(email, account.ID, guid);
	}

	public async Task ResetPasswordAsync(EpicID accountID, string guid, string newPassword)
	{
		var filter = Builders<Account>.Filter.Eq(x => x.ID, accountID) &
					 Builders<Account>.Filter.Eq(x => x.ResetLinkGUID, guid) &
					 Builders<Account>.Filter.Gt(x => x.ResetLinkExpiration, DateTime.UtcNow);
		var account = await accountCollection.Find(filter).FirstOrDefaultAsync();

		if (account is null)
		{
			throw new NotFoundException("Requested account not found or reset link expired.");
		}

		newPassword = PasswordHelper.GetPasswordHash(accountID, newPassword);

		var filterForUpdate = Builders<Account>.Filter.Eq(x => x.ID, accountID);
		var update = Builders<Account>.Update
			.Set(x => x.Password, newPassword)
			.Unset(x => x.ResetLinkGUID)
			.Unset(x => x.ResetLinkExpiration);
		await accountCollection.UpdateOneAsync(filterForUpdate, update);
	}

	private async Task SendActivationLinkAsync(string email, EpicID accountID, string guid)
	{
		UriBuilder uriBuilder = new()
		{
			Scheme = applicationSettings.WebsiteScheme,
			Host = applicationSettings.WebsiteDomain,
			Port = applicationSettings.WebsitePort,
			Path = "ActivateAccount",
			Query = $"accountId={accountID}&guid={guid}"
		};

		var html = @$"
			<p>Welcome to UT4 Master Server!</p>
			<p>Click <a href='{uriBuilder.Uri}' target='_blank'>here</a> to activate your UT4 Master Server account.</p>
		";

		await awsSesClient.SendHTMLEmailAsync(applicationSettings.NoReplyEmail, new List<string>() { email }, "Account Activation", html);
	}

	private async Task SendResetPasswordLinkAsync(string email, EpicID accountID, string guid)
	{
		UriBuilder uriBuilder = new()
		{
			Scheme = applicationSettings.WebsiteScheme,
			Host = applicationSettings.WebsiteDomain,
			Port = applicationSettings.WebsitePort,
			Path = "ResetPassword",
			Query = $"accountId={accountID}&guid={guid}"
		};

		var html = @$"
			<p>Click <a href='{uriBuilder.Uri}' target='_blank'>here</a> to reset your password for UT4 Master Server account.</p>
			<p>If you didn't initiate password reset, ignore this message.</p>
		";

		await awsSesClient.SendHTMLEmailAsync(applicationSettings.NoReplyEmail, new List<string>() { email }, "Reset Password", html);
	}
}
