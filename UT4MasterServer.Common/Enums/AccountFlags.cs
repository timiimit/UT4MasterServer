using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UT4MasterServer.Common.Enums;

[Flags]
public enum AccountFlags
{
	None = 0,

	/// <summary>
	/// Gives full access to everything.
	/// </summary>
	Admin = 0x01,

	/// <summary>
	/// Gives privilege to edit non-ACL related flags, to change account password, suspend account and do similar account moderation operations.
	/// </summary>
	ACL_AccountsLow = 0x02,

	/// <summary>
	/// Marks people that have contibuted a significant amount of code to this project.
	/// </summary>
	Developer = 0x04,

	/// <summary>
	/// Marks people that have created and shared at least one actual, fun, useful or interesting gamemode, map, mutator, cosmetic or taunt (pak).
	/// </summary>
	ContentCreator = 0x08,

	/// <summary>
	/// Marks people that are owners of one or more hubs. Only people with this flag may own a trusted game server.
	/// </summary>
	HubOwner = 0x10,

	/// <summary>
	/// Gives privilege to edit clients. 
	/// </summary>
	ACL_Clients = 0x20,

	/// <summary>
	/// Gives privilege to edit trusted servers.
	/// </summary>
	ACL_TrustedServers = 0x40,

	/// <summary>
	/// Gives privilege to read any cloud storage file and to edit "UnrealTournmentMCPAnnouncement.json" and any file that starts with "news-".
	/// </summary>
	ACL_CloudStorageAnnouncements = 0x80,

	/// <summary>
	/// Gives privilege to read any cloud storage file and edit cloud storage files "UTMCPPlaylists.json", "UnrealTournamentOnlineSettings.json" and "UnrealTournmentMCPGameRulesets.json".
	/// </summary>
	ACL_CloudStorageRulesets = 0x100,

	/// <summary>
	/// Gives privilege to read any cloud storage file and edit file "UnrealTournmentMCPStorage.json".
	/// </summary>
	ACL_CloudStorageChallenges = 0x200,

	/// <summary>
	/// Gives privilege to see and delete flagged stats and other suspicious XP/rating activity.
	/// </summary>
	ACL_Stats = 0x400,

	/// <summary>
	/// Gives privilege to do anything to accounts with the following restrictions:
	///  - not being able to give ACL_AccountsHigh or Admin account flag
	///  - not being able to remove any ACL flag
	///  - not being able to delete Admin accounts
	///  - not being able to delete account with an ACL flag
	/// </summary>
	ACL_AccountsHigh = 0x800,

	/// <summary>
	/// Meant for ease of use when dealing with masks. No account's flags should be set to this value.
	/// </summary>
	AllMask = ~0

	// NOTE: if you add more flags, make sure to handle account deletion checks.
}
