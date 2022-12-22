

using Newtonsoft.Json;

namespace UT4MasterServer.Models;

public enum LeagueTier
{
	None = 0, // idk how this is named, its when you dont have colored badge on stock scoreboard
	Bronze = 1,
	Silver = 2,
	Gold = 3
}

public class League
{
	// i think this structure is in UT source, so it can be explored further there

	[JsonProperty("tier")]
	public LeagueTier Tier { get; set; } = LeagueTier.None;

	[JsonProperty("division")]
	public byte Division { get; set; } = 0; // i assume this is the number on badge in stock scoreboard, so a number between 1-9 inclusive

	[JsonProperty("points")]
	public int Points { get; set; } = 0; // i assume this is number of wins-loses in ranked league match

	[JsonProperty("isInPromotionSeries")]
	public bool IsInPromotionSeries { get; set; } = false; // no clue

	[JsonProperty("promotionMatchesWon")]
	public int PromotionMatchesWon { get; set; } = 0;

	[JsonProperty("placementMatchesAttempted")]
	public int PlacementMatchesAttempted { get; set; } = 0;
}