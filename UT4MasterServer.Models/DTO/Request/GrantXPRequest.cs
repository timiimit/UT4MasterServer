using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Requests;

public sealed class GrantXPRequest
{
	[JsonPropertyName("xpAmount")]
	public int XPAmount { get; set; }
}
