using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace UT4MasterServer.Models;

/// <summary>
/// Names taken from <see href="https://github.com/EpicGames/UnrealTournament/blob/clean-master/UnrealTournament/Source/UnrealTournament/Private/UTPlayerState.cpp#L2219">LeagueTierToText</see>
/// </summary>
public enum LeagueTier
{
	BronzeLeague = 0,
	SilverLeague = 1,
	GoldLeague = 2,
	PlatinumLeague = 3,
	MasterLeague = 4,
	GrandMasterLeague = 5,
}

public class League
{
	// i think this structure is in UT source, so it can be explored further there

	// TODO: Use one JSON DLL in solution. See https://github.com/timiimit/UT4MasterServer/issues/33
	[JsonPropertyName("tier")] // Fix for API response
	[JsonProperty("tier")]
	public LeagueTier Tier { get; set; } = LeagueTier.BronzeLeague;

	[JsonPropertyName("division")]
	[JsonProperty("division")]
	public byte Division { get; set; } = 0; // i assume this is the number on badge in stock scoreboard, so a number between 1-9 inclusive

	[JsonPropertyName("points")]
	[JsonProperty("points")]
	public int Points { get; set; } = 0; // i assume this is number of wins-loses in ranked league match

	[JsonPropertyName("isInPromotionSeries")]
	[JsonProperty("isInPromotionSeries")]
	public bool IsInPromotionSeries { get; set; } = false; // no clue

	[JsonPropertyName("promotionMatchesAttempted")]
	[JsonProperty("promotionMatchesAttempted")]
	public int PromotionMatchesAttempted { get; set; }

	[JsonPropertyName("promotionMatchesWon")]
	[JsonProperty("promotionMatchesWon")]
	public int PromotionMatchesWon { get; set; } = 0;

	[JsonPropertyName("placementMatchesAttempted")]
	[JsonProperty("placementMatchesAttempted")]
	public int PlacementMatchesAttempted { get; set; } = 0;
}
