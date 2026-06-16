namespace UT4MasterServer.Models.DTO.Requests;

/// <summary>
/// Request body for POST /ut/api/matchmaking/quickplay.
///
/// Returns one running game-server suitable for a Quick-Play join.
/// Selection is server-side so the master server can steer Quick-Play
/// players onto a small curated pool of always-on, bot-filled servers
/// (humans displace bots as they join).
///
/// Mark a server as a Quick-Play target by setting the
/// <c>UT_RULETAG_s</c> game-server attribute (e.g. "QuickPlay_iDM",
/// "QuickPlay_CTF", "QuickPlay_DUEL"). Tags match the
/// <c>UniqueTag</c> values in <c>UnrealTournmentMCPGameRulesets.json</c>.
/// </summary>
public class QuickPlayRequest
{
	/// <summary>
	/// Ruleset tag the player wants to play. Matched against the
	/// <c>UT_RULETAG_s</c> attribute on candidate game servers. If null
	/// or empty, any tagged Quick-Play server is eligible.
	/// </summary>
	public string? RulesetTag { get; set; } = null;

	/// <summary>
	/// Optional map preference (matched against the <c>MAPNAME_s</c>
	/// attribute). If null, any map is acceptable.
	/// </summary>
	public string? PreferredMap { get; set; } = null;

	/// <summary>
	/// Optional build filter so old clients aren't routed to newer
	/// servers and vice-versa.
	/// </summary>
	public string? BuildUniqueId { get; set; } = null;
}
