import { Statistic } from '@/enums/statistic';
import { IStatisticSection } from '@/types/statistic-config';

export const statSections: IStatisticSection[] = [
  {
    heading: 'Overall Stats',
    cards: [
      {
        heading: 'Quick Look',
        stats: [
          Statistic.SkillRating,
          Statistic.TDMSkillRating,
          Statistic.CTFSkillRating,
          Statistic.DMSkillRating,
          Statistic.ShowdownSkillRating,
          Statistic.MatchesPlayed,
          Statistic.MatchesQuit,
          Statistic.TimePlayed,
          Statistic.Wins,
          Statistic.Losses,
          Statistic.Kills,
          Statistic.Deaths,
          Statistic.Suicides
        ]
      },
      {
        heading: 'Kill Achievements',
        stats: [
          Statistic.MultiKillLevel0,
          Statistic.MultiKillLevel1,
          Statistic.MultiKillLevel2,
          Statistic.MultiKillLevel3,
          Statistic.SpreeKillLevel0,
          Statistic.SpreeKillLevel1,
          Statistic.SpreeKillLevel2,
          Statistic.SpreeKillLevel3,
          Statistic.SpreeKillLevel4,
          Statistic.BestShockCombo,
          Statistic.AmazingCombos,
          Statistic.AirRox,
          Statistic.FlakShreds,
          Statistic.AirSnot
        ]
      },
      {
        heading: 'Power Up Achievements',
        stats: [
          Statistic.UDamageTime,
          Statistic.BerserkTime,
          Statistic.InvisibilityTime,
          Statistic.UDamageCount,
          Statistic.BerserkCount,
          Statistic.InvisibilityCount,
          Statistic.BootJumps,
          Statistic.ShieldBeltCount,
          Statistic.ArmorVestCount,
          Statistic.ArmorPadsCount,
          Statistic.HelmetCount,
          Statistic.KegCount
        ]
      }
    ]
  },
  {
    heading: 'Weapon Stats',
    cards: [
      {
        heading: 'Impact Hammer',
        headingIcon: 'ih.png',
        stats: [Statistic.ImpactHammerKills, Statistic.ImpactHammerDeaths]
      },
      {
        heading: 'Enforcer',
        headingIcon: 'en.png',
        stats: [
          Statistic.EnforcerKills,
          Statistic.EnforcerDeaths,
          Statistic.EnforcerShots,
          Statistic.EnforcerHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Bio Rifle',
        headingIcon: 'br.png',
        stats: [
          Statistic.BioRifleKills,
          Statistic.BioRifleDeaths,
          Statistic.BioRifleShots,
          Statistic.BioRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Grenade Launcher',
        headingIcon: 'br.png',
        stats: [
          Statistic.BioLauncherKills,
          Statistic.BioLauncherDeaths,
          Statistic.BioLauncherShots,
          Statistic.BioLauncherHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Shock Rifle',
        headingIcon: 'sr.png',
        stats: [
          Statistic.ShockBeamKills,
          Statistic.ShockBeamDeaths,
          Statistic.ShockCoreKills,
          Statistic.ShockCoreDeaths,
          Statistic.ShockComboKills,
          Statistic.ShockComboDeaths,
          Statistic.ShockRifleShots,
          Statistic.ShockRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Link Gun',
        headingIcon: 'lr.png',
        stats: [
          Statistic.LinkKills,
          Statistic.LinkDeaths,
          Statistic.LinkBeamKills,
          Statistic.LinkBeamDeaths,
          Statistic.LinkShots,
          Statistic.LinkHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Minigun',
        headingIcon: 'mg.png',
        stats: [
          Statistic.MinigunKills,
          Statistic.MinigunDeaths,
          Statistic.MinigunShardKills,
          Statistic.MinigunShardDeaths,
          Statistic.MinigunShots,
          Statistic.MinigunHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Flak Cannon',
        headingIcon: 'fc.png',
        stats: [
          Statistic.FlakShardKills,
          Statistic.FlakShardDeaths,
          Statistic.FlakShellKills,
          Statistic.FlakShellDeaths,
          Statistic.FlakShots,
          Statistic.FlakHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Rocket Launcher',
        headingIcon: 'rl.png',
        stats: [
          Statistic.RocketKills,
          Statistic.RocketDeaths,
          Statistic.RocketShots,
          Statistic.RocketHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Sniper',
        headingIcon: 'snr.png',
        stats: [
          Statistic.SniperKills,
          Statistic.SniperDeaths,
          Statistic.SniperHeadshotKills,
          Statistic.SniperHeadshotDeaths,
          Statistic.SniperShots,
          Statistic.SniperHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Lightning Rifle',
        headingIcon: 'lightning.png',
        stats: [
          Statistic.LightningRiflePrimaryKills,
          Statistic.LightningRiflePrimaryDeaths,
          Statistic.LightningRifleSecondaryKills,
          Statistic.LightningRifleSecondaryDeaths,
          Statistic.LightningRifleShots,
          Statistic.LightningRifleHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Redeemer',
        headingIcon: 'rd.png',
        stats: [
          Statistic.RedeemerKills,
          Statistic.RedeemerDeaths,
          Statistic.RedeemerShots,
          Statistic.RedeemerHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Instagib',
        headingIcon: 'sr.png',
        stats: [
          Statistic.InstagibKills,
          Statistic.InstagibDeaths,
          Statistic.InstagibShots,
          Statistic.InstagibHits,
          Statistic.Accuracy
        ]
      },
      {
        heading: 'Translocator',
        headingIcon: 'xloc.png',
        stats: [Statistic.TelefragKills, Statistic.TelefragDeaths]
      }
    ]
  },
  {
    heading: 'Miscellaneous',
    cards: [
      {
        heading: 'Movement',
        stats: [
          Statistic.RunDist,
          Statistic.SprintDist,
          Statistic.InAirDist,
          Statistic.SwimDist,
          Statistic.TranslocDist,
          Statistic.NumDodges,
          Statistic.NumWallDodges,
          Statistic.NumJumps,
          Statistic.NumLiftJumps,
          Statistic.NumFloorSlides,
          Statistic.NumWallRuns,
          Statistic.NumImpactJumps,
          Statistic.NumRocketJumps,
          Statistic.SlideDist,
          Statistic.WallRunDist
        ]
      },
      {
        heading: 'Capture the Flag',
        stats: [
          Statistic.FlagCaptures,
          Statistic.FlagReturns,
          Statistic.FlagAssists,
          Statistic.FlagHeldDeny,
          Statistic.FlagHeldDenyTime,
          Statistic.FlagHeldTime,
          Statistic.FlagReturnPoints,
          Statistic.CarryAssist,
          Statistic.CarryAssistPoints,
          Statistic.FlagCapPoints,
          Statistic.DefendAssist,
          Statistic.DefendAssistPoints,
          Statistic.ReturnAssist,
          Statistic.ReturnAssistPoints,
          Statistic.TeamCapPoints,
          Statistic.EnemyFCDamage,
          Statistic.FCKills,
          Statistic.FCKillPoints,
          Statistic.FlagSupportKills,
          Statistic.FlagSupportKillPoints,
          Statistic.RegularKillPoints,
          Statistic.FlagGrabs,
          Statistic.AttackerScore,
          Statistic.DefenderScore,
          Statistic.SupporterScore
        ]
      }
    ]
  }
];
