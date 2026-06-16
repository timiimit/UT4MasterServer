using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class WaitTimeEstimateResponse
{
	// UTMcpUtils.cpp:186-223 reads JSON via Obj->GetStringField("ratingType"),
	// GetIntegerField("numSamples"), GetNumberField("averageWaitTimeSecs") —
	// strict camelCase. "Error: 1" is also raised when the response
	// Content-Type isn't exactly "application/json" (no charset suffix).
	[JsonPropertyName("ratingType")]
	public string RatingType { get; set; }

	[JsonPropertyName("averageWaitTimeSecs")]
	public double WaitTimeSeconds { get; set; }

	[JsonPropertyName("numSamples")]
	public int SampleCount { get; set; }

	public WaitTimeEstimateResponse(string ratingType, double seconds, int sampleCount)
	{
		RatingType = ratingType;
		WaitTimeSeconds = seconds;
		SampleCount = sampleCount;
	}
}
