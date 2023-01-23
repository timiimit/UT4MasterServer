namespace UT4MasterServer.Models;

public sealed class ApplicationSettings
{
	public string DatabaseConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;

	/// <remarks>
	/// Check comments in <see cref="Controllers.SessionController.Authenticate"/> for <c>password</c> <c>grant_type</c>
	/// </remarks>
	public bool AllowPasswordGrantType { get; set; } = false;

	/// <summary>
	/// Used just to redirect users to correct domain when UT4UU is being used.
	/// </summary>
	public string WebsiteDomain { get; set; } = string.Empty;

	/// <summary>
	/// File containing a list of trusted proxy servers (one per line).
	/// This file is loaded only once when program starts and it add values to <see cref="ProxyServers"/>.
	/// </summary>
	public string ProxyServersFile { get; set; } = string.Empty;

	/// <summary>
	/// Header that proxy server is expected to use to forward information about the original client IP address.
	/// </summary>
	public string ProxyClientIPHeader { get; set; } = string.Empty;

	/// <summary>
	/// IP addresses of trusted proxy servers.
	/// </summary>
	public List<string> ProxyServers { get; set; } = new List<string>();

	/// <summary>
	/// Time zone in which deleting old statistics is executed
	/// </summary>
	public string DeleteOldStatisticsTimeZone { get; set; } = string.Empty;

	/// <summary>
	/// Hour at which deleting old statistics is executed
	/// </summary>
	public int DeleteOldStatisticsHour { get; set; }

	/// <summary>
	/// Number of days that statistic records are kept before deleted
	/// </summary>
	public int DeleteOldStatisticsBeforeDays { get; set; }

	/// <summary>
	/// Time zone in which merging old statistics is executed
	/// </summary>
	public string MergeOldStatisticsTimeZone { get; set; } = string.Empty;

	/// <summary>
	/// Hour at which merging old statistics is executed
	/// </summary>
	public int MergeOldStatisticsHour { get; set; }
}
