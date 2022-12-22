using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;

namespace UT4MasterServer.Services;

public class MatchmakingService
{
	// persistent runtime-only dictionary
	private Dictionary<EpicID, GameServer> serversBySession;

	public MatchmakingService(IOptions<UT4EverDatabaseSettings> settings)
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
		// TODO

		throw new NotImplementedException();

		return serversBySession.Values.ToList();
	}

	public bool Heartbeat(EpicID sessionID, EpicID serverID)
	{
		var server = Get(sessionID, serverID);

		if (server == null)
			return false;

		server.Heartbeat(); // TODO: this is a reference and not a copy right?
		return true;
	}
}
