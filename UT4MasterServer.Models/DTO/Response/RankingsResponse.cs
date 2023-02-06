using UT4MasterServer.Common;

namespace UT4MasterServer.Models.DTO.Response;

public sealed class RankingsResponse
{
	public int Rank { get; set; }
	public EpicID AccountID { get; set; }
	public string Player { get; set; } = string.Empty;
	public int Rating { get; set; }
	public int GamesPlayed { get; set; }
}
