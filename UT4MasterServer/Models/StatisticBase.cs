using MongoDB.Bson.Serialization.Attributes;

namespace UT4MasterServer.Models;

public class StatisticBase
{
	public readonly static List<string> StatisticProperties = new()
	{
		"skillrating",
		"tdmskillrating",
		"ctfskillrating",
		"dmskillrating",
		"showdownskillrating",
		"flagrunskillrating",
		"rankedduelskillrating",
		"rankedctfskillrating",
		"rankedshowdownskillrating",
		"rankedflagrunskillrating",
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
		SkillRating = statisticBase.SkillRating;
		TDMSkillRating = statisticBase.TDMSkillRating;
		CTFSkillRating = statisticBase.CTFSkillRating;
		DMSkillRating = statisticBase.DMSkillRating;
		ShowdownSkillRating = statisticBase.ShowdownSkillRating;
		FlagRunSkillRating = statisticBase.FlagRunSkillRating;
		RankedDuelSkillRating = statisticBase.RankedDuelSkillRating;
		RankedCTFSkillRating = statisticBase.RankedCTFSkillRating;
		RankedShowdownSkillRating = statisticBase.RankedShowdownSkillRating;
		RankedFlagRunSkillRating = statisticBase.RankedFlagRunSkillRating;
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
			SniperKills, // SniperHeadshotKills are already included in SniperKills
			LightningRiflePrimaryKills,
			LightningRifleSecondaryKills,
			RedeemerKills,
			InstagibKills,
			TelefragKills
		}.Sum();

		#region Quick Look Validations

