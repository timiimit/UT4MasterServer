namespace UT4MasterServer.Services.Singleton;

/// <summary>
/// Simple in-memory sliding-window rate limiter for anonymous log uploads.
/// Chosen over an external store on purpose: if master dies, the counters
/// disappear, which is acceptable for abuse protection of a low-volume
/// endpoint (same reasoning as <see cref="CodeService"/> keeping codes in memory).
/// </summary>
public sealed class LogReportRateLimitService
{
	private const int CleanupThreshold = 1024;

	private readonly Dictionary<string, List<DateTime>> uploadsPerClient = new();

	/// <summary>
	/// Records an upload attempt for <paramref name="clientKey"/> and returns
	/// whether the attempt is allowed to proceed.
	/// </summary>
	public bool TryRecordUpload(string clientKey, int maxUploads, TimeSpan window)
	{
		var now = DateTime.UtcNow;

		lock (uploadsPerClient) // Make sure counters are thread-safe
		{
			if (uploadsPerClient.Count >= CleanupThreshold)
			{
				RemoveStaleClients(now, window);
			}

			if (!uploadsPerClient.TryGetValue(clientKey, out List<DateTime>? timestamps))
			{
				timestamps = new List<DateTime>();
				uploadsPerClient.Add(clientKey, timestamps);
			}

			timestamps.RemoveAll(x => now - x > window);

			if (timestamps.Count >= maxUploads)
			{
				return false;
			}

			timestamps.Add(now);
			return true;
		}
	}

	private void RemoveStaleClients(DateTime now, TimeSpan window)
	{
		var staleKeys = uploadsPerClient
			.Where(x => !x.Value.Any(t => now - t <= window))
			.Select(x => x.Key)
			.ToList();

		foreach (var key in staleKeys)
		{
			uploadsPerClient.Remove(key);
		}
	}
}
