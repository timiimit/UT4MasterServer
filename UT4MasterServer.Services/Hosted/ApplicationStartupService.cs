using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Services.Hosted;

public sealed class ApplicationStartupService : IHostedService
{
	private readonly ILogger<ApplicationStartupService> logger;
	private readonly IServiceProvider serviceProvider;

	public ApplicationStartupService(ILogger<ApplicationStartupService> logger, IServiceProvider serviceProvider)
	{
		this.logger = logger;
		this.serviceProvider = serviceProvider;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();

		var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();
		var statisticsService = scope.ServiceProvider.GetRequiredService<StatisticsService>();
		var ratingsService = scope.ServiceProvider.GetRequiredService<RatingsService>();
		var cloudStorageService = scope.ServiceProvider.GetRequiredService<CloudStorageService>();
		var clientService = scope.ServiceProvider.GetRequiredService<ClientService>();

		logger.LogInformation("Configuring MongoDB indexes.");
		await accountService.CreateIndexesAsync();
		await statisticsService.CreateIndexesAsync();
		await ratingsService.CreateIndexesAsync();

		logger.LogInformation("Initializing MongoDB CloudStorage.");
		await cloudStorageService.EnsureSystemFilesExistAsync();

		logger.LogInformation("Initializing MongoDB Clients.");
		await clientService.UpdateDefaultClientsAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
