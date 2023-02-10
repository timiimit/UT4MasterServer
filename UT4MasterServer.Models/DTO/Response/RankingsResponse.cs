using UT4MasterServer.Common;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class RankingsResponse
{
	public int Rank { get; set; }
	public EpicID AccountID { get; set; }
	public string Player { get; set; } = string.Empty;
	public string CountryFlag { get; set; } = string.Empty;
	public int Rating { get; set; }
	public int GamesPlayed { get; set; }
}
