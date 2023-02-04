using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Response;

public sealed class RatingResponse
{
	[JsonPropertyName("rating")]
	public int RatingValue { get; set; }
}
