using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Helpers;

namespace UT4MasterServer.Models.Database;

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
#if DEBUG
	[JsonPropertyName("UT4MS__SESSION_ID__DEBUG_ONLY_VALUE")]
#endif
	[BsonElement("SessionID")]
	public EpicID SessionID { get; set; } = EpicID.Empty;

	/// <summary>
	/// GameServer's OwningClientID
	/// </summary>
	/// <remarks>
	/// All game servers owned by a single entity (hub owner) have the same client ID
	/// </remarks>
#if DEBUG
	[JsonPropertyName("UT4MS__OWNING_CLIENT_ID__DEBUG_ONLY_VALUE")]
#endif
	[BsonElement("OwningClientID")]
	public EpicID OwningClientID { get; set; } = EpicID.Empty;

#if DEBUG
	[JsonPropertyName("UT4MS__LAST_KNOWN_MATCH_START_TIME__DEBUG_ONLY_VALUE")]
#endif
	[BsonElement("LastKnownMatchStartTime"), BsonIgnoreIfDefault]
	public DateTime LastKnownMatchStartTime { get; set; } = default;

#if DEBUG
	[BsonElement("SessionAccessToken")]
	[JsonPropertyName("UT4MS__SESSION_TOKEN__DEBUG_ONLY_VALUE")]
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

	[BsonIgnore]
	[JsonPropertyName("openPublicPlayers")]
	public int OpenPublicPlayers => MaxPublicPlayers - PublicPlayers.Count;

	[BsonElement("MaxPrivatePlayers")]
	[JsonPropertyName("maxPrivatePlayers")]
	public int MaxPrivatePlayers { get; set; }

	[BsonIgnore]
	[JsonPropertyName("openPrivatePlayers")]
	public int OpenPrivatePlayers => MaxPrivatePlayers - PrivatePlayers.Count;

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
	public bool UsesStats { get; set; }

	[BsonElement("UsesPresence")]
	[JsonPropertyName("usesPresence")]
	public bool UsesPresence { get; set; }

	[BsonElement("AllowInvites")]
	[JsonPropertyName("allowInvites")]
	public bool AllowInvites { get; set; } = true;

	[BsonElement("AllowJoinViaPresence")]
	[JsonPropertyName("allowJoinViaPresence")]
	public bool AllowJoinViaPresence { get; set; } = true;

	[BsonElement("AllowJoinViaPresenceFriendsOnly")]
	[JsonPropertyName("allowJoinViaPresenceFriendsOnly")]
	public bool AllowJoinViaPresenceFriendsOnly { get; set; }

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
	}

	public JsonObject ToJson(bool isResponseToClient)
	{
		// TODO: Get rid of dynamic json

		// Do some preprocessing on attributes
		JsonObject? attrs = Attributes.ToJObject();

		// build json
		var obj = new List<KeyValuePair<string, JsonNode?>>();

		obj.Add(new("id", ID.ToString()));
#if DEBUG
		obj.Add(new("UT4MS__SESSION_ID__DEBUG_ONLY_VALUE", SessionID.ToString()));
		obj.Add(new("UT4MS__SESSION_TOKEN__DEBUG_ONLY_VALUE", SessionAccessToken));
		obj.Add(new("UT4MS__OWNING_CLIENT_ID__DEBUG_ONLY_VALUE", OwningClientID.ToString()));
#endif
		obj.Add(new("ownerId", OwnerID.ToString().ToUpper()));
		obj.Add(new("ownerName", OwnerName));
		obj.Add(new("serverName", ServerName));
		obj.Add(new("serverAddress", ServerAddress));
		obj.Add(new("serverPort", ServerPort));
		obj.Add(new("maxPublicPlayers", MaxPublicPlayers));
		obj.Add(new("openPublicPlayers", MaxPublicPlayers - PublicPlayers.Count));
		obj.Add(new("maxPrivatePlayers", MaxPrivatePlayers));
		obj.Add(new("openPrivatePlayers", MaxPrivatePlayers - PrivatePlayers.Count));
		obj.Add(new("attributes", attrs));
		var arr = new JsonArray();
		foreach (EpicID player in PublicPlayers)
		{
			arr.Add(JsonValue.Create(player.ToString()));
		}

		obj.Add(new("publicPlayers", arr));
		arr = new JsonArray();
		foreach (EpicID player in PrivatePlayers)
		{
			arr.Add(JsonValue.Create(player.ToString()));
		}

		obj.Add(new("privatePlayers", arr));
		obj.Add(new("totalPlayers", PublicPlayers.Count + PrivatePlayers.Count));
		obj.Add(new("allowJoinInProgress", AllowJoinInProgress));
		obj.Add(new("shouldAdvertise", ShouldAdvertise));
		obj.Add(new("isDedicated", IsDedicated));
		obj.Add(new("usesStats", UsesStats));
		obj.Add(new("allowInvites", AllowInvites));
		obj.Add(new("usesPresence", UsesPresence));
		obj.Add(new("allowJoinViaPresence", AllowJoinViaPresence));
		obj.Add(new("allowJoinViaPresenceFriendsOnly", AllowJoinViaPresenceFriendsOnly));
		obj.Add(new("buildUniqueId", BuildUniqueID));
		obj.Add(new("lastUpdated", LastUpdated.ToStringISO()));
		obj.Add(new("started", Started));
		if (!isResponseToClient)
		{
			obj.Add(new("sortWeights", SortWeight));
		}

		return new JsonObject(obj);
	}
}
