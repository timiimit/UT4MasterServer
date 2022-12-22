using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer
{
	public enum ServerTrust
	{
		Epic = 0,
		Trusted = 1,
		Untrusted = 2
	}

	public struct HubAttributes
	{
		// ordered from highest to lowest importance to end-user
		// commented properties seem to be always constant

		public string ServerName { get; set; }
		public string MatchCount { get; set; }
		public string MessageOfTheDay { get; set; }

		public string GameMode { get; set; }
		public string MapName { get; set; }
		public string MatchState { get; set; }
		//public int MatchDuration { get; set; }

		public int PlayerCount { get; set; }
		public int MaxPlayers { get; set; }
		public int SpectatorCount { get; set; }
		public int MaxSpectators { get; set; }

		public ServerTrust TrustLevel { get; set; }
		public int ServerFlags { get; set; } // this seems to be either 0 or 1. i think i saw this in UT source. atm idk what it means
		public int MinELO { get; set; }
		public int MaxELO { get; set; }

		public string RedTeamSize { get; set; }
		public string BlueTeamSize { get; set; }

		//public string ServerVersion { get; set; }
		public CommonID HubGUID { get; set; }
		public CommonID InstanceGUID { get; set; }
		public int BeaconPort { get; set; }
		public int GameInstance { get; set; }
		//public bool TrainingGrounds { get; set; }


		public JObject ToJObject()
		{
			var obj = new JObject();

			obj.Add("UT_SERVERNAME_s", ServerName);
			obj.Add("UT_REDTEAMSIZE_i", RedTeamSize);
			obj.Add("UT_NUMMATCHES_i", MatchCount);
			obj.Add("UT_GAMEINSTANCE_i", GameInstance);
			obj.Add("UT_MAXSPECTATORS_i", MaxSpectators);
			obj.Add("BEACONPORT_i", BeaconPort);
			obj.Add("UT_PLAYERONLINE_i", PlayerCount);
			obj.Add("UT_SERVERVERSION_s", "3525360"); // version engraved into game's tombstone
			obj.Add("GAMEMODE_s", GameMode);
			obj.Add("UT_HUBGUID_s", HubGUID.ToString());
			obj.Add("UT_BLUETEAMSIZE_i", BlueTeamSize);
			obj.Add("UT_MATCHSTATE_s", MatchState);
			obj.Add("UT_SERVERTRUSTLEVEL_i", (int)TrustLevel);
			obj.Add("UT_SERVERINSTANCEGUID_s", InstanceGUID.ToString());
			obj.Add("UT_TRAININGGROUND_b", false);
			obj.Add("UT_MINELO_i", MinELO);
			obj.Add("UT_MAXELO_i", MaxELO);
			obj.Add("UT_SPECTATORSONLINE_i", SpectatorCount);
			obj.Add("UT_MAXPLAYERS_i", MaxPlayers);
			obj.Add("UT_SERVERMOTD_s", MessageOfTheDay);
			obj.Add("MAPNAME_s", MapName);
			obj.Add("UT_MATCHDURATION_i", 0);
			obj.Add("UT_SERVERFLAGS_i", ServerFlags);

			return obj;
		}
	}

	public class Hub
	{
		// commented properties seem to be always constant

		public CommonID ID { get; set; }
		public CommonID OwnerID { get; set; }
		public string OwnerName { get; set; }
		public string ServerName { get; set; }
		public string ServerAddress { get; set; }
		public int ServerPort { get; set; }
		//public int MaxPublicPlayers { get; set; }
		//public int OpenPublicPlayers { get; set; }
		//public int MaxPrivatePlayers { get; set; }
		//public int OpenPrivatePlayers { get; set; }
		public HubAttributes Attributes { get; set; } // this might be a Dictionary<string, object>, but i think its useless to do it that way if we know that keys are constant
		public List<CommonID> PublicPlayers { get; set; }
		public List<CommonID> PrivatePlayers { get; set; }
		//public int TotalPlayers { get; set; } // probably just PublicPlayers.Count+PrivatePlayers.Count
		//public bool AllowJoinInProgress { get; set; }
		//public bool ShouldAdvertise { get; set; }
		//public bool IsDedicated { get; set; }
		//public bool UsesStats { get; set; }
		//public bool UsesPresence { get; set; }
		//public bool AllowInvites { get; set; }
		//public bool AllowJoinViaPresence { get; set; }
		//public bool AllowJoinViaPresenceFriendsOnly { get; set; }
		//public string BuildUniqueID { get; set; }
		public DateTime LastUpdated { get; set; }
		//public bool Started { get; set; }

		public Hub()
		{
			OwnerName = string.Empty;
			ServerName = string.Empty;
			ServerAddress = string.Empty;
			ID = CommonID.GetInvalid();
			PublicPlayers = new List<CommonID>();
			PrivatePlayers = new List<CommonID>();
			LastUpdated = DateTime.MinValue;
		}


		public string ToJson()
		{
			// these values seem to be constant
			int totalPlayers = PublicPlayers.Count + PrivatePlayers.Count;

			const int maxPublicPlayers = 10000;
			int maxOpenPublicPlayers = maxPublicPlayers - totalPlayers;
			const int maxPrivatePlayers = 0;
			const int maxOpenPrivatePlayers = 0;

			// build json
			var obj = new JObject();

			obj.Add("id", ID.ToString());
			obj.Add("ownerId", OwnerID.ToString());
			obj.Add("ownerName", $"[DS]{OwnerName}"); // "[DS]" seems to be a prefix for all, maybe it stands for dedicated server?
			obj.Add("serverName", $"[DS]{ServerName}"); // "[DS]" seems to be a prefix for all, maybe it stands for dedicated server?
			obj.Add("serverAddress", ServerAddress);
			obj.Add("serverPort", ServerPort);
			obj.Add("maxPublicPlayers", maxPublicPlayers);
			obj.Add("openPublicPlayers", maxOpenPublicPlayers);
			obj.Add("maxPrivatePlayers", maxPrivatePlayers);
			obj.Add("openPrivatePlayers", maxOpenPrivatePlayers);
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
			obj.Add("totalPlayers", totalPlayers);
			obj.Add("allowJoinInProgress", true);
			obj.Add("shouldAdvertise", true);
			obj.Add("isDedicated", true);
			obj.Add("usersStats", false);
			obj.Add("allowInvites", true);
			obj.Add("usesPresence", false);
			obj.Add("allowJoinViaPresence", true);
			obj.Add("allowJoinViaPresenceFriendsOnly", false);
			obj.Add("buildUniqueId", "256652735");
			obj.Add("lastUpdated", LastUpdated.ToStringEpic());
			obj.Add("started", true);

			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}
	}
}
