using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public class MMRBulk
{
	[JsonPropertyName("ratingTypes")]
	public List<string> RatingTypes { get; set; } = new List<string>();

	[JsonPropertyName("ratings")]
	public List<int> Ratings { get; set; } = new List<int>();

	[JsonPropertyName("numGamesPlayed")]
	public List<int> NumGamesPlayed { get; set; } = new List<int>();
}

// TODO: MMRBulk in an ugly ut structure. Rather use something like an array of class below for storage:

public class ModeRating
{
	public string Type { get; set; } = "Unknown";
	public float Rating { get; set; } = 1500.0f;
	public int PlayCount { get; set; } = 0;
}
