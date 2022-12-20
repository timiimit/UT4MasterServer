

using Newtonsoft.Json;

namespace UT4MasterServer.Models;


public class MMRBulk
{
	[JsonProperty("ratingTypes")]
	public List<string> RatingTypes { get; set; } = new List<string>();

	[JsonProperty("ratings")]
	public List<int> Ratings { get; set; } = new List<int>();

	[JsonProperty("numGamesPlayed")]
	public List<int> PlayCount { get; set; } = new List<int>();
}

// MMRBulk in an ugly ut structure. rather use something
// like an array of class below for storage:

public class ModeRating
{
	public string Type { get; set; } = "Unknown";
	public float Rating { get; set; } = 1500.0f;
	public int PlayCount { get; set; } = 0;
}