namespace UT4MasterServer.Models;

public class DatabaseSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;

	/// <remarks>
	/// Check comments in <see cref="Controllers.SessionController.Authenticate"/> for <c>password</c> <c>grant_type</c>
	/// </remarks>
	public bool AllowPasswordGrantType { get; set; } = false;
}
