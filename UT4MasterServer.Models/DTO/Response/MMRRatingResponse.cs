using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Response;

public sealed class MMRRatingResponse
{
	[JsonPropertyName("rating")]
	public int Rating { get; set; }

	[JsonPropertyName("numGamesPlayed")]
	public int GamesPlayed { get; set; }
}
