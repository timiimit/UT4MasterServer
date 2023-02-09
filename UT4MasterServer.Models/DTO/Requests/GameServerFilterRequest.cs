using System.Text.Json;

namespace UT4MasterServer.Models.DTO.Requests;

public class GameServerAttributeCriteria
{
	public string Type { get; set; } = string.Empty;
	public string Key { get; set; } = string.Empty;

	public JsonElement Value { get; set; } = default;
}

public class GameServerFilterRequest
{
	public List<GameServerAttributeCriteria> Criteria { get; set; } = new();
	public string? BuildUniqueId { get; set; } = null;
	public int? OpenPlayersRequired { get; set; } = null;
	public int? MaxCurrentPlayers { get; set; } = null;
	public bool? RequireDedicated { get; set; } = null;
	public int? MaxResults { get; set; } = null;
}
