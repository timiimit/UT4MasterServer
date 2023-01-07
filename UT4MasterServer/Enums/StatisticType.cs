namespace UT4MasterServer.Enums;

public enum StatisticType
{
	Unknown = 0,

	// Quick Look
	SkillRating,
	TDMSkillRating,
	CTFSkillRating,
	DMSkillRating,
	ShowdownSkillRating,
	FlagRunSkillRating, // Unavailable on the UI
	RankedDuelSkillRating, // Unavailable on the UI
	RankedCTFSkillRating, // Unavailable on the UI
	RankedShowdownSkillRating, // Unavailable on the UI
	RankedFlagRunSkillRating, // Unavailable on the UI
	MatchesPlayed,
	MatchesQuit,
	TimePlayed,
	//KDRatio, // Calculated
	//WLRatio, // Calculated
	Wins,
	Losses,
	Kills,
	Deaths,
	Suicides,

	// Kill Achievements
	MultiKillLevel0,
	MultiKillLevel1,
	MultiKillLevel2,
	MultiKillLevel3,
	SpreeKillLevel0,
	SpreeKillLevel1,
	SpreeKillLevel2,
	SpreeKillLevel3,
	SpreeKillLevel4,
	BestShockCombo,
	AmazingCombos,
	AirRox,
	FlakShreds,
	AirSnot,

	// Power Up Achievements
	UDamageTime,
	BerserkTime,
	InvisibilityTime,
	UDamageCount,
	BerserkCount,
	InvisibilityCount,
	BootJumps,
	ShieldBeltCount,
	ArmorVestCount,
	ArmorPadsCount,
	HelmetCount,
	KegCount, // Unavailable on the UI

	// Weapon Stats - Impact Hammer
	ImpactHammerKills,
	ImpactHammerDeaths,

	// Weapon Stats - Enforcer
	EnforcerKills,
	EnforcerDeaths,
	EnforcerShots,
	EnforcerHits,
	//EnforcerAccuracy, // Calculated

	// Weapon Stats - BioRifle
	BioRifleKills,
	BioRifleDeaths,
	BioRifleShots,
	BioRifleHits,
	//BioRifleAccuracy, // Calculated

	// Weapon Stats - Grenade Launcher
	BioLauncherKills,
	BioLauncherDeaths,
	BioLauncherShots,
	BioLauncherHits,
	//BioLauncherAccuracy, // Calculated

	// Weapon Stats - Shock Rifle
	ShockBeamKills,
	ShockBeamDeaths,
	ShockCoreKills,
	ShockCoreDeaths,
	ShockComboKills,
	ShockComboDeaths,
	ShockRifleShots,
	ShockRifleHits,
	//ShockRifleAccuracy, // Calculated

	// Weapon Stats - Link Gun
	LinkKills,
	LinkDeaths,
	LinkBeamKills,
	LinkBeamDeaths,
	LinkShots,
	LinkHits,
	//LinkAccuracy, // Calculated

	// Weapon Stats - Minigun
	MinigunKills,
	MinigunDeaths,
	MinigunShardKills,
	MinigunShardDeaths,
	MinigunShots,
	MinigunHits,
	//MinigunAccuracy, // Calculated

	// Weapon Stats - Flak Cannon
	FlakShardKills,
	FlakShardDeaths,
	FlakShellKills,
	FlakShellDeaths,
	FlakShots,
	FlakHits,
	//FlakAccuracy, // Calculated

	// Weapon Stats - Rocket
	RocketKills,
	RocketDeaths,
	RocketShots,
	RocketHits,
	//RocketAccuracy, // Calculated

	// Weapon Stats - Sniper
	SniperKills,
	SniperDeaths,
	SniperHeadshotKills,
	SniperHeadshotDeaths,
	SniperShots,
	SniperHits,
	//SniperAccuracy, // Calculated

	// Weapon Stats - Lightning Rifle (Not available in the source code)
	LightningRiflePrimaryKills,
	LightningRiflePrimaryDeaths,
	LightningRifleSecondaryKills,
	LightningRifleSecondaryDeaths,
	LightningRifleShots,
	LightningRifleHits,
	//LightningRifleAccuracy, // Calculated

	// Weapon Stats - Redeemer
	RedeemerKills,
	RedeemerDeaths,
	RedeemerShots,
	RedeemerHits,
	//RedeemerAccuracy, // Calculated

	// Weapon Stats - Instagib
	InstagibKills,
	InstagibDeaths,
	InstagibShots,
	InstagibHits,
	//InstagibAccuracy, // Calculated

	// Weapon Stats - Translocator
	TelefragKills,
	TelefragDeaths,

	// Miscellaneous - Movement
	RunDist,
	SprintDist,
	InAirDist,
	SwimDist,
	TranslocDist,
	NumDodges,
	NumWallDodges,
	NumJumps,
	NumLiftJumps,
	NumFloorSlides,
	NumWallRuns,
	NumImpactJumps,
	NumRocketJumps,
	SlideDist,
	WallRunDist,

	// Miscellaneous - Capture the Flag
	FlagCaptures,
	FlagReturns,
	FlagAssists,
	FlagHeldDeny,
	FlagHeldDenyTime,
	FlagHeldTime,
	FlagReturnPoints,
	CarryAssist,
	CarryAssistPoints,
	FlagCapPoints,
	DefendAssist,
	DefendAssistPoints,
	ReturnAssist,
	ReturnAssistPoints,
	TeamCapPoints,
	EnemyFCDamage,
	FCKills,
	FCKillPoints,
	FlagSupportKills,
	FlagSupportKillPoints,
	RegularKillPoints,
	FlagGrabs,
	AttackerScore,
	DefenderScore,
	SupporterScore,
	TeamKills, // Unavailable on the UI
}
