using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public enum GameServerTrust
	{
		Epic = 0,
		Trusted = 1,
		Untrusted = 2
	}

	public class GameServerAttributes
	{
		public Dictionary<string, object> ServerConfigs;

		public GameServerAttributes()
		{
			ServerConfigs = new Dictionary<string, object>();
		}

		public void Set(string key, string? value)
		{
			SetInternal(key, value);
		}

		public void Set(string key, int? value)
		{
			SetInternal(key, value);
		}

		public void Set(string key, bool? value)
		{
			SetInternal(key, value);
		}

		public void SetInternal(string key, object? value)
		{
			if (value != null)
			{
				if (ServerConfigs.ContainsKey(key))
					ServerConfigs[key] = value;
				else
					ServerConfigs.Add(key, value);
			}
			else
			{
				if (ServerConfigs.ContainsKey(key))
					ServerConfigs.Remove(key);
			}
		}

		public JObject ToJObject()
		{
			var obj = new JObject();

			foreach (var kvp in ServerConfigs)
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
	}

	public class GameServer
	{
		[JsonPropertyName("id")]
		public EpicID ID { get; set; }

		[JsonPropertyName("ownerId")]
		public EpicID OwnerID { get; set; }

		[JsonPropertyName("ownerName")]
		public string OwnerName { get; set; }

		[JsonPropertyName("serverName")]
		public string ServerName { get; set; }

		[JsonPropertyName("serverAddress")]
		public string ServerAddress { get; set; }

		[JsonPropertyName("serverPort")]
		public int ServerPort { get; set; }

		[JsonPropertyName("maxPublicPlayers")]
		public int MaxPublicPlayers { get; set; }

		//[JsonPropertyName("openPublicPlayers")]
		//public int OpenPublicPlayers { get; set; }

		[JsonPropertyName("maxPrivatePlayers")]
		public int MaxPrivatePlayers { get; set; }

		//[JsonPropertyName("openPrivatePlayers")]
		//public int OpenPrivatePlayers { get; set; }

		[JsonPropertyName("attributes")]
		public GameServerAttributes Attributes { get; set; }

		[JsonPropertyName("publicPlayers")]
		public List<EpicID> PublicPlayers { get; set; }

		[JsonPropertyName("privatePlayers")]
		public List<EpicID> PrivatePlayers { get; set; }

		//[JsonPropertyName("totalPlayers")]
		//public int TotalPlayers { get; set; }

		[JsonPropertyName("allowJoinInProgress")]
		public bool AllowJoinInProgress { get; set; }

		[JsonPropertyName("shouldAdvertise")]
		public bool ShouldAdvertise { get; set; }

		[JsonPropertyName("isDedicated")]
		public bool IsDedicated { get; set; }

		[JsonPropertyName("usesStats")]
		public bool UsesStats { get; set; }

		[JsonPropertyName("usesPresence")]
		public bool UsesPresence { get; set; }

		[JsonPropertyName("allowInvites")]
		public bool AllowInvites { get; set; }

		[JsonPropertyName("allowJoinViaPresence")]
		public bool AllowJoinViaPresence { get; set; }

		[JsonPropertyName("allowJoinViaPresenceFriendsOnly")]
		public bool AllowJoinViaPresenceFriendsOnly { get; set; }

		[JsonPropertyName("buildUniqueId")]
		public string BuildUniqueID { get; set; }

		[JsonPropertyName("lastUpdated")]
		public DateTime LastUpdated { get; set; }

		[JsonPropertyName("sortWeight")]
		public int SortWeight { get; set; }

		[JsonPropertyName("started")]
		public bool Started { get; set; }

		public GameServer()
		{
			ID = EpicID.GenerateNew();
			OwnerID = EpicID.GenerateNew();
			OwnerName = "[DS]nohost-00000";
			ServerName = "[DS]nohost-00000";
			ServerAddress = "0.0.0.0";
			ServerPort = 7777;
			MaxPublicPlayers = 10000;
			//OpenPublicPlayers = 10000;
			MaxPrivatePlayers = 0;
			//OpenPrivatePlayers = 0;
			Attributes = new GameServerAttributes();
			PublicPlayers = new List<EpicID>();
			PrivatePlayers = new List<EpicID>();
			//TotalPlayers = 0;
			AllowJoinInProgress = true;
			ShouldAdvertise = true;
			IsDedicated = true;
			UsesStats = false;
			AllowInvites = true;
			UsesPresence = false;
			AllowJoinViaPresence = true;
			AllowJoinViaPresenceFriendsOnly = false;
			BuildUniqueID = "256652735";
			LastUpdated = DateTimeExtension.UnixTimestampStartOfTime;
			Started = false;
		}

		/// <summary>
		/// Temporary constructor to form fake servers for testing
		/// </summary>
		internal GameServer(string serverName, string domain, string ipAddress) : this()
		{
			// it seems that at least ServerAddress is checked before game lists a hub
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
			foreach (var attribute in update.Attributes.ServerConfigs)
			{
				if (attribute.Key == "UT_SERVERTRUSTLEVEL_i")
					continue; // do not allow server to modify this attribute

				Attributes.SetInternal(attribute.Key, attribute.Value);
			}
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
		}


		public JObject ToJson(bool isResponseToClient)
		{
			// build json
			var obj = new JObject();

			obj.Add("id", ID.ToString());
			obj.Add("ownerId", OwnerID.ToString().ToUpper());
			obj.Add("ownerName", OwnerName);
			obj.Add("serverName", ServerName);
			obj.Add("serverAddress", ServerAddress);
			obj.Add("serverPort", ServerPort);
			obj.Add("maxPublicPlayers", MaxPublicPlayers);
			obj.Add("openPublicPlayers", MaxPublicPlayers - PublicPlayers.Count);
			obj.Add("maxPrivatePlayers", MaxPrivatePlayers);
			obj.Add("openPrivatePlayers", MaxPrivatePlayers - PrivatePlayers.Count);
			obj.Add("attributes", Attributes.ToJObject());
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
}
