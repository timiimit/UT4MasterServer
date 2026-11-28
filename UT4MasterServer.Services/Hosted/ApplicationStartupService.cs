using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Services.Hosted;

public sealed class ApplicationStartupService : IHostedService
{
	private readonly ILogger<ApplicationStartupService> logger;
	private readonly AccountService accountService;
	private readonly StatisticsService statisticsService;
	private readonly CloudStorageService cloudStorageService;
	private readonly ClientService clientService;
	private readonly RatingsService ratingsService;
	private readonly FriendService friendService;

	public ApplicationStartupService(
		ILogger<ApplicationStartupService> logger,
		ILogger<StatisticsService> statsLogger,
		IOptions<ApplicationSettings> settings,
		ILogger<CloudStorageService> cloudStorageLogger,
		ILogger<RatingsService> ratingsLogger)
	{
		this.logger = logger;
		var db = new DatabaseContext(settings);
		accountService = new AccountService(db, settings);
		statisticsService = new StatisticsService(statsLogger, db);
		cloudStorageService = new CloudStorageService(db, cloudStorageLogger);
		clientService = new ClientService(db);
		ratingsService = new RatingsService(ratingsLogger, db);
		friendService = new FriendService(db);
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Configuring MongoDB indexes.");
		await accountService.CreateIndexesAsync();
		await statisticsService.CreateIndexesAsync();
		await ratingsService.CreateIndexesAsync();

		// Must succeed before any friend write is served. Without the migration and the unique index,
		// friend requests could create duplicate records. A failure here stops startup on purpose.
		logger.LogInformation("Migrating friend records and configuring friends index.");
		await friendService.CreateIndexesAsync();

		logger.LogInformation("Initializing MongoDB CloudStorage.");
		await cloudStorageService.EnsureSystemFilesExistAsync();

		logger.LogInformation("Initializing MongoDB Clients.");
		await clientService.UpdateDefaultClientsAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
