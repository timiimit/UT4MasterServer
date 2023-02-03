namespace UT4MasterServer.Services.Singleton;

public class RuntimeInfoService
{
	public DateTime StartupTime { get; set; }

	public RuntimeInfoService()
	{
		StartupTime = DateTime.UtcNow;
	}
}
