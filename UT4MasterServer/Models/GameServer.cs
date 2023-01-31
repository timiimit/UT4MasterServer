using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using UT4MasterServer.Other;

namespace UT4MasterServer.Models;

public enum GameServerTrust
{
	Epic = 0,
	Trusted = 1,
	Untrusted = 2
}

public class GameServerAttributes
{
	private readonly Dictionary<string, object> serverConfigs;

	public GameServerAttributes()
	{
		serverConfigs = new Dictionary<string, object>();
	}

	public void Set(string key, string? value)
	{
		SetDirect(key, value);
	}

	public void Set(string key, int? value)
	{
		SetDirect(key, value);
	}

	public void Set(string key, bool? value)
	{
		SetDirect(key, value);
	}

	public void Update(GameServerAttributes other)
	{
		foreach (var attribute in other.serverConfigs)
		{
			if (attribute.Key == "UT_SERVERTRUSTLEVEL_i")
				continue; // do not allow server to modify this attribute

			SetDirect(attribute.Key, attribute.Value);
		}
	}

	public bool Contains(string key)
	{
		return serverConfigs.ContainsKey(key);
	}

	public object? Get(string key)
	{
		if (!Contains(key))
			return null;
		return serverConfigs[key];
	}

	public string[] GetKeys()
	{
		return serverConfigs.Keys.ToArray();
	}

	public JObject ToJObject()
	{
		var obj = new JObject();

		foreach (var kvp in serverConfigs)
		{
			if (kvp.Key.EndsWith("_b"))
				obj.Add(kvp.Key, (bool)kvp.Value);
			else if (kvp.Key.EndsWith("_i"))
				obj.Add(kvp.Key, (int)kvp.Value);
			else if (kvp.Key.EndsWith("_s"))
				obj.Add(kvp.Key, (string)kvp.Value);
		}

		return obj;
	}

	private void SetDirect(string key, object? value)
	{
		if (value != null)
		{
			if (serverConfigs.ContainsKey(key))
				serverConfigs[key] = value;
			else
				serverConfigs.Add(key, value);
		}
		else
		{
			if (serverConfigs.ContainsKey(key))
				serverConfigs.Remove(key);
		}
	}
}

public class GameServer
{
	[BsonId]
	[JsonPropertyName("id")]
	public EpicID ID { get; set; } = EpicID.Empty;

	/// <summary>
	/// GameServer's Session
	/// </summary>
	/// <remarks>
	/// Each session is allowed to have only a single server
	/// </remarks>
	[BsonElement("SessionID")]
	public EpicID SessionID { get; set; } = EpicID.Empty;

	/// <summary>
	/// GameServer's OwningClientID
	/// </summary>
	/// <remarks>
	/// All game servers owned by a single entity (hub owner) have the same client ID
	/// </remarks>
	[BsonElement("OwningClientID")]
	public EpicID OwningClientID { get; set; } = EpicID.Empty;

	[BsonElement("LastKnownMatchStartTime")]
	public DateTime LastKnownMatchStartTime { get; set; } = DateTime.UtcNow;

#if DEBUG
	[BsonElement("SessionAccessToken")]
	public string SessionAccessToken { get; set; } = string.Empty;
#endif

	[BsonElement("OwnerID")]
	[JsonPropertyName("ownerId")]
	public EpicID OwnerID { get; set; } = EpicID.Empty;

	[BsonElement("OwnerName")]
	[JsonPropertyName("ownerName")]
	public string OwnerName { get; set; } = "[DS]nohost-00000";

	[BsonElement("ServerName")]
	[JsonPropertyName("serverName")]
	public string ServerName { get; set; } = "[DS]nohost-00000";

	[BsonElement("ServerAddress")]
	[JsonPropertyName("serverAddress")]
	public string ServerAddress { get; set; } = "0.0.0.0";

	[BsonElement("ServerPort")]
	[JsonPropertyName("serverPort")]
	public int ServerPort { get; set; } = 7777;

	[BsonElement("MaxPublicPlayers")]
	[JsonPropertyName("maxPublicPlayers")]
	public int MaxPublicPlayers { get; set; } = 10000;

	//[BsonElement("OpenPublicPlayers")]
	//[JsonPropertyName("openPublicPlayers")]
	//public int OpenPublicPlayers { get; set; } = 10000

	[BsonElement("MaxPrivatePlayers")]
	[JsonPropertyName("maxPrivatePlayers")]
	public int MaxPrivatePlayers { get; set; } = 0;

	//[BsonElement("OpenPrivatePlayers")]
	//[JsonPropertyName("openPrivatePlayers")]
	//public int OpenPrivatePlayers { get; set; } = 0;

