using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class Rating
{
	[JsonPropertyName("rating")]
	public int RatingValue { get; set; }
}
