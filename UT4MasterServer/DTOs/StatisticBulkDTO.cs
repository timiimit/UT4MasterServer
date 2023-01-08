namespace UT4MasterServer.DTOs;

public sealed class StatisticBulkDTO
{
	// Quick Look
	public int? SkillRating { get; set; }
	public int? TDMSkillRating { get; set; }
	public int? CTFSkillRating { get; set; }
	public int? DMSkillRating { get; set; }
	public int? ShowdownSkillRating { get; set; }
	public int? FlagRunSkillRating { get; set; } // Unavailable on the UI
	public int? RankedDuelSkillRating { get; set; } // Unavailable on the UI
	public int? RankedCTFSkillRating { get; set; } // Unavailable on the UI
	public int? RankedShowdownSkillRating { get; set; } // Unavailable on the UI
	public int? RankedFlagRunSkillRating { get; set; } // Unavailable on the UI
	public int? MatchesPlayed { get; set; }
	public int? MatchesQuit { get; set; }
	public int? TimePlayed { get; set; }
	//KDRatio - Calculated
	//WLRatio - Calculated
	public int? Wins { get; set; }
	public int? Losses { get; set; }
	public int? Kills { get; set; }
	public int? Deaths { get; set; }
	public int? Suicides { get; set; }

	// Kill Achievements
	public int? MultiKillLevel0 { get; set; }
	public int? MultiKillLevel1 { get; set; }
	public int? MultiKillLevel2 { get; set; }
	public int? MultiKillLevel3 { get; set; }
	public int? SpreeKillLevel0 { get; set; }
	public int? SpreeKillLevel1 { get; set; }
	public int? SpreeKillLevel2 { get; set; }
	public int? SpreeKillLevel3 { get; set; }
	public int? SpreeKillLevel4 { get; set; }
	public float? BestShockCombo { get; set; }
	public int? AmazingCombos { get; set; }
	public int? AirRox { get; set; }
	public int? FlakShreds { get; set; }
	public int? AirSnot { get; set; }

	// Power Up Achievements
	public int? UDamageTime { get; set; }
	public int? BerserkTime { get; set; }
	public int? InvisibilityTime { get; set; }
	public int? UDamageCount { get; set; }
	public int? BerserkCount { get; set; }
	public int? InvisibilityCount { get; set; }
	public int? BootJumps { get; set; }
	public int? ShieldBeltCount { get; set; }
	public int? ArmorVestCount { get; set; }
	public int? ArmorPadsCount { get; set; }
	public int? HelmetCount { get; set; }
	public int? KegCount { get; set; } // Unavailable on the UI

	// Weapon Stats - Impact Hammer
	public int? ImpactHammerKills { get; set; }
	public int? ImpactHammerDeaths { get; set; }

	// Weapon Stats - Enforcer
	public int? EnforcerKills { get; set; }
	public int? EnforcerDeaths { get; set; }
	public int? EnforcerShots { get; set; }
	public float? EnforcerHits { get; set; }
	//EnforcerAccuracy - Calculated

	// Weapon Stats - BioRifle
	public int? BioRifleKills { get; set; }
	public int? BioRifleDeaths { get; set; }
	public int? BioRifleShots { get; set; }
	public float? BioRifleHits { get; set; }
	//BioRifleAccuracy - Calculated

	// Weapon Stats - Grenade Launcher
	public int? BioLauncherKills { get; set; }
	public int? BioLauncherDeaths { get; set; }
	public int? BioLauncherShots { get; set; }
	public float? BioLauncherHits { get; set; }
	//BioLauncherAccuracy - Calculated

	// Weapon Stats - Shock Rifle
	public int? ShockBeamKills { get; set; }
	public int? ShockBeamDeaths { get; set; }
	public int? ShockCoreKills { get; set; }
	public int? ShockCoreDeaths { get; set; }
	public int? ShockComboKills { get; set; }
	public int? ShockComboDeaths { get; set; }
	public int? ShockRifleShots { get; set; }
	public float? ShockRifleHits { get; set; }
	//ShockRifleAccuracy - Calculated

	// Weapon Stats - Link Gun
	public int? LinkKills { get; set; }
	public int? LinkDeaths { get; set; }
	public int? LinkBeamKills { get; set; }
	public int? LinkBeamDeaths { get; set; }
	public int? LinkShots { get; set; }
	public float? LinkHits { get; set; }
	//LinkAccuracy - Calculated