		// Since this is added after match, this property should be only 1
		if (MatchesPlayed.HasValue && MatchesPlayed.Value > 1)
		{
			flaggedFields.Add(nameof(MatchesPlayed));
		}
		// Since this is added after match, this property should be only 1
		if (MatchesQuit.HasValue && MatchesQuit.Value > 1)
		{
			flaggedFields.Add(nameof(MatchesQuit));
		}
		// You shouldn't have both Wins and Losses value available at the same time
		if (MatchesPlayed.HasValue && MatchesQuit.HasValue && MatchesPlayed.Value > 0 && MatchesQuit.Value > 0)
		{
			flaggedFields.Add(nameof(MatchesPlayed));
			flaggedFields.Add(nameof(MatchesQuit));
		}
		// Since this is added after match, this property should be only 1
		if (Wins.HasValue && Wins.Value > 1)
		{
			flaggedFields.Add(nameof(Wins));
		}
		// Since this is added after match, this property should be only 1
		if (Losses.HasValue && Losses.Value > 1)
		{
			flaggedFields.Add(nameof(Losses));
		}
		// You shouldn't have both Wins and Losses value available at the same time
		if (Wins.HasValue && Losses.HasValue && Wins.Value > 0 && Losses.Value > 0)
		{
			flaggedFields.Add(nameof(Wins));
			flaggedFields.Add(nameof(Losses));
		}
		// Maximum time for custom game is 60 minutes (1 minute is added just to be sure)
		if (TimePlayed.HasValue && TimePlayed.Value > (60 * 60) + 60)
		{
			flaggedFields.Add(nameof(TimePlayed));
		}
		if (Kills.HasValue)
		{
			// Kills are reported if you manage to achieve 1 kill each 5 seconds
			if (TimePlayed.HasValue && Kills.Value > (TimePlayed.Value / 5))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(TimePlayed));
			}
			// Kills are reported if they don't match the sum of individual weapon kills
			if (Kills.Value != totalKills.Value)
			{
				flaggedFields.Add(nameof(Kills));
			}
		}
		// Deaths are reported if you manage to achieve 1 death each 5 seconds
		if (Deaths.HasValue && TimePlayed.HasValue && Deaths.Value > (TimePlayed.Value / 5))
		{
			flaggedFields.Add(nameof(Deaths));
		}
		// Suicides are reported if you manage to achieve 1 suicide each 5 seconds
		if (Suicides.HasValue && TimePlayed.HasValue && Suicides.Value > (TimePlayed.Value / 5))
		{
			flaggedFields.Add(nameof(Suicides));
		}

		#endregion

		#region Kill Achievements Validations

		// Cannot have double kills without or more than actual kills                                                                   
		if (MultiKillLevel0.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && MultiKillLevel0.Value * 2 < Kills.Value))
			{
				flaggedFields.Add(nameof(MultiKillLevel0));
			}
		}
		// Cannot have multi kills without or more than actual kills
		if (MultiKillLevel1.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && MultiKillLevel1.Value * 3 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(MultiKillLevel1));
			}
		}
		// Cannot have ultra kills without or more than actual kills
		if (MultiKillLevel2.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && MultiKillLevel2.Value * 4 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(MultiKillLevel2));
			}
		}
		// Cannot have monster kills without or more than actual kills
		if (MultiKillLevel3.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && MultiKillLevel3.Value * 5 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(MultiKillLevel3));
			}
		}
		// Cannot have killing spree without or more than actual kills
		if (SpreeKillLevel0.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && SpreeKillLevel0.Value * 5 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(SpreeKillLevel0));
			}
		}
		// Cannot have rampages without or more than actual kills
		if (SpreeKillLevel1.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && SpreeKillLevel1.Value * 10 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(SpreeKillLevel1));
			}
		}
		// Cannot have dominatings without or more than actual kills
		if (SpreeKillLevel2.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && SpreeKillLevel2.Value * 15 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(SpreeKillLevel2));
			}
		}
		// Cannot have unstoppables without or more than actual kills
		if (SpreeKillLevel3.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && SpreeKillLevel3.Value * 20 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(SpreeKillLevel3));
			}
		}
		// Cannot have godlikes without or more than actual kills
		if (SpreeKillLevel4.HasValue)
		{
			if (!Kills.HasValue || (Kills.HasValue && SpreeKillLevel4.Value * 25 < Kills.Value))
			{
				flaggedFields.Add(nameof(Kills));
				flaggedFields.Add(nameof(SpreeKillLevel4));
			}
		}

		#endregion

		#region Power Up Achievements Validations

		if (UDamageTime.HasValue)
		{
			// UDamage time cannot be longer than match time
			if (TimePlayed.HasValue && UDamageTime.Value > TimePlayed.Value)
			{
				flaggedFields.Add(nameof(TimePlayed));
				flaggedFields.Add(nameof(UDamageTime));
			}
		}
		if (BerserkTime.HasValue)
		{
			// BerserkTime time cannot be longer than match time
			if (TimePlayed.HasValue && BerserkTime.Value > TimePlayed.Value)
			{
				flaggedFields.Add(nameof(TimePlayed));
				flaggedFields.Add(nameof(BerserkTime));
			}
		}
		if (InvisibilityTime.HasValue)
		{
			// InvisibilityTime time cannot be longer than match time
			if (TimePlayed.HasValue && InvisibilityTime.Value > TimePlayed.Value)
			{
				flaggedFields.Add(nameof(TimePlayed));
				flaggedFields.Add(nameof(InvisibilityTime));
			}
		}

		#endregion

		#region Weapon Stats Validations

		// Impact Hammer kills are reported if they are more than the actual kills
		if (ImpactHammerKills.HasValue && Kills.HasValue && ImpactHammerKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(ImpactHammerKills));
		}
		// Enforcer kills are reported if they are more than the actual kills
		if (EnforcerKills.HasValue && Kills.HasValue && EnforcerKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(EnforcerKills));
		}
		// Bio Rifle kills are reported if they are more than the actual kills
		if (BioRifleKills.HasValue && Kills.HasValue && BioRifleKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(BioRifleKills));
		}
		// Grenade Launcher kills are reported if they are more than the actual kills
		if (BioLauncherKills.HasValue && Kills.HasValue && BioLauncherKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(BioLauncherKills));
		}
		// Shock Beam kills are reported if they are more than the actual kills
		if (ShockBeamKills.HasValue && Kills.HasValue && ShockBeamKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(ShockBeamKills));
		}
		// Shock Core kills are reported if they are more than the actual kills
		if (ShockCoreKills.HasValue && Kills.HasValue && ShockCoreKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(ShockCoreKills));
		}
		// Shock Combo kills are reported if they are more than the actual kills
		if (ShockComboKills.HasValue && Kills.HasValue && ShockComboKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(ShockComboKills));
		}
		// Link kills are reported if they are more than the actual kills
		if (LinkKills.HasValue && Kills.HasValue && LinkKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(LinkKills));
		}
		// Link Beam kills are reported if they are more than the actual kills
		if (LinkBeamKills.HasValue && Kills.HasValue && LinkBeamKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(LinkBeamKills));
		}
		// Minigun kills are reported if they are more than the actual kills
		if (MinigunKills.HasValue && Kills.HasValue && MinigunKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(MinigunKills));
		}
		// Minigun Shard kills are reported if they are more than the actual kills
		if (MinigunShardKills.HasValue && Kills.HasValue && MinigunShardKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(MinigunShardKills));
		}
		// Flak Shard kills are reported if they are more than the actual kills
		if (FlakShardKills.HasValue && Kills.HasValue && FlakShardKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(FlakShardKills));
		}
		// Flak Shell kills are reported if they are more than the actual kills
		if (FlakShellKills.HasValue && Kills.HasValue && FlakShellKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(FlakShellKills));
		}
		// Rocket kills are reported if they are more than the actual kills
		if (RocketKills.HasValue && Kills.HasValue && RocketKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(RocketKills));
		}
		// Sniper kills are reported if they are more than the actual kills
		if (SniperKills.HasValue && Kills.HasValue && SniperKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(SniperKills));
		}
		// Sniper Headshot kills are reported if they are more than the actual kills
		if (SniperHeadshotKills.HasValue && Kills.HasValue && SniperHeadshotKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(SniperHeadshotKills));
		}
		// Lightning Rifle Primary kills are reported if they are more than the actual kills
		if (LightningRiflePrimaryKills.HasValue && Kills.HasValue && LightningRiflePrimaryKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(LightningRiflePrimaryKills));
		}
		// Lightning Rifle Secondary kills are reported if they are more than the actual kills
		if (LightningRifleSecondaryKills.HasValue && Kills.HasValue && LightningRifleSecondaryKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(LightningRifleSecondaryKills));
		}
		// Redeemer kills are reported if they are more than the actual kills
		if (RedeemerKills.HasValue && Kills.HasValue && RedeemerKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(RedeemerKills));
		}
		// Instagib kills are reported if they are more than the actual kills
		if (InstagibKills.HasValue && Kills.HasValue && InstagibKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(InstagibKills));
		}
		// Telefrag kills are reported if they are more than the actual kills
		if (TelefragKills.HasValue && Kills.HasValue && TelefragKills.Value > Kills.Value)
		{
			flaggedFields.Add(nameof(TelefragKills));
		}

		#endregion

		return flaggedFields;
	}

	#region Quick Look
	[BsonElement("skillRating"), BsonIgnoreIfNull]
	public int? SkillRating { get; set; }

	[BsonElement("tdmSkillRating"), BsonIgnoreIfNull]
	public int? TDMSkillRating { get; set; }

	[BsonElement("ctfSkillRating"), BsonIgnoreIfNull]
	public int? CTFSkillRating { get; set; }

	[BsonElement("dmSkillRating"), BsonIgnoreIfNull]
	public int? DMSkillRating { get; set; }

	[BsonElement("showdownSkillRating"), BsonIgnoreIfNull]
	public int? ShowdownSkillRating { get; set; }

	[BsonElement("flagRunSkilLRating"), BsonIgnoreIfNull]
	public int? FlagRunSkillRating { get; set; }

	[BsonElement("rankedDuelSkillRating"), BsonIgnoreIfNull]
	public int? RankedDuelSkillRating { get; set; }

	[BsonElement("rankedCtfSkillRating"), BsonIgnoreIfNull]
	public int? RankedCTFSkillRating { get; set; }

	[BsonElement("rankedShowdownSkillRating"), BsonIgnoreIfNull]
	public int? RankedShowdownSkillRating { get; set; }

	[BsonElement("rankedFlagRunSkillRating"), BsonIgnoreIfNull]
	public int? RankedFlagRunSkillRating { get; set; }

	[BsonElement("matchesPlayed"), BsonIgnoreIfNull]
	public int? MatchesPlayed { get; set; }

	[BsonElement("matchesQuit"), BsonIgnoreIfNull]
	public int? MatchesQuit { get; set; }

	[BsonElement("timePlayed"), BsonIgnoreIfNull]
	public int? TimePlayed { get; set; }

	[BsonElement("wins"), BsonIgnoreIfNull]
	public int? Wins { get; set; }

	[BsonElement("losses"), BsonIgnoreIfNull]
	public int? Losses { get; set; }

	[BsonElement("kills"), BsonIgnoreIfNull]
	public int? Kills { get; set; }

	[BsonElement("deaths"), BsonIgnoreIfNull]
	public int? Deaths { get; set; }

	[BsonElement("suicides"), BsonIgnoreIfNull]
	public int? Suicides { get; set; }
	#endregion

	#region Kill Achievements
	[BsonElement("multiKillLevel0"), BsonIgnoreIfNull]
	public int? MultiKillLevel0 { get; set; }

	[BsonElement("multiKillLevel1"), BsonIgnoreIfNull]
	public int? MultiKillLevel1 { get; set; }

	[BsonElement("multiKillLevel2"), BsonIgnoreIfNull]
	public int? MultiKillLevel2 { get; set; }

	[BsonElement("multiKillLevel3"), BsonIgnoreIfNull]
	public int? MultiKillLevel3 { get; set; }

	[BsonElement("spreeKillLevel0"), BsonIgnoreIfNull]
	public int? SpreeKillLevel0 { get; set; }

	[BsonElement("spreeKillLevel1"), BsonIgnoreIfNull]
	public int? SpreeKillLevel1 { get; set; }

	[BsonElement("spreeKillLevel2"), BsonIgnoreIfNull]
	public int? SpreeKillLevel2 { get; set; }

	[BsonElement("spreeKillLevel3"), BsonIgnoreIfNull]
	public int? SpreeKillLevel3 { get; set; }

	[BsonElement("spreeKillLevel4"), BsonIgnoreIfNull]
	public int? SpreeKillLevel4 { get; set; }

	[BsonElement("bestShockCombo"), BsonIgnoreIfNull]
	public double? BestShockCombo { get; set; }

	[BsonElement("amazingCombos"), BsonIgnoreIfNull]
	public int? AmazingCombos { get; set; }

	[BsonElement("airRox"), BsonIgnoreIfNull]
	public int? AirRox { get; set; }

	[BsonElement("flakShreds"), BsonIgnoreIfNull]
	public int? FlakShreds { get; set; }

	[BsonElement("airSnot"), BsonIgnoreIfNull]
	public int? AirSnot { get; set; }
	#endregion

	#region Power Up Achievements
	[BsonElement("uDamageTime"), BsonIgnoreIfNull]
	public double? UDamageTime { get; set; }

	[BsonElement("berserkTime"), BsonIgnoreIfNull]
	public double? BerserkTime { get; set; }

	[BsonElement("invisibilityTime"), BsonIgnoreIfNull]
	public double? InvisibilityTime { get; set; }

	[BsonElement("uDamageCount"), BsonIgnoreIfNull]
	public int? UDamageCount { get; set; }

	[BsonElement("berserkCount"), BsonIgnoreIfNull]
	public int? BerserkCount { get; set; }

	[BsonElement("invisibilityCount"), BsonIgnoreIfNull]
	public int? InvisibilityCount { get; set; }

	[BsonElement("bootJumps"), BsonIgnoreIfNull]
	public int? BootJumps { get; set; }

	[BsonElement("shieldBeltCount"), BsonIgnoreIfNull]
	public int? ShieldBeltCount { get; set; }

	[BsonElement("armorVestCount"), BsonIgnoreIfNull]
	public int? ArmorVestCount { get; set; }

	[BsonElement("armorPadsCount"), BsonIgnoreIfNull]
	public int? ArmorPadsCount { get; set; }

	[BsonElement("helmetCount"), BsonIgnoreIfNull]
	public int? HelmetCount { get; set; }

	[BsonElement("kegCount"), BsonIgnoreIfNull]
	public int? KegCount { get; set; }
	#endregion

	#region Weapon Stats
	[BsonElement("impactHammerKills"), BsonIgnoreIfNull]
	public int? ImpactHammerKills { get; set; }

	[BsonElement("impactHammerDeaths"), BsonIgnoreIfNull]
	public int? ImpactHammerDeaths { get; set; }

	[BsonElement("enforcerKills"), BsonIgnoreIfNull]
	public int? EnforcerKills { get; set; }

	[BsonElement("enforcerDeaths"), BsonIgnoreIfNull]
	public int? EnforcerDeaths { get; set; }

	[BsonElement("enforcerShots"), BsonIgnoreIfNull]
	public int? EnforcerShots { get; set; }

	[BsonElement("enforcerHits"), BsonIgnoreIfNull]
	public double? EnforcerHits { get; set; }

	[BsonElement("bioRifleKills"), BsonIgnoreIfNull]
	public int? BioRifleKills { get; set; }
	[BsonElement("bioRifleDeaths"), BsonIgnoreIfNull]
	public int? BioRifleDeaths { get; set; }

	[BsonElement("bioRifleShots"), BsonIgnoreIfNull]
	public int? BioRifleShots { get; set; }

	[BsonElement("bioRifleHits"), BsonIgnoreIfNull]
	public double? BioRifleHits { get; set; }

	[BsonElement("bioLauncherKills"), BsonIgnoreIfNull]
	public int? BioLauncherKills { get; set; }

	[BsonElement("bioLauncherDeaths"), BsonIgnoreIfNull]
	public int? BioLauncherDeaths { get; set; }

	[BsonElement("bioLauncherShots"), BsonIgnoreIfNull]
	public int? BioLauncherShots { get; set; }

	[BsonElement("bioLauncherHits"), BsonIgnoreIfNull]
	public double? BioLauncherHits { get; set; }

	[BsonElement("shockBeamKills"), BsonIgnoreIfNull]
	public int? ShockBeamKills { get; set; }

	[BsonElement("shockBeamDeaths"), BsonIgnoreIfNull]
	public int? ShockBeamDeaths { get; set; }

	[BsonElement("shockCoreKills"), BsonIgnoreIfNull]
	public int? ShockCoreKills { get; set; }

	[BsonElement("shockCoreDeaths"), BsonIgnoreIfNull]
	public int? ShockCoreDeaths { get; set; }

	[BsonElement("shockComboKills"), BsonIgnoreIfNull]
	public int? ShockComboKills { get; set; }

	[BsonElement("shockComboDeaths"), BsonIgnoreIfNull]
	public int? ShockComboDeaths { get; set; }

	[BsonElement("shockRifleShots"), BsonIgnoreIfNull]
	public int? ShockRifleShots { get; set; }

	[BsonElement("shockRifleHits"), BsonIgnoreIfNull]
	public double? ShockRifleHits { get; set; }

	[BsonElement("linkKills"), BsonIgnoreIfNull]
	public int? LinkKills { get; set; }

	[BsonElement("linkDeaths"), BsonIgnoreIfNull]
	public int? LinkDeaths { get; set; }

	[BsonElement("linkBeamKills"), BsonIgnoreIfNull]
	public int? LinkBeamKills { get; set; }

	[BsonElement("linkBeamDeaths"), BsonIgnoreIfNull]
	public int? LinkBeamDeaths { get; set; }

	[BsonElement("linkShots"), BsonIgnoreIfNull]
	public int? LinkShots { get; set; }

	[BsonElement("linkHits"), BsonIgnoreIfNull]
	public double? LinkHits { get; set; }

	[BsonElement("minigunKills"), BsonIgnoreIfNull]
	public int? MinigunKills { get; set; }

	[BsonElement("minigunDeaths"), BsonIgnoreIfNull]
	public int? MinigunDeaths { get; set; }

	[BsonElement("minigunShardKills"), BsonIgnoreIfNull]
	public int? MinigunShardKills { get; set; }

	[BsonElement("minigunShardDeaths"), BsonIgnoreIfNull]
	public int? MinigunShardDeaths { get; set; }

	[BsonElement("minigunShots"), BsonIgnoreIfNull]
	public int? MinigunShots { get; set; }

	[BsonElement("minigunHits"), BsonIgnoreIfNull]
	public double? MinigunHits { get; set; }

	[BsonElement("flakShardKills"), BsonIgnoreIfNull]
	public int? FlakShardKills { get; set; }

	[BsonElement("flakShardDeaths"), BsonIgnoreIfNull]
	public int? FlakShardDeaths { get; set; }

	[BsonElement("flakShellKills"), BsonIgnoreIfNull]
	public int? FlakShellKills { get; set; }

	[BsonElement("flakShellDeaths"), BsonIgnoreIfNull]
	public int? FlakShellDeaths { get; set; }

	[BsonElement("flakShots"), BsonIgnoreIfNull]
	public int? FlakShots { get; set; }

	[BsonElement("flakHits"), BsonIgnoreIfNull]
	public double? FlakHits { get; set; }

	[BsonElement("rocketKills"), BsonIgnoreIfNull]
	public int? RocketKills { get; set; }

	[BsonElement("rocketDeaths"), BsonIgnoreIfNull]
	public int? RocketDeaths { get; set; }

	[BsonElement("rocketShots"), BsonIgnoreIfNull]
	public int? RocketShots { get; set; }

	[BsonElement("rocketHits"), BsonIgnoreIfNull]
	public double? RocketHits { get; set; }

	[BsonElement("sniperKills"), BsonIgnoreIfNull]
	public int? SniperKills { get; set; }

	[BsonElement("sniperDeaths"), BsonIgnoreIfNull]
	public int? SniperDeaths { get; set; }

	[BsonElement("sniperHeadshotKills"), BsonIgnoreIfNull]
	public int? SniperHeadshotKills { get; set; }

	[BsonElement("sniperHeadshotDeaths"), BsonIgnoreIfNull]
	public int? SniperHeadshotDeaths { get; set; }

	[BsonElement("sniperShots"), BsonIgnoreIfNull]
	public int? SniperShots { get; set; }

	[BsonElement("sniperHits"), BsonIgnoreIfNull]
	public double? SniperHits { get; set; }

	[BsonElement("lightningRiflePrimaryKills"), BsonIgnoreIfNull]
	public int? LightningRiflePrimaryKills { get; set; }

	[BsonElement("lightningRiflePrimaryDeaths"), BsonIgnoreIfNull]
	public int? LightningRiflePrimaryDeaths { get; set; }

	[BsonElement("lightningRifleSecondaryKills"), BsonIgnoreIfNull]
	public int? LightningRifleSecondaryKills { get; set; }

	[BsonElement("lightningRifleSecondaryDeaths"), BsonIgnoreIfNull]
	public int? LightningRifleSecondaryDeaths { get; set; }

	[BsonElement("lightningRifleShots"), BsonIgnoreIfNull]
	public int? LightningRifleShots { get; set; }

	[BsonElement("lightningRifleHits"), BsonIgnoreIfNull]
	public double? LightningRifleHits { get; set; }

	[BsonElement("redeemerKills"), BsonIgnoreIfNull]
	public int? RedeemerKills { get; set; }

	[BsonElement("redeemerDeaths"), BsonIgnoreIfNull]
	public int? RedeemerDeaths { get; set; }

	[BsonElement("redeemerShots"), BsonIgnoreIfNull]
	public int? RedeemerShots { get; set; }

	[BsonElement("redeemerHits"), BsonIgnoreIfNull]
	public double? RedeemerHits { get; set; }

	[BsonElement("instagibKills"), BsonIgnoreIfNull]
	public int? InstagibKills { get; set; }

	[BsonElement("instagibDeaths"), BsonIgnoreIfNull]
	public int? InstagibDeaths { get; set; }

	[BsonElement("instagibShots"), BsonIgnoreIfNull]
	public int? InstagibShots { get; set; }

	[BsonElement("instagibHits"), BsonIgnoreIfNull]
	public double? InstagibHits { get; set; }

	[BsonElement("telefragKills"), BsonIgnoreIfNull]
	public int? TelefragKills { get; set; }

	[BsonElement("telefragDeaths"), BsonIgnoreIfNull]
	public int? TelefragDeaths { get; set; }
	#endregion

	#region Miscellaneous - Movement
	[BsonElement("runDist"), BsonIgnoreIfNull]
	public double? RunDist { get; set; }

	[BsonElement("sprintDist"), BsonIgnoreIfNull]
	public double? SprintDist { get; set; }

	[BsonElement("inAirDist"), BsonIgnoreIfNull]
	public double? InAirDist { get; set; }

	[BsonElement("swimDist"), BsonIgnoreIfNull]
	public double? SwimDist { get; set; }

	[BsonElement("translocDist"), BsonIgnoreIfNull]
	public double? TranslocDist { get; set; }

	[BsonElement("numDodges"), BsonIgnoreIfNull]
	public int? NumDodges { get; set; }

	[BsonElement("numWallDodges"), BsonIgnoreIfNull]
	public int? NumWallDodges { get; set; }

	[BsonElement("numJumps"), BsonIgnoreIfNull]
	public int? NumJumps { get; set; }

	[BsonElement("numLiftJumps"), BsonIgnoreIfNull]
	public int? NumLiftJumps { get; set; }

	[BsonElement("numFloorSlides"), BsonIgnoreIfNull]
	public int? NumFloorSlides { get; set; }

	[BsonElement("numWallRuns"), BsonIgnoreIfNull]
	public int? NumWallRuns { get; set; }

	[BsonElement("numImpactJumps"), BsonIgnoreIfNull]
	public int? NumImpactJumps { get; set; }

	[BsonElement("numRocketJumps"), BsonIgnoreIfNull]
	public int? NumRocketJumps { get; set; }

	[BsonElement("slideDist"), BsonIgnoreIfNull]
	public double? SlideDist { get; set; }

	[BsonElement("wallRunDist"), BsonIgnoreIfNull]
	public double? WallRunDist { get; set; }
	#endregion

	#region Miscellaneous - Capture the Flag
	[BsonElement("flagCaptures"), BsonIgnoreIfNull]
	public int? FlagCaptures { get; set; }

	[BsonElement("flagReturns"), BsonIgnoreIfNull]
	public int? FlagReturns { get; set; }

	[BsonElement("flagAssists"), BsonIgnoreIfNull]
	public int? FlagAssists { get; set; }

	[BsonElement("flagHeldDeny"), BsonIgnoreIfNull]
	public int? FlagHeldDeny { get; set; }

	[BsonElement("flagHeldDenyTime"), BsonIgnoreIfNull]
	public double? FlagHeldDenyTime { get; set; }

	[BsonElement("flagHeldTime"), BsonIgnoreIfNull]
	public double? FlagHeldTime { get; set; }

	[BsonElement("flagReturnPoints"), BsonIgnoreIfNull]
	public int? FlagReturnPoints { get; set; }

	[BsonElement("carryAssist"), BsonIgnoreIfNull]
	public int? CarryAssist { get; set; }

	[BsonElement("carryAssistPoints"), BsonIgnoreIfNull]
	public int? CarryAssistPoints { get; set; }

	[BsonElement("flagCapPoints"), BsonIgnoreIfNull]
	public int? FlagCapPoints { get; set; }

	[BsonElement("defendAssist"), BsonIgnoreIfNull]
	public int? DefendAssist { get; set; }

	[BsonElement("defendAssistPoints"), BsonIgnoreIfNull]
	public int? DefendAssistPoints { get; set; }

	[BsonElement("returnAssist"), BsonIgnoreIfNull]
	public int? ReturnAssist { get; set; }

	[BsonElement("returnAssistPoints"), BsonIgnoreIfNull]
	public int? ReturnAssistPoints { get; set; }

	[BsonElement("teamCapPoints"), BsonIgnoreIfNull]
	public int? TeamCapPoints { get; set; }

	[BsonElement("enemyFCDamage"), BsonIgnoreIfNull]
	public int? EnemyFCDamage { get; set; }

	[BsonElement("FCKills"), BsonIgnoreIfNull]
	public int? FCKills { get; set; }

	[BsonElement("FCKillPoints"), BsonIgnoreIfNull]
	public int? FCKillPoints { get; set; }

	[BsonElement("flagSupportKills"), BsonIgnoreIfNull]
	public int? FlagSupportKills { get; set; }

	[BsonElement("flagSupportKillPoints"), BsonIgnoreIfNull]
	public int? FlagSupportKillPoints { get; set; }

	[BsonElement("regularKillPoints"), BsonIgnoreIfNull]
	public int? RegularKillPoints { get; set; }

	[BsonElement("flagGrabs"), BsonIgnoreIfNull]
	public int? FlagGrabs { get; set; }

	[BsonElement("attackerScore"), BsonIgnoreIfNull]
	public int? AttackerScore { get; set; }

	[BsonElement("defenderScore"), BsonIgnoreIfNull]
	public int? DefenderScore { get; set; }

	[BsonElement("supporterScore"), BsonIgnoreIfNull]
	public int? SupporterScore { get; set; }

	[BsonElement("teamKills"), BsonIgnoreIfNull]
	public int? TeamKills { get; set; }
	#endregion
}
