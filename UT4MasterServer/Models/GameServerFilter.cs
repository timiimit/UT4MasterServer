/*

			{
				"criteria": [
					{
						"type": "NOT_EQUAL",
						"key": "UT_GAMEINSTANCE_i",
						"value": 1
					},
					{
						"type": "NOT_EQUAL",
						"key": "UT_RANKED_i",
						"value": 1
					}
				],
				"buildUniqueId": "256652735",
				"maxResults": 10000
			}


*/

using System.Text.Json;

namespace UT4MasterServer.Models;

public class GameServerAttributeCriteria
{
	public string Type { get; set; } = string.Empty;
	public string Key { get; set; } = string.Empty;

	public JsonElement Value { get; set; } = default;
}

public class GameServerFilter
{
	public List<GameServerAttributeCriteria> Criteria { get; set; } = new();
	public string? BuildUniqueId { get; set; } = null;
	public int? OpenPlayersRequired { get; set; } = null;
	public int? MaxResults { get; set; } = null;
}