	// Weapon Stats - Minigun
	public int? MinigunKills { get; set; }
	public int? MinigunDeaths { get; set; }
	public int? MinigunShardKills { get; set; }
	public int? MinigunShardDeaths { get; set; }
	public int? MinigunShots { get; set; }
	public float? MinigunHits { get; set; }
	//MinigunAccuracy - Calculated

	// Weapon Stats - Flak Cannon
	public int? FlakShardKills { get; set; }
	public int? FlakShardDeaths { get; set; }
	public int? FlakShellKills { get; set; }
	public int? FlakShellDeaths { get; set; }
	public int? FlakShots { get; set; }
	public float? FlakHits { get; set; }
	//FlakAccuracy - Calculated

	// Weapon Stats - Rocket
	public int? RocketKills { get; set; }
	public int? RocketDeaths { get; set; }
	public int? RocketShots { get; set; }
	public int? RocketHits { get; set; }
	//RocketAccuracy - Calculated

	// Weapon Stats - Sniper
	public int? SniperKills { get; set; }
	public int? SniperDeaths { get; set; }
	public int? SniperHeadshotKills { get; set; }
	public int? SniperHeadshotDeaths { get; set; }
	public int? SniperShots { get; set; }
	public float? SniperHits { get; set; }
	//SniperAccuracy - Calculated

	// Weapon Stats - Lightning Rifle (Not available in the source code)
	public int? LightningRiflePrimaryKills { get; set; }
	public int? LightningRiflePrimaryDeaths { get; set; }
	public int? LightningRifleSecondaryKills { get; set; }
	public int? LightningRifleSecondaryDeaths { get; set; }
	public int? LightningRifleShots { get; set; }
	public float? LightningRifleHits { get; set; }
	//LightningRifleAccuracy - Calculated

	// Weapon Stats - Redeemer
	public int? RedeemerKills { get; set; }
	public int? RedeemerDeaths { get; set; }
	public int? RedeemerShots { get; set; }
	public float? RedeemerHits { get; set; }
	//RedeemerAccuracy - Calculated

	// Weapon Stats - Instagib
	public int? InstagibKills { get; set; }
	public int? InstagibDeaths { get; set; }
	public int? InstagibShots { get; set; }
	public float? InstagibHits { get; set; }
	//InstagibAccuracy - Calculated

	// Weapon Stats - Translocator
	public int? TelefragKills { get; set; }
	public int? TelefragDeaths { get; set; }

	// Miscellaneous - Movement
	public float? RunDist { get; set; }
	public float? SprintDist { get; set; }
	public float? InAirDist { get; set; }
	public float? SwimDist { get; set; }
	public float? TranslocDist { get; set; }
	public int? NumDodges { get; set; }
	public int? NumWallDodges { get; set; }
	public int? NumJumps { get; set; }
	public int? NumLiftJumps { get; set; }
	public int? NumFloorSlides { get; set; }
	public int? NumWallRuns { get; set; }
	public int? NumImpactJumps { get; set; }
	public int? NumRocketJumps { get; set; }
	public float? SlideDist { get; set; }
	public float? WallRunDist { get; set; }

	// Miscellaneous - Capture the Flag
	public int? FlagCaptures { get; set; }
	public int? FlagReturns { get; set; }
	public int? FlagAssists { get; set; }
	public int? FlagHeldDeny { get; set; }
	public int? FlagHeldDenyTime { get; set; }
	public int? FlagHeldTime { get; set; }
	public int? FlagReturnPoints { get; set; }
	public int? CarryAssist { get; set; }
	public int? CarryAssistPoints { get; set; }
	public int? FlagCapPoints { get; set; }
	public int? DefendAssist { get; set; }
	public int? DefendAssistPoints { get; set; }
	public int? ReturnAssist { get; set; }
	public int? ReturnAssistPoints { get; set; }
	public int? TeamCapPoints { get; set; }
	public int? EnemyFCDamage { get; set; }
	public int? FCKills { get; set; }
	public int? FCKillPoints { get; set; }
	public int? FlagSupportKills { get; set; }
	public int? FlagSupportKillPoints { get; set; }
	public int? RegularKillPoints { get; set; }
	public int? FlagGrabs { get; set; }
	public int? AttackerScore { get; set; }
	public int? DefenderScore { get; set; }
	public int? SupporterScore { get; set; }
	public int? TeamKills { get; set; } // Unavailable on the UI
}
