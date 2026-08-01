namespace UT4MasterServer.Models.Settings;

public sealed class LogReportSettings
{
	/// <summary>
	/// Name of the GridFS bucket in which uploaded log files are stored.
	/// </summary>
	public string BucketName { get; set; } = "bugreports";

	/// <summary>
	/// Maximum allowed size of a single uploaded log file in bytes.
	/// </summary>
	public int MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;

	/// <summary>
	/// Maximum number of uploads a single client IP may perform within <see cref="RateLimitWindowMinutes"/>.
	/// </summary>
	public int RateLimitMaxUploads { get; set; } = 10;

	/// <summary>
	/// Size of the sliding rate-limit window in minutes.
	/// </summary>
	public int RateLimitWindowMinutes { get; set; } = 60;
}
