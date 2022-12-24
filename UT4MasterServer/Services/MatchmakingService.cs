using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;

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
		if (!serversBySession.TryGetValue(sessionID, out var server))
			return null;

		if (server.ID != serverID)
			return null;

		return server;
	}

	public List<GameServer> List(GameServerFilter filter)
	{
		RemoveStale();

		var matches = new List<GameServer>();
		foreach (var server in serversBySession.Values)
		{
			if (server.BuildUniqueID != filter.BuildUniqueId)
				continue;

			bool isMatch = true;
			foreach (var condition in filter.Criteria)
			{
				bool isEqual = true;

				if (!server.Attributes.ServerConfigs.ContainsKey(condition.Key))
				{
					isEqual = condition.Value.ValueKind == System.Text.Json.JsonValueKind.Null;
				}
				else
				{
					object obj = server.Attributes.ServerConfigs[condition.Key];
					isEqual = condition.Value.Equals(obj);
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

	public bool Heartbeat(EpicID sessionID, EpicID serverID)
	{
		var server = Get(sessionID, serverID);

		if (server == null)
			return false;

		server.Heartbeat(); // TODO: this is a reference and not a copy right?
		return true;
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
