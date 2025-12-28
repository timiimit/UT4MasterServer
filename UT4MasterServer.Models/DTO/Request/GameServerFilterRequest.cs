using System.Text.Json;

namespace UT4MasterServer.Models.DTO.Requests;

public class GameServerAttributeCriteria
{
	public string Type { get; set; } = string.Empty;
	public string Key { get; set; } = string.Empty;

	public JsonElement Value { get; set; }
}

public class GameServerFilterRequest
{
	public List<GameServerAttributeCriteria> Criteria { get; set; } = new();
	public string? BuildUniqueId { get; set; }
	public int? OpenPlayersRequired { get; set; }
	public int? MaxCurrentPlayers { get; set; }
	public bool? RequireDedicated { get; set; }
	public int? MaxResults { get; set; }
}