	[BsonElement("Attributes")]
	[JsonPropertyName("attributes")]
	public GameServerAttributes Attributes { get; set; } = new();

	[BsonElement("PublicPlayers")]
	[JsonPropertyName("publicPlayers")]
	public List<EpicID> PublicPlayers { get; set; } = new();

	[BsonElement("PrivatePlayers")]
	[JsonPropertyName("privatePlayers")]
	public List<EpicID> PrivatePlayers { get; set; } = new();

	//[BsonElement("TotalPlayers")]
	//[JsonPropertyName("totalPlayers")]
	//public int TotalPlayers { get; set; }

	[BsonElement("AllowJoinInProgress")]
	[JsonPropertyName("allowJoinInProgress")]
	public bool AllowJoinInProgress { get; set; } = true;

	[BsonElement("ShouldAdvertise")]
	[JsonPropertyName("shouldAdvertise")]
	public bool ShouldAdvertise { get; set; } = true;

	[BsonElement("IsDedicated")]
	[JsonPropertyName("isDedicated")]
	public bool IsDedicated { get; set; } = true;

	[BsonElement("UsesStats")]
	[JsonPropertyName("usesStats")]
	public bool UsesStats { get; set; } = false;

	[BsonElement("UsesPresence")]
	[JsonPropertyName("usesPresence")]
	public bool UsesPresence { get; set; } = false;

	[BsonElement("AllowInvites")]
	[JsonPropertyName("allowInvites")]
	public bool AllowInvites { get; set; } = true;

	[BsonElement("AllowJoinViaPresence")]
	[JsonPropertyName("allowJoinViaPresence")]
	public bool AllowJoinViaPresence { get; set; } = true;

	[BsonElement("AllowJoinViaPresenceFriendsOnly")]
	[JsonPropertyName("allowJoinViaPresenceFriendsOnly")]
	public bool AllowJoinViaPresenceFriendsOnly { get; set; } = false;

	[BsonElement("BuildUniqueID")]
	[JsonPropertyName("buildUniqueId")]
	public string BuildUniqueID { get; set; } = "256652735";

	[BsonElement("LastUpdated")]
	[JsonPropertyName("lastUpdated")]
	public DateTime LastUpdated { get; set; } = DateTimeExtension.UnixTimestampStartOfTime;

	/// <summary>
	/// TODO: not sure what this is used for, perhaps we can remove it
	/// entirely and just respond with static `"sortWeight": 0`
	/// when needed
	/// </summary>
	[BsonElement("SortWeight")]
	[JsonPropertyName("sortWeight")]
	public int SortWeight { get; set; } = 0;

	[BsonElement("Started")]
	[JsonPropertyName("started")]
	public bool Started { get; set; } = false;

	public GameServer()
	{
		// everything is already initialized
	}

	/// <summary>
	/// Temporary constructor to form fake servers for testing
	/// </summary>
	internal GameServer(string serverName, string domain, string ipAddress) : this()
	{
		// it seems that at least ServerAddress is checked before game actually lists a hub
		ServerName = OwnerName = domain;
		ServerAddress = ipAddress;
		Attributes.Set("UT_SERVERNAME_s", serverName);

		//Attributes.Set("UT_SERVERNAME", "ServerName");
		Attributes.Set("UT_REDTEAMSIZE_i", 0);
		Attributes.Set("UT_BLUETEAMSIZE_i", 0);
		Attributes.Set("UT_NUMMATCHES_i", 0);
		Attributes.Set("UT_GAMEINSTANCE_i", 0); // 0 = hub, 1 = game instance?
		Attributes.Set("UT_MAXSPECTATORS_i", 7);
		Attributes.Set("BEACONPORT_i", 7787);
		Attributes.Set("UT_PLAYERONLINE_i", 0);
		Attributes.Set("UT_SERVERVERSION_s", "3525360");
		Attributes.Set("GAMEMODE_s", "/Script/UnrealTournament.UTLobbyGameMode");
		Attributes.Set("UT_HUBGUID_s", OwnerID.ToString());
		Attributes.Set("UT_MATCHSTATE_s", "InProgress");
		Attributes.Set("UT_SERVERTRUSTLEVEL_i", 0);
		Attributes.Set("UT_SERVERINSTANCEGUID_s", EpicID.GenerateNew().ToString());
		Attributes.Set("UT_TRAININGGROUND_b", false);
		Attributes.Set("UT_MINELO_i", 0);
		Attributes.Set("UT_MAXELO_i", 0);
		Attributes.Set("UT_SPECTATORSONLINE_i", 0);
		Attributes.Set("UT_MAXPLAYERS_i", 200);
		Attributes.Set("UT_SERVERMOTD_s", "");
		Attributes.Set("MAPNAME_s", "UT-EntryRank");
		Attributes.Set("UT_MATCHDURATION_i", 0);
		Attributes.Set("UT_SERVERFLAGS_i", 0);
	}

