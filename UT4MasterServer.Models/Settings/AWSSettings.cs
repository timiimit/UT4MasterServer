namespace UT4MasterServer.Models.Settings;

public sealed class AWSSettings
{
	public string AccessKey { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	public string RegionName { get; set; } = string.Empty;
}
