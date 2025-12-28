using Microsoft.Extensions.Logging;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer.Services.Scoped;

public sealed class CleanupService
{
	private readonly ILogger<CleanupService> logger;
	private readonly AccountService accountService;
	private readonly SessionService sessionService;
	private readonly CodeService codeService;
	private readonly FriendService friendService;
	private readonly CloudStorageService cloudStorageService;
	private readonly StatisticsService statisticsService;
	private readonly TrustedGameServerService trustedGameServerService;
	private readonly RatingsService ratingsService;

	public CleanupService(
		ILogger<CleanupService> logger,
		AccountService accountService,
		SessionService sessionService,
		CodeService codeService,
		FriendService friendService,
		CloudStorageService cloudStorageService,
		StatisticsService statisticsService,
		TrustedGameServerService trustedGameServerService,
		RatingsService ratingsService
)
	{
		this.logger = logger;
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.codeService = codeService;
		this.friendService = friendService;
		this.cloudStorageService = cloudStorageService;
		this.statisticsService = statisticsService;
		this.trustedGameServerService = trustedGameServerService;
		this.ratingsService = ratingsService;
	}

	public async Task RemoveNonVerifiedAccountsAsync()
	{
		var nonVerifiedAccountIds = await accountService.GetNonVerifiedAccountsAsync();
		await RemoveAccountAndAssociatedDataAsync(nonVerifiedAccountIds);
	}

	public async Task RemoveAccountAndAssociatedDataAsync(List<EpicID> accountIDs)
	{
		foreach (var accountID in accountIDs)
		{
			await RemoveAccountAndAssociatedDataAsync(accountID);
		}
	}

	public async Task RemoveAccountAndAssociatedDataAsync(EpicID accountID)
	{
		logger.LogInformation("Deleting account: {AccountID}.", accountID);

		await accountService.RemoveAccountAsync(accountID);
		await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, accountID, EpicID.Empty);
		await codeService.RemoveAllByAccountAsync(accountID);
		await cloudStorageService.RemoveAllByAccountAsync(accountID);
		await statisticsService.RemoveAllByAccountAsync(accountID);
		await ratingsService.RemoveAllByAccountAsync(accountID);
		await friendService.RemoveAllByAccountAsync(accountID);
		await trustedGameServerService.RemoveAllByAccountAsync(accountID);
		// NOTE: missing removal of account from live servers. this should take care of itself in a relatively short time.
	}
}
