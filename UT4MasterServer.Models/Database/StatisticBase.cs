using MongoDB.Bson.Serialization.Attributes;

namespace UT4MasterServer.Models.Database;

public class StatisticBase
{
	public readonly static List<string> StatisticProperties = new()
	{
		"matchesplayed",
		"matchesquit",
		"timeplayed",
		"wins",
		"losses",
		"kills",
		"deaths",
		"suicides",
		"multikilllevel0",
		"multikilllevel1",
		"multikilllevel2",
		"multikilllevel3",
		"spreekilllevel0",
		"spreekilllevel1",
		"spreekilllevel2",
		"spreekilllevel3",
		"spreekilllevel4",
		"bestshockcombo",
		"amazingcombos",
		"airrox",
		"flakshreds",
		"airsnot",
		"udamagetime",
		"berserktime",
		"invisibilitytime",
		"udamagecount",
		"berserkcount",
		"invisibilitycount",
		"bootjumps",
		"shieldbeltcount",
		"armorvestcount",
		"armorpadscount",
		"helmetcount",
		"kegcount",
		"impacthammerkills",
		"impacthammerdeaths",
		"enforcerkills",
		"enforcerdeaths",
		"enforcershots",
		"enforcerhits",
		"bioriflekills",
		"biorifledeaths",
		"biorifleshots",
		"bioriflehits",
		"biolauncherkills",
		"biolauncherdeaths",
		"biolaunchershots",
		"biolauncherhits",
		"shockbeamkills",
		"shockbeamdeaths",
		"shockcorekills",
		"shockcoredeaths",
		"shockcombokills",
		"shockcombodeaths",
		"shockrifleshots",
		"shockriflehits",
		"linkkills",
		"linkdeaths",
		"linkbeamkills",
		"linkbeamdeaths",
		"linkshots",
		"linkhits",
		"minigunkills",
		"minigundeaths",
		"minigunshardkills",
		"minigunsharddeaths",
		"minigunshots",
		"minigunhits",
		"flakshardkills",
		"flaksharddeaths",
		"flakshellkills",
		"flakshelldeaths",
		"flakshots",
		"flakhits",
		"rocketkills",
		"rocketdeaths",
		"rocketshots",
		"rockethits",
		"sniperkills",
		"sniperdeaths",
		"sniperheadshotkills",
		"sniperheadshotdeaths",
		"snipershots",
		"sniperhits",
		"lightningrifleprimarykills",
		"lightningrifleprimarydeaths",
		"lightningriflesecondarykills",
		"lightningriflesecondarydeaths",
		"lightningrifleshots",
		"lightningriflehits",
		"redeemerkills",
		"redeemerdeaths",
		"redeemershots",
		"redeemerhits",
		"instagibkills",
		"instagibdeaths",
		"instagibshots",
		"instagibhits",
		"telefragkills",
		"telefragdeaths",
		"rundist",
		"sprintdist",
		"inairdist",
		"swimdist",
		"translocdist",
		"numdodges",
		"numwalldodges",
		"numjumps",
		"numliftjumps",
		"numfloorslides",
		"numwallruns",
		"numimpactjumps",
		"numrocketjumps",
		"slidedist",
		"wallrundist",
		"flagcaptures",
		"flagreturns",
		"flagassists",
		"flaghelddeny",
		"flaghelddenytime",
		"flagheldtime",
		"flagreturnpoints",
		"carryassist",
		"carryassistpoints",
		"flagcappoints",
		"defendassist",
		"defendassistpoints",
		"returnassist",
		"returnassistpoints",
		"teamcappoints",
		"enemyfcdamage",
		"fckills",
		"fckillpoints",
		"flagsupportkills",
		"flagsupportkillpoints",
		"regularkillpoints",
		"flaggrabs",
		"attackerscore",
		"defenderscore",
		"supporterscore",
		"teamkills"
	};

	public StatisticBase() { }

