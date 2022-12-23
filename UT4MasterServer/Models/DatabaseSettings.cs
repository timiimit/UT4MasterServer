namespace UT4MasterServer.Models;

public class DatabaseSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;

	public bool AllowPasswordGrantType { get; set; } = false;
}
