

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

namespace UT4MasterServer.Models;

public class GameServerAttributeCriteria
{
	public string Type { get; set; } = string.Empty;
	public string Key { get; set; } = string.Empty;
	public object? Value { get; set; } = null;
}

public class GameServerFilter
{
	public List<GameServerAttributeCriteria> Criteria { get; set; } = new List<GameServerAttributeCriteria>();
	public string BuildUniqueId { get; set; } = string.Empty;
	public int MaxResults { get; set; } = 0;
}