	public StatisticBase(StatisticBase statisticBase)
	{
		MatchesPlayed = statisticBase.MatchesPlayed;
		MatchesQuit = statisticBase.MatchesQuit;
		TimePlayed = statisticBase.TimePlayed;
		Wins = statisticBase.Wins;
		Losses = statisticBase.Losses;
		Kills = statisticBase.Kills;
		Deaths = statisticBase.Deaths;
		Suicides = statisticBase.Suicides;
		MultiKillLevel0 = statisticBase.MultiKillLevel0;
		MultiKillLevel1 = statisticBase.MultiKillLevel1;
		MultiKillLevel2 = statisticBase.MultiKillLevel2;
		MultiKillLevel3 = statisticBase.MultiKillLevel3;
		SpreeKillLevel0 = statisticBase.SpreeKillLevel0;
		SpreeKillLevel1 = statisticBase.SpreeKillLevel1;
		SpreeKillLevel2 = statisticBase.SpreeKillLevel2;
		SpreeKillLevel3 = statisticBase.SpreeKillLevel3;
		SpreeKillLevel4 = statisticBase.SpreeKillLevel4;
		BestShockCombo = statisticBase.BestShockCombo;
		AmazingCombos = statisticBase.AmazingCombos;
		AirRox = statisticBase.AirRox;
		FlakShreds = statisticBase.FlakShreds;
		AirSnot = statisticBase.AirSnot;
		UDamageTime = statisticBase.UDamageTime;
		BerserkTime = statisticBase.BerserkTime;
		InvisibilityTime = statisticBase.InvisibilityTime;
		UDamageCount = statisticBase.UDamageCount;
		BerserkCount = statisticBase.BerserkCount;
		InvisibilityCount = statisticBase.InvisibilityCount;
		BootJumps = statisticBase.BootJumps;
		ShieldBeltCount = statisticBase.ShieldBeltCount;
		ArmorVestCount = statisticBase.ArmorVestCount;
		ArmorPadsCount = statisticBase.ArmorPadsCount;
		HelmetCount = statisticBase.HelmetCount;
		KegCount = statisticBase.KegCount;
		ImpactHammerKills = statisticBase.ImpactHammerKills;
		ImpactHammerDeaths = statisticBase.ImpactHammerDeaths;
		EnforcerKills = statisticBase.EnforcerKills;
		EnforcerDeaths = statisticBase.EnforcerDeaths;
		EnforcerShots = statisticBase.EnforcerShots;
		EnforcerHits = statisticBase.EnforcerHits;
		BioRifleKills = statisticBase.BioRifleKills;
		BioRifleDeaths = statisticBase.BioRifleDeaths;
		BioRifleShots = statisticBase.BioRifleShots;
		BioRifleHits = statisticBase.BioRifleHits;
		BioLauncherKills = statisticBase.BioLauncherKills;
		BioLauncherDeaths = statisticBase.BioLauncherDeaths;
		BioLauncherShots = statisticBase.BioLauncherShots;
		BioLauncherHits = statisticBase.BioLauncherHits;
		ShockBeamKills = statisticBase.ShockBeamKills;
		ShockBeamDeaths = statisticBase.ShockBeamDeaths;
		ShockCoreKills = statisticBase.ShockCoreKills;
		ShockCoreDeaths = statisticBase.ShockCoreDeaths;
		ShockComboKills = statisticBase.ShockComboKills;
		ShockComboDeaths = statisticBase.ShockComboDeaths;
		ShockRifleShots = statisticBase.ShockRifleShots;
		ShockRifleHits = statisticBase.ShockRifleHits;
		LinkKills = statisticBase.LinkKills;
		LinkDeaths = statisticBase.LinkDeaths;
		LinkBeamKills = statisticBase.LinkBeamKills;
		LinkBeamDeaths = statisticBase.LinkBeamDeaths;
		LinkShots = statisticBase.LinkShots;
		LinkHits = statisticBase.LinkHits;
		MinigunKills = statisticBase.MinigunKills;
		MinigunDeaths = statisticBase.MinigunDeaths;
		MinigunShardKills = statisticBase.MinigunShardKills;
		MinigunShardDeaths = statisticBase.MinigunShardDeaths;
		MinigunShots = statisticBase.MinigunShots;
		MinigunHits = statisticBase.MinigunHits;
		FlakShardKills = statisticBase.FlakShardKills;
		FlakShardDeaths = statisticBase.FlakShardDeaths;
		FlakShellKills = statisticBase.FlakShellKills;
		FlakShellDeaths = statisticBase.FlakShellDeaths;
		FlakShots = statisticBase.FlakShots;
		FlakHits = statisticBase.FlakHits;
		RocketKills = statisticBase.RocketKills;
		RocketDeaths = statisticBase.RocketDeaths;
		RocketShots = statisticBase.RocketShots;
		RocketHits = statisticBase.RocketHits;
		SniperKills = statisticBase.SniperKills;
		SniperDeaths = statisticBase.SniperDeaths;
		SniperHeadshotKills = statisticBase.SniperHeadshotKills;
		SniperHeadshotDeaths = statisticBase.SniperHeadshotDeaths;
		SniperShots = statisticBase.SniperShots;
		SniperHits = statisticBase.SniperHits;
		LightningRiflePrimaryKills = statisticBase.LightningRiflePrimaryKills;
		LightningRiflePrimaryDeaths = statisticBase.LightningRiflePrimaryDeaths;
		LightningRifleSecondaryKills = statisticBase.LightningRifleSecondaryKills;
		LightningRifleSecondaryDeaths = statisticBase.LightningRifleSecondaryDeaths;
		LightningRifleShots = statisticBase.LightningRifleShots;
		LightningRifleHits = statisticBase.LightningRifleHits;
		RedeemerKills = statisticBase.RedeemerKills;
		RedeemerDeaths = statisticBase.RedeemerDeaths;
		RedeemerShots = statisticBase.RedeemerShots;
		RedeemerHits = statisticBase.RedeemerHits;
		InstagibKills = statisticBase.InstagibKills;
		InstagibDeaths = statisticBase.InstagibDeaths;
		InstagibShots = statisticBase.InstagibShots;
		InstagibHits = statisticBase.InstagibHits;
		TelefragKills = statisticBase.TelefragKills;
		TelefragDeaths = statisticBase.TelefragDeaths;
		RunDist = statisticBase.RunDist;
		SprintDist = statisticBase.SprintDist;
		InAirDist = statisticBase.InAirDist;
		SwimDist = statisticBase.SwimDist;
		TranslocDist = statisticBase.TranslocDist;
		NumDodges = statisticBase.NumDodges;
		NumWallDodges = statisticBase.NumWallDodges;
		NumJumps = statisticBase.NumJumps;
		NumLiftJumps = statisticBase.NumLiftJumps;
		NumFloorSlides = statisticBase.NumFloorSlides;
		NumWallRuns = statisticBase.NumWallRuns;
		NumImpactJumps = statisticBase.NumImpactJumps;
		NumRocketJumps = statisticBase.NumRocketJumps;
		SlideDist = statisticBase.SlideDist;
		WallRunDist = statisticBase.WallRunDist;
		FlagCaptures = statisticBase.FlagCaptures;
		FlagReturns = statisticBase.FlagReturns;
		FlagAssists = statisticBase.FlagAssists;
		FlagHeldDeny = statisticBase.FlagHeldDeny;
		FlagHeldDenyTime = statisticBase.FlagHeldDenyTime;
		FlagHeldTime = statisticBase.FlagHeldTime;
		FlagReturnPoints = statisticBase.FlagReturnPoints;
		CarryAssist = statisticBase.CarryAssist;
		CarryAssistPoints = statisticBase.CarryAssistPoints;
		FlagCapPoints = statisticBase.FlagCapPoints;
		DefendAssist = statisticBase.DefendAssist;
		DefendAssistPoints = statisticBase.DefendAssistPoints;
		ReturnAssist = statisticBase.ReturnAssist;
		ReturnAssistPoints = statisticBase.ReturnAssistPoints;
		TeamCapPoints = statisticBase.TeamCapPoints;
		EnemyFCDamage = statisticBase.EnemyFCDamage;
		FCKills = statisticBase.FCKills;
		FCKillPoints = statisticBase.FCKillPoints;
		FlagSupportKills = statisticBase.FlagSupportKills;
		FlagSupportKillPoints = statisticBase.FlagSupportKillPoints;
		RegularKillPoints = statisticBase.RegularKillPoints;
		FlagGrabs = statisticBase.FlagGrabs;
		AttackerScore = statisticBase.AttackerScore;
		DefenderScore = statisticBase.DefenderScore;
		SupporterScore = statisticBase.SupporterScore;
		TeamKills = statisticBase.TeamKills;
	}

