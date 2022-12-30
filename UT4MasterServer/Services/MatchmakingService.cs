using System.Text.Json;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services;

public class MatchmakingService
{
	// TODO: store servers in database to avoid problems if master reboots
	private Dictionary<EpicID, GameServer> serversBySession;

	public MatchmakingService()
	{
		serversBySession = new Dictionary<EpicID, GameServer>();
	}

	public bool Add(EpicID sessionID, GameServer server)
	{
		if (serversBySession.ContainsKey(sessionID))
			return false;

		serversBySession.Add(sessionID, server);
		return true;
	}

	public bool Update(EpicID sessionID, GameServer server)
	{
		if (!serversBySession.ContainsKey(sessionID))
			return false;

		if (serversBySession[sessionID].ID != server.ID)
			return false;

		return true;
	}

	public bool Remove(EpicID sessionID, EpicID serverID)
	{
		if (Get(sessionID, serverID) == null)
			return false;

		serversBySession.Remove(sessionID);
		return true;
	}

	public GameServer? Get(EpicID sessionID, EpicID serverID)
	{
		var server = Get(sessionID);
		if (server == null)
			return null;

		if (server.ID != serverID)
			return null;

		return server;
	}

	public GameServer? Get(EpicID sessionID)
	{
		if (!serversBySession.TryGetValue(sessionID, out var server))
			return null;

		return server;
	}

	public List<GameServer> List(GameServerFilter filter)
	{
		RemoveStale();

		var matches = new List<GameServer>();
		foreach (var server in serversBySession.Values)
		{
			if (!server.Started)
				continue;

			if (server.BuildUniqueID != filter.BuildUniqueId)
				continue;

			bool isMatch = true;
			foreach (var condition in filter.Criteria)
			{
				bool isEqual = true;

				if (!server.Attributes.ServerConfigs.ContainsKey(condition.Key))
				{
					isEqual = condition.Value.ValueKind == JsonValueKind.Null;
				}
				else
				{
					object obj = server.Attributes.ServerConfigs[condition.Key];
					if (obj is string && condition.Value.ValueKind == JsonValueKind.String)
					{
						string? val = condition.Value.GetString();
						isEqual = val == (string)obj;
					}
					else if (obj is int && condition.Value.ValueKind == JsonValueKind.Number)
					{
						int val = condition.Value.GetInt32();
						isEqual = val == (int)obj;
					}
					else if (obj is bool && (condition.Value.ValueKind == JsonValueKind.True || condition.Value.ValueKind == JsonValueKind.False))
					{
						bool val = condition.Value.GetBoolean();
						isEqual = val == (bool)obj;
					}
					else
					{
						throw new InvalidOperationException($"obj type={obj.GetType().Name}, json type={condition.Value.ValueKind}");
					}
				}

				bool isConditionMet =
					(condition.Type == "EQUAL" && isEqual) ||
					(condition.Type == "NOT_EQUAL" && !isEqual);

				if (!isConditionMet)
				{
					isMatch = false;
					break;
				}
			}

			if (isMatch)
			{
				matches.Add(server);
				if (matches.Count >= filter.MaxResults)
					break;
			}
		}

		return matches;
	}

	public void RemoveStale()
	{
		var staleServerSessions = new List<EpicID>();
		foreach (var kvp in serversBySession)
		{
			if (DateTime.UtcNow > kvp.Value.LastUpdated + TimeSpan.FromMinutes(1))
				staleServerSessions.Add(kvp.Key);
		}

		foreach (var staleServerSession in staleServerSessions)
		{
			serversBySession.Remove(staleServerSession);
		}
	}
}
