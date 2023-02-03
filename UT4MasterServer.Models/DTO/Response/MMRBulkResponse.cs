using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public class MMRBulkResponse
{
	[JsonPropertyName("ratingTypes")]
	public List<string> RatingTypes { get; set; } = new List<string>();

	[JsonPropertyName("ratings")]
	public List<int> Ratings { get; set; } = new List<int>();

	[JsonPropertyName("numGamesPlayed")]
	public List<int> NumGamesPlayed { get; set; } = new List<int>();
}
