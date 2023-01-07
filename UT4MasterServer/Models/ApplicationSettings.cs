namespace UT4MasterServer.Models;

public class ApplicationSettings
{
	public string DatabaseConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;

	/// <remarks>
	/// Check comments in <see cref="Controllers.SessionController.Authenticate"/> for <c>password</c> <c>grant_type</c>
	/// </remarks>
	public bool AllowPasswordGrantType { get; set; } = false;

	/// <summary>
	/// File containing an IP addresses (one per line) trusted to be a proxy server.
	/// </summary>
	public string ProxyServersFile { get; set; } = string.Empty;

	/// <summary>
	/// Header that proxy server is expected to use to forward information about the original client IP address.
	/// </summary>
	public string ProxyClientIPHeader { get; set; } = string.Empty;

}
