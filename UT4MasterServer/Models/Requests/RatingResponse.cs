using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.Requests;

public sealed class RatingResponse
{
	[JsonPropertyName("rating")]
	public int Rating { get; set; }
}
