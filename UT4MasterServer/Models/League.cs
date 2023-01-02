using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace UT4MasterServer.Models;

public enum LeagueTier
{
	None = 0, // idk how this is named, its when you don't have colored badge on stock scoreboard
	Bronze = 1,
	Silver = 2,
	Gold = 3
}

public class League
{
	// i think this structure is in UT source, so it can be explored further there

	// TODO: Use one JSON DLL in solution. See https://github.com/timiimit/UT4MasterServer/issues/33
	[JsonPropertyName("tier")] // Fix for API response
	[JsonProperty("tier")]
	public LeagueTier Tier { get; set; } = LeagueTier.None;

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
