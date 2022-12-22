using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		//// ordered from highest to lowest importance to end-user
		//// commented properties seem to be always constant

		//public string ServerName { get; set; }
		//public string MatchCount { get; set; }
		//public string MessageOfTheDay { get; set; }

		//public string GameMode { get; set; }
		//public string MapName { get; set; }
		//public string MatchState { get; set; }
		////public int MatchDuration { get; set; }

		//public int PlayerCount { get; set; }
		//public int MaxPlayers { get; set; }
		//public int SpectatorCount { get; set; }
		//public int MaxSpectators { get; set; }

		//public ServerTrust TrustLevel { get; set; }
		//public int ServerFlags { get; set; } // this seems to be either 0 or 1. i think i saw this in UT source. atm idk what it means
		//public int MinELO { get; set; }
		//public int MaxELO { get; set; }

		//public string RedTeamSize { get; set; }
		//public string BlueTeamSize { get; set; }

		////public string ServerVersion { get; set; }
		//public EpicID HubGUID { get; set; }
		//public EpicID InstanceGUID { get; set; }
		//public int BeaconPort { get; set; }
		//public int GameInstance { get; set; }
		////public bool TrainingGrounds { get; set; }

		public Dictionary<string, object> ServerConfigs;

		public GameServerAttributes()
		{
			ServerConfigs = new Dictionary<string, object>();
		}

		public void Set(string key, string? value)
		{
			SetInternal($"{key.ToUpper()}_s", value);
		}

		public void Set(string key, int? value)
		{
			SetInternal($"{key.ToUpper()}_i", value);
		}

		public void Set(string key, bool? value)
		{
			SetInternal($"{key.ToUpper()}_b", value);
		}

		private void SetInternal(string key, object? value)
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

			//obj.Add("UT_SERVERNAME_s", ServerName);
			//obj.Add("UT_REDTEAMSIZE_i", RedTeamSize);
			//obj.Add("UT_NUMMATCHES_i", MatchCount);
			//obj.Add("UT_GAMEINSTANCE_i", GameInstance);
			//obj.Add("UT_MAXSPECTATORS_i", MaxSpectators);
			//obj.Add("BEACONPORT_i", BeaconPort);
			//obj.Add("UT_PLAYERONLINE_i", PlayerCount);
			//obj.Add("UT_SERVERVERSION_s", "3525360"); // version engraved into game's tombstone
			//obj.Add("GAMEMODE_s", GameMode);
			//obj.Add("UT_HUBGUID_s", HubGUID.ToString());
			//obj.Add("UT_BLUETEAMSIZE_i", BlueTeamSize);
			//obj.Add("UT_MATCHSTATE_s", MatchState);
			//obj.Add("UT_SERVERTRUSTLEVEL_i", (int)TrustLevel);
			//obj.Add("UT_SERVERINSTANCEGUID_s", InstanceGUID.ToString());
			//obj.Add("UT_TRAININGGROUND_b", false);
			//obj.Add("UT_MINELO_i", MinELO);
			//obj.Add("UT_MAXELO_i", MaxELO);
			//obj.Add("UT_SPECTATORSONLINE_i", SpectatorCount);
			//obj.Add("UT_MAXPLAYERS_i", MaxPlayers);
			//obj.Add("UT_SERVERMOTD_s", MessageOfTheDay);
			//obj.Add("MAPNAME_s", MapName);
			//obj.Add("UT_MATCHDURATION_i", 0);
			//obj.Add("UT_SERVERFLAGS_i", ServerFlags);

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
		// commented properties seem to be always constant

		public EpicID ID { get; set; }
		public EpicID OwnerID { get; set; }
		public string OwnerName { get; set; }
		public string ServerName { get; set; }
		public string ServerAddress { get; set; }
		public int ServerPort { get; set; }
		public int MaxPublicPlayers { get; set; }
		public int OpenPublicPlayers { get; set; }
		public int MaxPrivatePlayers { get; set; }
		public int OpenPrivatePlayers { get; set; }
		public GameServerAttributes Attributes { get; set; }
		public List<EpicID> PublicPlayers { get; set; }
		public List<EpicID> PrivatePlayers { get; set; }
		public int TotalPlayers { get; set; }
		public bool AllowJoinInProgress { get; set; }
		public bool ShouldAdvertise { get; set; }
		public bool IsDedicated { get; set; }
		public bool UsesStats { get; set; }
		public bool UsesPresence { get; set; }
		public bool AllowInvites { get; set; }
		public bool AllowJoinViaPresence { get; set; }
		public bool AllowJoinViaPresenceFriendsOnly { get; set; }
		public string BuildUniqueID { get; set; }
		public DateTime LastUpdated { get; set; }
		public int SortWeight { get; set; }
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
			OpenPublicPlayers = 10000;
			MaxPrivatePlayers = 0;
			OpenPrivatePlayers = 0;
			Attributes = new GameServerAttributes();

			Attributes.Set("UT_SERVERNAME", "ServerName");
			Attributes.Set("UT_REDTEAMSIZE", 0);
			Attributes.Set("UT_BLUETEAMSIZE", 0);
			Attributes.Set("UT_NUMMATCHES", 0);
			Attributes.Set("UT_GAMEINSTANCE", 0); // 0 = hub, 1 = game instance?
			Attributes.Set("UT_MAXSPECTATORS", 7);
			Attributes.Set("BEACONPORT", 7787);
			Attributes.Set("UT_PLAYERONLINE", 0);
			Attributes.Set("UT_SERVERVERSION", "3525360");
			Attributes.Set("GAMEMODE", "/Script/UnrealTournament.UTLobbyGameMode");
			Attributes.Set("UT_HUBGUID", OwnerID.ToString());
			Attributes.Set("UT_MATCHSTATE", "InProgress");
			Attributes.Set("UT_SERVERTRUSTLEVEL", 0);
			Attributes.Set("UT_SERVERINSTANCEGUID", EpicID.GenerateNew().ToString());
			Attributes.Set("UT_TRAININGGROUND", false);
			Attributes.Set("UT_MINELO", 0);
			Attributes.Set("UT_MAXELO", 0);
			Attributes.Set("UT_SPECTATORSONLINE", 0);
			Attributes.Set("UT_MAXPLAYERS", 200);
			Attributes.Set("UT_SERVERMOTD", "");
			Attributes.Set("MAPNAME", "UT-EntryRank");
			Attributes.Set("UT_MATCHDURATION", 0);
			Attributes.Set("UT_SERVERFLAGS", 0);

			PublicPlayers = new List<EpicID>();
			PrivatePlayers = new List<EpicID>();
			TotalPlayers = 0;
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
			Started = true;
		}

		/// <summary>
		/// Temporary constructor to form fake servers for testing
		/// </summary>
		internal GameServer(string serverName, string domain, string ipAddress) : this()
		{
			// it seems that at least ServerAddress is checked before game lists a hub
			ServerName = OwnerName = domain;
			ServerAddress = ipAddress;
			Attributes.ServerConfigs["UT_SERVERNAME_s"] = serverName;
		}


		public void Heartbeat()
		{
			LastUpdated = DateTime.UtcNow;
		}


		public JObject ToJson(bool isResponseToClient)
		{
			//// these values seem to be constant
			//int totalPlayers = PublicPlayers.Count + PrivatePlayers.Count;

			//const int maxPublicPlayers = 10000;
			//int maxOpenPublicPlayers = maxPublicPlayers - totalPlayers;
			//const int maxPrivatePlayers = 0;
			//const int maxOpenPrivatePlayers = 0;

			// build json
			var obj = new JObject();

			obj.Add("id", ID.ToString());
			obj.Add("ownerId", OwnerID.ToString());
			obj.Add("ownerName", OwnerName);
			obj.Add("serverName", ServerName);
			obj.Add("serverAddress", ServerAddress);
			obj.Add("serverPort", ServerPort);
			obj.Add("maxPublicPlayers", MaxPublicPlayers);
			obj.Add("openPublicPlayers", OpenPublicPlayers);
			obj.Add("maxPrivatePlayers", MaxPrivatePlayers);
			obj.Add("openPrivatePlayers", OpenPrivatePlayers);
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
			obj.Add("totalPlayers", TotalPlayers);
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
