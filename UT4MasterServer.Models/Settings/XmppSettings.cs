namespace UT4MasterServer.Models.Settings;

public sealed class XmppSettings
{
	/// <summary>
	/// Domain used in Xmpp as server identity.
	/// </summary>
	public string Domain { get; set; } = string.Empty;

	/// <summary>
	/// Hostname or IP address of the machine on which Xmpp server is running on
	/// </summary>
	public string Hostname { get; set; } = string.Empty;

	/// <summary>
	/// Port on which Xmpp server is listening on for master server events
	/// </summary>
	public int CommunicationPort { get; set; } = 52225;

	/// <summary>
	/// Location of the certificate that is used in Xmpp TLS encrypted tunnels
	/// </summary>
	public string CertificateLocation { get; set; } = string.Empty;

	/// <summary>
	/// Optional password to access certificate information
	/// </summary>
	public string? CertificatePassword { get; set; }
}
