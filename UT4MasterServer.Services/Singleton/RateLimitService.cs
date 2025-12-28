using Microsoft.Extensions.Caching.Memory;
using UT4MasterServer.Common.Exceptions;

namespace UT4MasterServer.Services.Singleton;

public sealed class RateLimitService
{
	private readonly IMemoryCache memoryCache;

	public RateLimitService(IMemoryCache memoryCache)
	{
		this.memoryCache = memoryCache;
	}

	public void CheckRateLimit(string key, int expirationInSeconds = 60)
	{
		if (memoryCache.TryGetValue<DateTime>(key, out var value))
		{
			throw new RateLimitExceededException($"Rate limit exceeded. Wait {value.Subtract(DateTime.UtcNow).Seconds} second(s) and try again.");
		}

		var expiration = DateTime.UtcNow.AddSeconds(expirationInSeconds);
		memoryCache.Set(key, expiration, TimeSpan.FromSeconds(expirationInSeconds));
	}
}
