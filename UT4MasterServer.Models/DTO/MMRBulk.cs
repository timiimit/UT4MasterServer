using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO;

public class MMRBulk
{
	[JsonPropertyName("ratingTypes")]
	public List<string> RatingTypes { get; set; } = new List<string>();

	[JsonPropertyName("ratings")]
	public List<int> Ratings { get; set; } = new List<int>();

	[JsonPropertyName("numGamesPlayed")]
	public List<int> NumGamesPlayed { get; set; } = new List<int>();
}
