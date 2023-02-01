using Newtonsoft.Json;

namespace UT4MasterServer.Models;

public sealed class MMRBulk
{
	[JsonProperty("ratingTypes")]
	public List<string> RatingTypes { get; set; } = new List<string>();

	[JsonProperty("ratings")]
	public List<int> Ratings { get; set; } = new List<int>();

	[JsonProperty("numGamesPlayed")]
	public List<int> NumGamesPlayed { get; set; } = new List<int>();
}
