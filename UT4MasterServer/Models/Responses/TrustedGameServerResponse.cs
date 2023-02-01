namespace UT4MasterServer.Models.Responses;

public class TrustedGameServerResponse: TrustedGameServer
{
	public Client? Client { get; set; } = null;
	public Account? Owner { get; set; } = null;
}