	public void Update(GameServer update)
	{
		MaxPublicPlayers = update.MaxPublicPlayers;
		MaxPrivatePlayers = update.MaxPrivatePlayers;
		//OpenPublicPlayers = update.OpenPublicPlayers;
		//OpenPrivatePlayers = update.OpenPrivatePlayers;
		Attributes.Update(update.Attributes);
		PublicPlayers = update.PublicPlayers;
		PrivatePlayers = update.PrivatePlayers;
		//TotalPlayers = update.TotalPlayers;
		AllowJoinInProgress = update.AllowJoinInProgress;
		ShouldAdvertise = update.ShouldAdvertise;
		IsDedicated = update.IsDedicated;
		UsesStats = update.UsesStats;
		UsesPresence = update.UsesPresence;
		AllowInvites = update.AllowInvites;
		AllowJoinViaPresence = update.AllowJoinViaPresence;
		AllowJoinViaPresenceFriendsOnly = update.AllowJoinViaPresenceFriendsOnly;
		BuildUniqueID = update.BuildUniqueID;
		LastUpdated = DateTime.UtcNow;

		// Update custom properties
		SessionID = update.SessionID;
		OwningClientID = update.OwningClientID;

		var matchDuration = (int?)update.Attributes.Get("UT_MATCHDURATION_i");
		if (matchDuration is not null)
		{
			LastKnownMatchStartTime = DateTime.UtcNow - TimeSpan.FromSeconds(matchDuration.Value);
		}
	}

	public JObject ToJson(bool isResponseToClient)
	{
		// Do some preprocessing on attributes
		var attrs = Attributes.ToJObject();
		if (attrs["UT_MATCHSTATE_s"]?.ToObject<string?>() == "InProgress" && attrs.ContainsKey("UT_MATCHDURATION_i"))
		{
			attrs["UT_MATCHDURATION_i"] = (int)(DateTime.UtcNow - LastKnownMatchStartTime).TotalSeconds;
		}


		// build json
		var obj = new JObject();

		obj.Add("id", ID.ToString());
#if DEBUG
		obj.Add("UT4MS__SESSION_ID__DEBUG_ONLY_VALUE", SessionID.ToString());
		obj.Add("UT4MS__SESSION_TOKEN__DEBUG_ONLY_VALUE", SessionAccessToken);
		obj.Add("UT4MS__OWNING_CLIENT_ID__DEBUG_ONLY_VALUE", OwningClientID.ToString());
		obj.Add("UT4MS__LAST_KNOWN_MATCH_START_TIME__DEBUG_ONLY_VALUE", LastKnownMatchStartTime.ToStringISO());
#endif
		obj.Add("ownerId", OwnerID.ToString().ToUpper());
		obj.Add("ownerName", OwnerName);
		obj.Add("serverName", ServerName);
		obj.Add("serverAddress", ServerAddress);
		obj.Add("serverPort", ServerPort);
		obj.Add("maxPublicPlayers", MaxPublicPlayers);
		obj.Add("openPublicPlayers", MaxPublicPlayers - PublicPlayers.Count);
		obj.Add("maxPrivatePlayers", MaxPrivatePlayers);
		obj.Add("openPrivatePlayers", MaxPrivatePlayers - PrivatePlayers.Count);
		obj.Add("attributes", attrs);
		JArray arr = new JArray();
		foreach (var player in PublicPlayers)
		{
			arr.Add(player.ToString());
		}
		obj.Add("publicPlayers", arr);
		arr = new JArray();
		foreach (var player in PrivatePlayers)
		{
			arr.Add(player.ToString());
		}
		obj.Add("privatePlayers", arr);
		obj.Add("totalPlayers", PublicPlayers.Count + PrivatePlayers.Count);
		obj.Add("allowJoinInProgress", AllowJoinInProgress);
		obj.Add("shouldAdvertise", ShouldAdvertise);
		obj.Add("isDedicated", IsDedicated);
		obj.Add("usesStats", UsesStats);
		obj.Add("allowInvites", AllowInvites);
		obj.Add("usesPresence", UsesPresence);
		obj.Add("allowJoinViaPresence", AllowJoinViaPresence);
		obj.Add("allowJoinViaPresenceFriendsOnly", AllowJoinViaPresenceFriendsOnly);
		obj.Add("buildUniqueId", BuildUniqueID);
		obj.Add("lastUpdated", LastUpdated.ToStringISO());
		obj.Add("started", Started);
		if (!isResponseToClient)
		{
			obj.Add("sortWeights", SortWeight);
		}

		return obj;
	}
}
