using System.Text.Json.Serialization;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class RatingMatch
{
	[JsonPropertyName("ratingType")]
	public string RatingType { get; set; } = string.Empty;

	[JsonPropertyName("matchInfo")]
	public RatingMatchInfo MatchInfo { get; set; } = new();

	[JsonPropertyName("redTeam")]
	public RatingTeam RedTeam { get; set; } = new();

	[JsonPropertyName("blueTeam")]
	public RatingTeam BlueTeam { get; set; } = new();
}

public sealed class RatingMatchInfo
{
	[JsonPropertyName("sessionId")]
	public EpicID ServerID { get; set; } = default;

	[JsonPropertyName("redScore")]
	public int RedScore { get; set; } = default;

	[JsonPropertyName("matchLengthSeconds")]
	public int MatchLength { get; set; } = default;
}

public sealed class RatingTeam
{
	public sealed class Member
	{
		[JsonPropertyName("accountId")]
		public string AccountID { get; set; } = string.Empty;

		[JsonPropertyName("score")]
		public int Score { get; set; } = 0;

		[JsonPropertyName("isBot")]
		public bool IsBot { get; set; } = false;
	}

	[JsonPropertyName("members")]
	public List<Member> Members { get; set; } = [];

	[JsonPropertyName("socialPartySize")]
	public int SocialPartySize { get; set; }
}
