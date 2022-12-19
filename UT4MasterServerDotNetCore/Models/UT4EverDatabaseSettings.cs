namespace UT4MasterServer.Models;

public class UT4EverDatabaseSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;

	public string AccountCollectionName { get; set; } = string.Empty;
	public string CodeCollectionName { get; set; } = string.Empty;
	public string SessionCollectionName { get; set; } = string.Empty;
}
