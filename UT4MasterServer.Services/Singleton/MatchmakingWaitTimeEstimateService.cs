using UT4MasterServer.Models.DTO.Responses;

namespace UT4MasterServer.Services.Singleton;

public sealed class MatchmakingWaitTimeEstimateService
{
	private readonly Dictionary<string, List<(DateTime DeleteTime, double WaitTime)>> estimates;
	private static readonly TimeSpan RelevantReportTimeDuration = TimeSpan.FromMinutes(1);

	public MatchmakingWaitTimeEstimateService()
	{
		estimates = new Dictionary<string, List<(DateTime, double)>>();
	}

	public void AddWaitTime(string mode, double seconds)
	{
		if (!estimates.ContainsKey(mode))
		{
			estimates.Add(mode, new List<(DateTime, double)>());
		}

		estimates[mode].Add((DateTime.UtcNow + RelevantReportTimeDuration, seconds));
	}

	public List<WaitTimeEstimateResponse> GetWaitTimes()
	{
		Clean();

		var waitTimes = new List<WaitTimeEstimateResponse>();
		foreach (KeyValuePair<string, List<(DateTime DeleteTime, double WaitTime)>> estimate in estimates)
		{
			if (estimate.Value.Count <= 0)
			{
				continue;
			}

			var estimatedModeWait = estimate.Value.Average(x => x.WaitTime);
			waitTimes.Add(new WaitTimeEstimateResponse(estimate.Key, estimatedModeWait, estimate.Value.Count));
		}

		return waitTimes;
	}

	private void Clean()
	{
		DateTime now = DateTime.UtcNow;
		var modesToRemove = new List<string>();
		foreach (KeyValuePair<string, List<(DateTime DeleteTime, double WaitTime)>> mode in estimates)
		{
			mode.Value.RemoveAll(x => now > x.DeleteTime);
			if (mode.Value.Count <= 0)
			{
				modesToRemove.Add(mode.Key);
			}
		}

		foreach (var mode in modesToRemove)
		{
			estimates.Remove(mode);
		}
	}
}