	#region Quick Look
	[BsonIgnoreIfNull]
	public int? MatchesPlayed { get; set; }

	[BsonIgnoreIfNull]
	public int? MatchesQuit { get; set; }

	[BsonIgnoreIfNull]
	public int? TimePlayed { get; set; }

	[BsonIgnoreIfNull]
	public int? Wins { get; set; }

	[BsonIgnoreIfNull]
	public int? Losses { get; set; }

	[BsonIgnoreIfNull]
	public int? Kills { get; set; }

	[BsonIgnoreIfNull]
	public int? Deaths { get; set; }

	[BsonIgnoreIfNull]
	public int? Suicides { get; set; }
	#endregion

	#region Kill Achievements
	[BsonIgnoreIfNull]
	public int? MultiKillLevel0 { get; set; }

	[BsonIgnoreIfNull]
	public int? MultiKillLevel1 { get; set; }

	[BsonIgnoreIfNull]
	public int? MultiKillLevel2 { get; set; }

	[BsonIgnoreIfNull]
	public int? MultiKillLevel3 { get; set; }

	[BsonIgnoreIfNull]
	public int? SpreeKillLevel0 { get; set; }

	[BsonIgnoreIfNull]
	public int? SpreeKillLevel1 { get; set; }

	[BsonIgnoreIfNull]
	public int? SpreeKillLevel2 { get; set; }

