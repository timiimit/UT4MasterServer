using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests;

public sealed class AccountSearchRequest
{
	[JsonPropertyName("query")]
	public string Query { get; set; } = string.Empty;

	[JsonPropertyName("roles")]
	public string[]? Roles { get; set; } = null;

	[JsonPropertyName("skip")]
	public int Skip { get; set; } = 0;

	[JsonPropertyName("take")]
	public int Take { get; set; } = 10;

	[JsonPropertyName("includeRoles")]
	public bool IncludeRoles { get; set; } = false;
}
