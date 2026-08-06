namespace UT4MasterServer.Models.Settings;

/// <summary>
/// Optional overrides that let a <em>trusted</em> game server advertise a public
/// address different from the source IP of its registration request.
/// </summary>
/// <remarks>
/// This exists for game servers behind NAT / tunnels (playit.gg,
/// Cloudflare Spectrum, home servers behind CGNAT): they register from an
/// egress IP that is not the address players must connect to, so the browser
/// would otherwise list an unreachable address. Both mechanisms below are
/// opt-in and default OFF, so upstream behavior is unchanged unless an
/// operator explicitly enables them for a trusted server.
///
/// Bound from the top-level <c>Trusted</c> configuration section
/// (e.g. env vars <c>Trusted__AllowDeclaredAddress</c> and
/// <c>Trusted__AddressOverrides__&lt;sourceIP&gt;</c>).
/// </remarks>
public sealed class TrustedGameServerSettings
{
	/// <summary>
	/// When true, a trusted server may declare its own <c>serverAddress</c> in
	/// the registration request body and it will be honored instead of the
	/// request source IP, provided the declared value is a valid, non-empty,
	/// non-<c>"0.0.0.0"</c> IP address.
	/// </summary>
	public bool AllowDeclaredAddress { get; set; } = false;

	/// <summary>
	/// Static map of request source IP (egress) to public address (ingress).
	/// When a trusted server registers from a source IP present as a key here,
	/// the mapped value is advertised instead. Consulted only when
	/// <see cref="AllowDeclaredAddress"/> did not already override the address.
	/// </summary>
	public Dictionary<string, string> AddressOverrides { get; set; } = new();
}
