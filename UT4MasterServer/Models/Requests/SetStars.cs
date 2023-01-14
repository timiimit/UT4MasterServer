using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests;

public sealed class SetStars
{
	[JsonPropertyName("newGoldStars")]
	public int NewGoldStars { get; set; }

	[JsonPropertyName("newBlueStars")]
	public int NewBlueStars { get; set; }
}
