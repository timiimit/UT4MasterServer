namespace UT4MasterServer.Models
{
	public class UT4EverDatabaseSettings
	{
		public string ConnectionString { get; set; } = null!;
		public string DatabaseName { get; set; } = null!;
		public string AccountCollectionName { get; set; } = null!;
		public string SessionCollectionName { get; set; } = null!;
		public string AuthorizationCodeCollectionName { get; set; } = null!;
		public string ExchangeCodeCollectionName { get; set; } = null!;
	}
}
