using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Request;

public sealed class GrantXP
{
	[JsonPropertyName("xpAmount")]
	public int XPAmount { get; set; }
}
