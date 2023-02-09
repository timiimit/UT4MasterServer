using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class WaitTimeEstimateResponse
{
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
