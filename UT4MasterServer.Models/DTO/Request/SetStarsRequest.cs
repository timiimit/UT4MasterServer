using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Requests;

public sealed class SetStarsRequest
{
	[JsonPropertyName("newGoldStars")]
	public int NewGoldStars { get; set; }

	[JsonPropertyName("newBlueStars")]
	public int NewBlueStars { get; set; }
}
