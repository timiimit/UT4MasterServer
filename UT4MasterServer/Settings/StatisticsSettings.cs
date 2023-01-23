namespace UT4MasterServer.Settings;

public sealed class StatisticsSettings
{
	/// <summary>
	/// Time zone in which deleting old statistics is executed
	/// </summary>
	public string DeleteOldStatisticsTimeZone { get; init; } = string.Empty;

	/// <summary>
	/// Hour at which deleting old statistics is executed
	/// </summary>
	public int DeleteOldStatisticsHour { get; init; }

	/// <summary>
	/// Number of days that statistic records are kept before deleted
	/// </summary>
	public int DeleteOldStatisticsBeforeDays { get; init; }

	/// <summary>
	/// Time zone in which merging old statistics is executed
	/// </summary>
	public string MergeOldStatisticsTimeZone { get; init; } = string.Empty;

	/// <summary>
	/// Hour at which merging old statistics is executed
	/// </summary>
	public int MergeOldStatisticsHour { get; init; }
}