	[BsonIgnoreIfNull]
	public int? SpreeKillLevel3 { get; set; }

	[BsonIgnoreIfNull]
	public int? SpreeKillLevel4 { get; set; }

	[BsonIgnoreIfNull]
	public double? BestShockCombo { get; set; }

	[BsonIgnoreIfNull]
	public int? AmazingCombos { get; set; }

	[BsonIgnoreIfNull]
	public int? AirRox { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShreds { get; set; }

	[BsonIgnoreIfNull]
	public int? AirSnot { get; set; }
	#endregion

	#region Power Up Achievements
	[BsonIgnoreIfNull]
	public double? UDamageTime { get; set; }

	[BsonIgnoreIfNull]
	public double? BerserkTime { get; set; }

	[BsonIgnoreIfNull]
	public double? InvisibilityTime { get; set; }

	[BsonIgnoreIfNull]
	public int? UDamageCount { get; set; }

	[BsonIgnoreIfNull]
	public int? BerserkCount { get; set; }

	[BsonIgnoreIfNull]
	public int? InvisibilityCount { get; set; }

	[BsonIgnoreIfNull]
	public int? BootJumps { get; set; }

	[BsonIgnoreIfNull]
	public int? ShieldBeltCount { get; set; }

	[BsonIgnoreIfNull]
	public int? ArmorVestCount { get; set; }

	[BsonIgnoreIfNull]
	public int? ArmorPadsCount { get; set; }

	[BsonIgnoreIfNull]
	public int? HelmetCount { get; set; }

	[BsonIgnoreIfNull]
	public int? KegCount { get; set; }
	#endregion

	#region Weapon Stats
	[BsonIgnoreIfNull]
	public int? ImpactHammerKills { get; set; }

	[BsonIgnoreIfNull]
	public int? ImpactHammerDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? EnforcerKills { get; set; }

	[BsonIgnoreIfNull]
	public int? EnforcerDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? EnforcerShots { get; set; }

	[BsonIgnoreIfNull]
	public double? EnforcerHits { get; set; }

	[BsonIgnoreIfNull]
	public int? BioRifleKills { get; set; }
	[BsonIgnoreIfNull]
	public int? BioRifleDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? BioRifleShots { get; set; }

	[BsonIgnoreIfNull]
	public double? BioRifleHits { get; set; }

	[BsonIgnoreIfNull]
	public int? BioLauncherKills { get; set; }

	[BsonIgnoreIfNull]
	public int? BioLauncherDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? BioLauncherShots { get; set; }

	[BsonIgnoreIfNull]
	public double? BioLauncherHits { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockBeamKills { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockBeamDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockCoreKills { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockCoreDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockComboKills { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockComboDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? ShockRifleShots { get; set; }

	[BsonIgnoreIfNull]
	public double? ShockRifleHits { get; set; }

	[BsonIgnoreIfNull]
	public int? LinkKills { get; set; }

	[BsonIgnoreIfNull]
	public int? LinkDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? LinkBeamKills { get; set; }

	[BsonIgnoreIfNull]
	public int? LinkBeamDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? LinkShots { get; set; }

	[BsonIgnoreIfNull]
	public double? LinkHits { get; set; }

	[BsonIgnoreIfNull]
	public int? MinigunKills { get; set; }

	[BsonIgnoreIfNull]
	public int? MinigunDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? MinigunShardKills { get; set; }

	[BsonIgnoreIfNull]
	public int? MinigunShardDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? MinigunShots { get; set; }

	[BsonIgnoreIfNull]
	public double? MinigunHits { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShardKills { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShardDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShellKills { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShellDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? FlakShots { get; set; }

	[BsonIgnoreIfNull]
	public double? FlakHits { get; set; }

	[BsonIgnoreIfNull]
	public int? RocketKills { get; set; }

	[BsonIgnoreIfNull]
	public int? RocketDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? RocketShots { get; set; }

	[BsonIgnoreIfNull]
	public double? RocketHits { get; set; }

	[BsonIgnoreIfNull]
	public int? SniperKills { get; set; }

	[BsonIgnoreIfNull]
	public int? SniperDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? SniperHeadshotKills { get; set; }

	[BsonIgnoreIfNull]
	public int? SniperHeadshotDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? SniperShots { get; set; }

	[BsonIgnoreIfNull]
	public double? SniperHits { get; set; }

	[BsonIgnoreIfNull]
	public int? LightningRiflePrimaryKills { get; set; }

	[BsonIgnoreIfNull]
	public int? LightningRiflePrimaryDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? LightningRifleSecondaryKills { get; set; }

	[BsonIgnoreIfNull]
	public int? LightningRifleSecondaryDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? LightningRifleShots { get; set; }

	[BsonIgnoreIfNull]
	public double? LightningRifleHits { get; set; }

	[BsonIgnoreIfNull]
	public int? RedeemerKills { get; set; }

	[BsonIgnoreIfNull]
	public int? RedeemerDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? RedeemerShots { get; set; }

	[BsonIgnoreIfNull]
	public double? RedeemerHits { get; set; }

	[BsonIgnoreIfNull]
	public int? InstagibKills { get; set; }

	[BsonIgnoreIfNull]
	public int? InstagibDeaths { get; set; }

	[BsonIgnoreIfNull]
	public int? InstagibShots { get; set; }

	[BsonIgnoreIfNull]
	public double? InstagibHits { get; set; }

	[BsonIgnoreIfNull]
	public int? TelefragKills { get; set; }

	[BsonIgnoreIfNull]
	public int? TelefragDeaths { get; set; }
	#endregion

	#region Miscellaneous - Movement
	[BsonIgnoreIfNull]
	public double? RunDist { get; set; }

	[BsonIgnoreIfNull]
	public double? SprintDist { get; set; }

	[BsonIgnoreIfNull]
	public double? InAirDist { get; set; }

	[BsonIgnoreIfNull]
	public double? SwimDist { get; set; }

	[BsonIgnoreIfNull]
	public double? TranslocDist { get; set; }

	[BsonIgnoreIfNull]
	public int? NumDodges { get; set; }

	[BsonIgnoreIfNull]
	public int? NumWallDodges { get; set; }

	[BsonIgnoreIfNull]
	public int? NumJumps { get; set; }

	[BsonIgnoreIfNull]
	public int? NumLiftJumps { get; set; }

	[BsonIgnoreIfNull]
	public int? NumFloorSlides { get; set; }

	[BsonIgnoreIfNull]
	public int? NumWallRuns { get; set; }

	[BsonIgnoreIfNull]
	public int? NumImpactJumps { get; set; }

	[BsonIgnoreIfNull]
	public int? NumRocketJumps { get; set; }

	[BsonIgnoreIfNull]
	public double? SlideDist { get; set; }

	[BsonIgnoreIfNull]
	public double? WallRunDist { get; set; }
	#endregion

	#region Miscellaneous - Capture the Flag
	[BsonIgnoreIfNull]
	public int? FlagCaptures { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagReturns { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagAssists { get; set; }

	[BsonIgnoreIfNull]
	public double? FlagHeldDeny { get; set; }

	[BsonIgnoreIfNull]
	public double? FlagHeldDenyTime { get; set; }

	[BsonIgnoreIfNull]
	public double? FlagHeldTime { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagReturnPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? CarryAssist { get; set; }

	[BsonIgnoreIfNull]
	public int? CarryAssistPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagCapPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? DefendAssist { get; set; }

	[BsonIgnoreIfNull]
	public int? DefendAssistPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? ReturnAssist { get; set; }

	[BsonIgnoreIfNull]
	public int? ReturnAssistPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? TeamCapPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? EnemyFCDamage { get; set; }

	[BsonIgnoreIfNull]
	public int? FCKills { get; set; }

	[BsonIgnoreIfNull]
	public int? FCKillPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagSupportKills { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagSupportKillPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? RegularKillPoints { get; set; }

	[BsonIgnoreIfNull]
	public int? FlagGrabs { get; set; }

	[BsonIgnoreIfNull]
	public double? AttackerScore { get; set; }

	[BsonIgnoreIfNull]
	public double? DefenderScore { get; set; }

	[BsonIgnoreIfNull]
	public double? SupporterScore { get; set; }

	[BsonIgnoreIfNull]
	public int? TeamKills { get; set; }
	#endregion

	#region Validation
	public List<string> Validate()
	{
		var flaggedFields = new List<string>();

		var totalKills = new int?[]
		{
			ImpactHammerKills,
			EnforcerKills,
			BioRifleKills,
			BioLauncherKills,
			ShockBeamKills,
			ShockCoreKills,
			ShockComboKills,
			LinkKills,
			LinkBeamKills,
			MinigunKills,
			MinigunShardKills,
			FlakShardKills,
			FlakShellKills,
			RocketKills,
			SniperKills,
			SniperHeadshotKills,
			LightningRiflePrimaryKills,
			LightningRifleSecondaryKills,
			RedeemerKills,
			InstagibKills,
			TelefragKills,
		}.Sum() ?? 0;

		#region Quick Look Validations
		// These statistics cannot be more than 1
		ValidateMaxValueForStatistic(MatchesPlayed, 1, nameof(MatchesPlayed), flaggedFields);
		ValidateMaxValueForStatistic(MatchesQuit, 1, nameof(MatchesQuit), flaggedFields);
		ValidateMaxValueForStatistic(Wins, 1, nameof(Wins), flaggedFields);
		ValidateMaxValueForStatistic(Losses, 1, nameof(Losses), flaggedFields);

		// These statistics cannot exist at the same time
		ValidateIfBothStatisticsExist(MatchesPlayed, MatchesQuit, nameof(MatchesPlayed), nameof(MatchesQuit), flaggedFields);
		ValidateIfBothStatisticsExist(Wins, Losses, nameof(Wins), nameof(Losses), flaggedFields);

		// Maximum time for custom game is 60 minutes (1 minute is added just to be sure)
		if (TimePlayed.HasValue && TimePlayed.Value > 60 * 60 + 60)
		{
			flaggedFields.Add(nameof(TimePlayed));
		}

		if (Kills.HasValue)
		{
			// Kills are reported if you manage to achieve 1 kill each 1.5 second
			ValidateStatisticsAgainstTimePlayed(Kills, 1.5, nameof(Kills), flaggedFields);

			// Kills are reported if they don't match the sum of individual weapon kills
			if (Kills.Value != totalKills)
			{
				flaggedFields.Add(nameof(Kills));
			}
		}

		// Deaths/suicides are reported if you manage to achieve 1 each 2 seconds
		ValidateStatisticsAgainstTimePlayed(Deaths, 2, nameof(Deaths), flaggedFields);
		ValidateStatisticsAgainstTimePlayed(Suicides, 2, nameof(Suicides), flaggedFields);
		#endregion

		#region Kill Achievements Validations
		// Cannot have double/multi/ultra kills without or more than actual kills
		ValidateMultiAndSpreeKillStatistic(MultiKillLevel0, 2, nameof(MultiKillLevel0), flaggedFields);
		ValidateMultiAndSpreeKillStatistic(MultiKillLevel1, 3, nameof(MultiKillLevel1), flaggedFields);
		ValidateMultiAndSpreeKillStatistic(MultiKillLevel2, 4, nameof(MultiKillLevel2), flaggedFields);

		// Cannot have killing spree/rampages/dominatings/unstoppables without or more than actual kills
		ValidateMultiAndSpreeKillStatistic(SpreeKillLevel0, 5, nameof(SpreeKillLevel0), flaggedFields);
		ValidateMultiAndSpreeKillStatistic(SpreeKillLevel1, 10, nameof(SpreeKillLevel1), flaggedFields);
		ValidateMultiAndSpreeKillStatistic(SpreeKillLevel2, 15, nameof(SpreeKillLevel2), flaggedFields);
		ValidateMultiAndSpreeKillStatistic(SpreeKillLevel3, 20, nameof(SpreeKillLevel3), flaggedFields);
		#endregion

		#region Power Up Achievements Validations
		// UDamage/Berserk/Invisibility time cannot be longer than match time
		ValidateTimeStatistic(UDamageTime, nameof(UDamageTime), flaggedFields);
		ValidateTimeStatistic(BerserkTime, nameof(BerserkTime), flaggedFields);
		ValidateTimeStatistic(InvisibilityTime, nameof(InvisibilityTime), flaggedFields);
		#endregion

		#region Weapon Stats Validations
		// Per-weapon kills are reported if they are more than the actual kills or if actual kills are missing
		ValidatePerWeaponKillStatistic(ImpactHammerKills, nameof(ImpactHammerKills), flaggedFields);
		ValidatePerWeaponKillStatistic(EnforcerKills, nameof(EnforcerKills), flaggedFields);
		ValidatePerWeaponKillStatistic(BioRifleKills, nameof(BioRifleKills), flaggedFields);
		ValidatePerWeaponKillStatistic(BioLauncherKills, nameof(BioLauncherKills), flaggedFields);
		ValidatePerWeaponKillStatistic(ShockBeamKills, nameof(ShockBeamKills), flaggedFields);
		ValidatePerWeaponKillStatistic(ShockCoreKills, nameof(ShockCoreKills), flaggedFields);
		ValidatePerWeaponKillStatistic(ShockComboKills, nameof(ShockComboKills), flaggedFields);
		ValidatePerWeaponKillStatistic(LinkKills, nameof(LinkKills), flaggedFields);
		ValidatePerWeaponKillStatistic(LinkBeamKills, nameof(LinkBeamKills), flaggedFields);
		ValidatePerWeaponKillStatistic(MinigunKills, nameof(MinigunKills), flaggedFields);
		ValidatePerWeaponKillStatistic(MinigunShardKills, nameof(MinigunShardKills), flaggedFields);
		ValidatePerWeaponKillStatistic(FlakShardKills, nameof(FlakShardKills), flaggedFields);
		ValidatePerWeaponKillStatistic(FlakShellKills, nameof(FlakShellKills), flaggedFields);
		ValidatePerWeaponKillStatistic(RocketKills, nameof(RocketKills), flaggedFields);
		ValidatePerWeaponKillStatistic(SniperKills, nameof(SniperKills), flaggedFields);
		ValidatePerWeaponKillStatistic(SniperHeadshotKills, nameof(SniperHeadshotKills), flaggedFields);
		ValidatePerWeaponKillStatistic(LightningRiflePrimaryKills, nameof(LightningRiflePrimaryKills), flaggedFields);
		ValidatePerWeaponKillStatistic(LightningRifleSecondaryKills, nameof(LightningRifleSecondaryKills), flaggedFields);
		ValidatePerWeaponKillStatistic(RedeemerKills, nameof(RedeemerKills), flaggedFields);
		ValidatePerWeaponKillStatistic(InstagibKills, nameof(InstagibKills), flaggedFields);
		ValidatePerWeaponKillStatistic(TelefragKills, nameof(TelefragKills), flaggedFields);
		#endregion

		return flaggedFields;
	}

	private static void ValidateMaxValueForStatistic(int? value, int maxValue, string statisticName, List<string> flaggedFields)
	{
		if (value.HasValue && value.Value > maxValue)
		{
			flaggedFields.Add(statisticName);
		}
	}

	private static void ValidateIfBothStatisticsExist(int? firstValue, int? secondValue, string firstStatistic, string secondStatistic, List<string> flaggedFields)
	{
		if (firstValue.HasValue && secondValue.HasValue && firstValue.Value > 0 && secondValue.Value > 0)
		{
			flaggedFields.Add(firstStatistic);
			flaggedFields.Add(secondStatistic);
		}
	}

	private void ValidateStatisticsAgainstTimePlayed(int? value, double maxValuePerSecond, string statisticName, List<string> flaggedFields)
	{
		if (value.HasValue && TimePlayed.HasValue && value.Value > TimePlayed.Value / maxValuePerSecond)
		{
			flaggedFields.Add(statisticName);
			flaggedFields.Add(nameof(TimePlayed));
		}
	}

	private void ValidateMultiAndSpreeKillStatistic(int? value, int multiplier, string statisticName, List<string> flaggedFields)
	{
		if (value.HasValue && (!Kills.HasValue || Kills.HasValue && value.Value * multiplier > Kills.Value))
		{
			flaggedFields.Add(statisticName);
			flaggedFields.Add(nameof(Kills));
		}
	}

	private void ValidateTimeStatistic(double? value, string statisticName, List<string> flaggedFields)
	{
		if (value.HasValue && TimePlayed.HasValue && value.Value > TimePlayed.Value)
		{
			flaggedFields.Add(statisticName);
			flaggedFields.Add(nameof(TimePlayed));
		}
	}

	private void ValidatePerWeaponKillStatistic(int? value, string statisticName, List<string> flaggedFields)
	{
		if (value.HasValue && (!Kills.HasValue || value.Value > Kills.Value))
		{
			flaggedFields.Add(statisticName);
			flaggedFields.Add(nameof(Kills));
		}
	}

	#endregion
}
