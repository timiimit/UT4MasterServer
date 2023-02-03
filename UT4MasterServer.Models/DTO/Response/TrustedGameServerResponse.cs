using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Models.Responses;

public sealed class TrustedGameServerResponse : TrustedGameServer
{
	public Client? Client { get; set; } = null;
	public Account? Owner { get; set; } = null;
}
