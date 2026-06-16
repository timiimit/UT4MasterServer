namespace UT4MasterServer.Models.Settings;

/// <summary>
/// SMTP / mail transport configuration. Populated from appsettings.json
/// (section "ApplicationSettings:Mail") or the equivalent env vars
/// (e.g. ApplicationSettings__Mail__Host).
/// </summary>
public sealed class MailSettings
{
	/// <summary>SMTP hostname (e.g. "mailpit" in dev, real SMTP host in prod).</summary>
	public string Host { get; set; } = string.Empty;

	/// <summary>SMTP port. 1025 for mailpit; 587 for STARTTLS; 465 for SMTPS.</summary>
	public int Port { get; set; } = 0;

	/// <summary>Auth username; leave empty for unauthenticated SMTP (e.g. mailpit).</summary>
	public string Username { get; set; } = string.Empty;

	/// <summary>Auth password; leave empty for unauthenticated SMTP.</summary>
	public string Password { get; set; } = string.Empty;

	/// <summary>Whether to use STARTTLS when connecting.</summary>
	public bool UseStartTls { get; set; } = false;

	/// <summary>Whether to use implicit TLS (SMTPS, usually port 465).</summary>
	public bool UseSsl { get; set; } = false;

	/// <summary>From: address on outbound mail.</summary>
	public string FromAddress { get; set; } = "noreply@ut4-hub.local";

	/// <summary>From: display name on outbound mail.</summary>
	public string FromName { get; set; } = "UT4 Master Server";

	/// <summary>
	/// When true (typical for dev), also log every outbound email body to the
	/// API stdout. Lets you grab the reset link from logs when SMTP isn't
	/// configured.
	/// </summary>
	public bool LogToConsole { get; set; } = false;

	/// <summary>True iff Host is non-empty (used by EmailService to decide whether to dispatch).</summary>
	public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && Port > 0;
}
