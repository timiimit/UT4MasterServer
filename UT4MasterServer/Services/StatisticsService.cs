using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UT4MasterServer.DTOs;
using UT4MasterServer.Enums;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public sealed class StatisticsService
{
	private readonly ILogger<StatisticsService> logger;
	private readonly IMongoCollection<Statistic> statisticsCollection;

	public StatisticsService(
		ILogger<StatisticsService> logger,
		DatabaseContext dbContext)
	{
		this.logger = logger;
		statisticsCollection = dbContext.Database.GetCollection<Statistic>("statistics");
	}

	public async Task<List<StatisticDto>> GetAggregateAccountStatistics(EpicID accountId, StatisticWindow statisticWindow)
	{
		logger.LogInformation("Calculating {StatisticWindow} statistics for account: {AccountId}.", statisticWindow.ToString().ToLower(), accountId);

		var dateFrom = DateTime.UtcNow.Date;
		if (statisticWindow != StatisticWindow.Daily)
		{
			dateFrom = DateTime.UtcNow.AddDays(-(int)statisticWindow).Date;
		}
		var dateTo = DateTime.UtcNow.AddDays(1).Date;
		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountId, accountId) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.Daily) &
					 Builders<Statistic>.Filter.Gte(f => f.CreatedAt, dateFrom) &
					 Builders<Statistic>.Filter.Lt(f => f.CreatedAt, dateTo);

		var statistics = await statisticsCollection
			.Aggregate()
			.Match(filter)
			.Group(k => k.Type,
				   g => new { Type = g.Key, Value = g.Sum(s => s.Value) })
			.ToListAsync();

		var result = new List<StatisticDto>();

		foreach (var type in Enum.GetValues<StatisticType>())
		{
			if (type == StatisticType.Unknown) continue;

			var existingStatistic = statistics.FirstOrDefault(f => f.Type == type);
			result.Add(new StatisticDto()
			{
				Name = type.ToString(),
				Value = GetValueForStatisticType(type, existingStatistic?.Value ?? 0),
				Window = statisticWindow.ToString().ToLower(),
				OwnerType = OwnerType.Default
			});
		}

		return result;
	}

	public async Task<List<StatisticDto>> GetAllTimeAccountStatistics(EpicID accountId)
	{
		logger.LogInformation("Calculating all-time statistics for account: {AccountId}.", accountId);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountId, accountId) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime);

		var statistics = await statisticsCollection.Find(filter).ToListAsync();

		var result = new List<StatisticDto>();

		foreach (var type in Enum.GetValues<StatisticType>())
		{
			if (type == StatisticType.Unknown) continue;

			var existingStatistic = statistics.FirstOrDefault(f => f.Type == type);
			result.Add(new StatisticDto()
			{
				Name = type.ToString(),
				Value = GetValueForStatisticType(type, existingStatistic?.Value ?? 0),
				Window = "alltime",
				OwnerType = OwnerType.Default
			});
		}

		return result;
	}

	public async Task CreateAccountStatistics(EpicID accountId, OwnerType ownerType, StatisticBulkDto statisticBulkDto)
	{
		logger.LogInformation("Creating statistics for account: {AccountId}.", accountId);

		var newStatistics = GenerateStatistics(accountId, ownerType, statisticBulkDto);

		await statisticsCollection.InsertManyAsync(newStatistics);

		await UpdateAllTimeAccountStatistics(accountId, newStatistics);
	}

	#region Private methods

	private static List<Statistic> GenerateStatistics(EpicID epicAccountId, OwnerType ownerType, StatisticBulkDto statisticBulkDto)
	{
		var currentDateTime = DateTime.UtcNow;
		var newStatistics = new List<Statistic>();

		#region Quick Look

		if (statisticBulkDto.SkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SkillRating,
				Value = statisticBulkDto.SkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TDMSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TDMSkillRating,
				Value = statisticBulkDto.TDMSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.CTFSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.CTFSkillRating,
				Value = statisticBulkDto.CTFSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.DMSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.DMSkillRating,
				Value = statisticBulkDto.DMSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShowdownSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShowdownSkillRating,
				Value = statisticBulkDto.ShowdownSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagRunSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagRunSkillRating,
				Value = statisticBulkDto.FlagRunSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RankedDuelSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RankedDuelSkillRating,
				Value = statisticBulkDto.RankedDuelSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RankedCTFSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RankedCTFSkillRating,
				Value = statisticBulkDto.RankedCTFSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RankedShowdownSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RankedShowdownSkillRating,
				Value = statisticBulkDto.RankedShowdownSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RankedFlagRunSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RankedFlagRunSkillRating,
				Value = statisticBulkDto.RankedFlagRunSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MatchesPlayed.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MatchesPlayed,
				Value = statisticBulkDto.MatchesPlayed.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MatchesQuit.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MatchesQuit,
				Value = statisticBulkDto.MatchesQuit.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TimePlayed.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TimePlayed,
				Value = statisticBulkDto.TimePlayed.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.Wins.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.Wins,
				Value = statisticBulkDto.Wins.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.Losses.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.Losses,
				Value = statisticBulkDto.Losses.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.Kills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.Kills,
				Value = statisticBulkDto.Kills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.Deaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.Deaths,
				Value = statisticBulkDto.Deaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.Suicides.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.Suicides,
				Value = statisticBulkDto.Suicides.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Kill Achievements

		if (statisticBulkDto.MultiKillLevel0.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MultiKillLevel0,
				Value = statisticBulkDto.MultiKillLevel0.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MultiKillLevel1.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MultiKillLevel1,
				Value = statisticBulkDto.MultiKillLevel1.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MultiKillLevel2.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MultiKillLevel2,
				Value = statisticBulkDto.MultiKillLevel2.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MultiKillLevel3.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MultiKillLevel3,
				Value = statisticBulkDto.MultiKillLevel3.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SpreeKillLevel0.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SpreeKillLevel0,
				Value = statisticBulkDto.SpreeKillLevel0.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SpreeKillLevel1.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SpreeKillLevel1,
				Value = statisticBulkDto.SpreeKillLevel1.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SpreeKillLevel2.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SpreeKillLevel2,
				Value = statisticBulkDto.SpreeKillLevel2.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SpreeKillLevel3.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SpreeKillLevel3,
				Value = statisticBulkDto.SpreeKillLevel3.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SpreeKillLevel4.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SpreeKillLevel4,
				Value = statisticBulkDto.SpreeKillLevel4.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BestShockCombo.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BestShockCombo,
				Value = statisticBulkDto.BestShockCombo.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.AmazingCombos.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.AmazingCombos,
				Value = statisticBulkDto.AmazingCombos.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.AirRox.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.AirRox,
				Value = statisticBulkDto.AirRox.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShreds.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShreds,
				Value = statisticBulkDto.FlakShreds.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.AirSnot.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.AirSnot,
				Value = statisticBulkDto.AirSnot.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Power Up Achievements

		if (statisticBulkDto.UDamageTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.UDamageTime,
				Value = statisticBulkDto.UDamageTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BerserkTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BerserkTime,
				Value = statisticBulkDto.BerserkTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InvisibilityTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InvisibilityTime,
				Value = statisticBulkDto.InvisibilityTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.UDamageCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.UDamageCount,
				Value = statisticBulkDto.UDamageCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BerserkCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BerserkCount,
				Value = statisticBulkDto.BerserkCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InvisibilityCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InvisibilityCount,
				Value = statisticBulkDto.InvisibilityCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BootJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BootJumps,
				Value = statisticBulkDto.BootJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShieldBeltCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShieldBeltCount,
				Value = statisticBulkDto.ShieldBeltCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ArmorVestCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ArmorVestCount,
				Value = statisticBulkDto.ArmorVestCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ArmorPadsCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ArmorPadsCount,
				Value = statisticBulkDto.ArmorPadsCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.HelmetCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.HelmetCount,
				Value = statisticBulkDto.HelmetCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.KegCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.KegCount,
				Value = statisticBulkDto.KegCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Weapon Stats

		if (statisticBulkDto.ImpactHammerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ImpactHammerKills,
				Value = statisticBulkDto.ImpactHammerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ImpactHammerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ImpactHammerDeaths,
				Value = statisticBulkDto.ImpactHammerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.EnforcerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.EnforcerKills,
				Value = statisticBulkDto.EnforcerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.EnforcerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.EnforcerDeaths,
				Value = statisticBulkDto.EnforcerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.EnforcerShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.EnforcerShots,
				Value = statisticBulkDto.EnforcerShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.EnforcerHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.EnforcerHits,
				Value = statisticBulkDto.EnforcerHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioRifleKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioRifleKills,
				Value = statisticBulkDto.BioRifleKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioRifleDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioRifleDeaths,
				Value = statisticBulkDto.BioRifleDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioRifleShots,
				Value = statisticBulkDto.BioRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioRifleHits,
				Value = statisticBulkDto.BioRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioLauncherKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioLauncherKills,
				Value = statisticBulkDto.BioLauncherKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioLauncherDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioLauncherDeaths,
				Value = statisticBulkDto.BioLauncherDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioLauncherShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioLauncherShots,
				Value = statisticBulkDto.BioLauncherShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.BioLauncherHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.BioLauncherHits,
				Value = statisticBulkDto.BioLauncherHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockBeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockBeamKills,
				Value = statisticBulkDto.ShockBeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockBeamDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockBeamDeaths,
				Value = statisticBulkDto.ShockBeamDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockCoreKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockCoreKills,
				Value = statisticBulkDto.ShockCoreKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockCoreDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockCoreDeaths,
				Value = statisticBulkDto.ShockCoreDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockComboKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockComboKills,
				Value = statisticBulkDto.ShockComboKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockComboDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockComboDeaths,
				Value = statisticBulkDto.ShockComboDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockRifleShots,
				Value = statisticBulkDto.ShockRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ShockRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ShockRifleHits,
				Value = statisticBulkDto.ShockRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkKills,
				Value = statisticBulkDto.LinkKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkDeaths,
				Value = statisticBulkDto.LinkDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkBeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkBeamKills,
				Value = statisticBulkDto.LinkBeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkBeamDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkBeamDeaths,
				Value = statisticBulkDto.LinkBeamDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkShots,
				Value = statisticBulkDto.LinkShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LinkHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LinkHits,
				Value = statisticBulkDto.LinkHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunKills,
				Value = statisticBulkDto.MinigunKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunDeaths,
				Value = statisticBulkDto.MinigunDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunShardKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunShardKills,
				Value = statisticBulkDto.MinigunShardKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunShardDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunShardDeaths,
				Value = statisticBulkDto.MinigunShardDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunShots,
				Value = statisticBulkDto.MinigunShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.MinigunHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.MinigunHits,
				Value = statisticBulkDto.MinigunHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShardKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShardKills,
				Value = statisticBulkDto.FlakShardKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShardDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShardDeaths,
				Value = statisticBulkDto.FlakShardDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShellKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShellKills,
				Value = statisticBulkDto.FlakShellKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShellDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShellDeaths,
				Value = statisticBulkDto.FlakShellDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakShots,
				Value = statisticBulkDto.FlakShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlakHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlakHits,
				Value = statisticBulkDto.FlakHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RocketKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RocketKills,
				Value = statisticBulkDto.RocketKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RocketDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RocketDeaths,
				Value = statisticBulkDto.RocketDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RocketShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RocketShots,
				Value = statisticBulkDto.RocketShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RocketHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RocketHits,
				Value = statisticBulkDto.RocketHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperKills,
				Value = statisticBulkDto.SniperKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperDeaths,
				Value = statisticBulkDto.SniperDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperHeadshotKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperHeadshotKills,
				Value = statisticBulkDto.SniperHeadshotKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperHeadshotDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperHeadshotDeaths,
				Value = statisticBulkDto.SniperHeadshotDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperShots,
				Value = statisticBulkDto.SniperShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SniperHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SniperHits,
				Value = statisticBulkDto.SniperHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRiflePrimaryKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRiflePrimaryKills,
				Value = statisticBulkDto.LightningRiflePrimaryKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRiflePrimaryDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRiflePrimaryDeaths,
				Value = statisticBulkDto.LightningRiflePrimaryDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRifleSecondaryKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRifleSecondaryKills,
				Value = statisticBulkDto.LightningRifleSecondaryKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRifleSecondaryDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRifleSecondaryDeaths,
				Value = statisticBulkDto.LightningRifleSecondaryDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRifleShots,
				Value = statisticBulkDto.LightningRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.LightningRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.LightningRifleHits,
				Value = statisticBulkDto.LightningRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RedeemerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RedeemerKills,
				Value = statisticBulkDto.RedeemerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RedeemerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RedeemerDeaths,
				Value = statisticBulkDto.RedeemerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RedeemerShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RedeemerShots,
				Value = statisticBulkDto.RedeemerShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RedeemerHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RedeemerHits,
				Value = statisticBulkDto.RedeemerHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InstagibKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InstagibKills,
				Value = statisticBulkDto.InstagibKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InstagibDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InstagibDeaths,
				Value = statisticBulkDto.InstagibDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InstagibShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InstagibShots,
				Value = statisticBulkDto.InstagibShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InstagibHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InstagibHits,
				Value = statisticBulkDto.InstagibHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TelefragKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TelefragKills,
				Value = statisticBulkDto.TelefragKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TelefragDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TelefragDeaths,
				Value = statisticBulkDto.TelefragDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Miscellaneous - Movement

		if (statisticBulkDto.RunDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RunDist,
				Value = statisticBulkDto.RunDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SprintDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SprintDist,
				Value = statisticBulkDto.SprintDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.InAirDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.InAirDist,
				Value = statisticBulkDto.InAirDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SwimDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SwimDist,
				Value = statisticBulkDto.SwimDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TranslocDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TranslocDist,
				Value = statisticBulkDto.TranslocDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumDodges.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumDodges,
				Value = statisticBulkDto.NumDodges.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumWallDodges.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumWallDodges,
				Value = statisticBulkDto.NumWallDodges.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumJumps,
				Value = statisticBulkDto.NumJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumLiftJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumLiftJumps,
				Value = statisticBulkDto.NumLiftJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumFloorSlides.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumFloorSlides,
				Value = statisticBulkDto.NumFloorSlides.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumWallRuns.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumWallRuns,
				Value = statisticBulkDto.NumWallRuns.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumImpactJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumImpactJumps,
				Value = statisticBulkDto.NumImpactJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.NumRocketJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.NumRocketJumps,
				Value = statisticBulkDto.NumRocketJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SlideDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SlideDist,
				Value = statisticBulkDto.SlideDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.WallRunDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.WallRunDist,
				Value = statisticBulkDto.WallRunDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Miscellaneous - Capture the Flag

		if (statisticBulkDto.FlagCaptures.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagCaptures,
				Value = statisticBulkDto.FlagCaptures.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagReturns.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagReturns,
				Value = statisticBulkDto.FlagReturns.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagAssists.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagAssists,
				Value = statisticBulkDto.FlagAssists.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagHeldDeny.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagHeldDeny,
				Value = statisticBulkDto.FlagHeldDeny.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagHeldDenyTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagHeldDenyTime,
				Value = statisticBulkDto.FlagHeldDenyTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagHeldTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagHeldTime,
				Value = statisticBulkDto.FlagHeldTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagReturnPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagReturnPoints,
				Value = statisticBulkDto.FlagReturnPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.CarryAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.CarryAssist,
				Value = statisticBulkDto.CarryAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.CarryAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.CarryAssistPoints,
				Value = statisticBulkDto.CarryAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagCapPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagCapPoints,
				Value = statisticBulkDto.FlagCapPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.DefendAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.DefendAssist,
				Value = statisticBulkDto.DefendAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.DefendAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.DefendAssistPoints,
				Value = statisticBulkDto.DefendAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ReturnAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ReturnAssist,
				Value = statisticBulkDto.ReturnAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.ReturnAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.ReturnAssistPoints,
				Value = statisticBulkDto.ReturnAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TeamCapPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TeamCapPoints,
				Value = statisticBulkDto.TeamCapPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.EnemyFCDamage.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.EnemyFCDamage,
				Value = statisticBulkDto.EnemyFCDamage.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FCKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FCKills,
				Value = statisticBulkDto.FCKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FCKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FCKillPoints,
				Value = statisticBulkDto.FCKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagSupportKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagSupportKills,
				Value = statisticBulkDto.FlagSupportKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagSupportKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagSupportKillPoints,
				Value = statisticBulkDto.FlagSupportKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.RegularKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.RegularKillPoints,
				Value = statisticBulkDto.RegularKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.FlagGrabs.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.FlagGrabs,
				Value = statisticBulkDto.FlagGrabs.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.AttackerScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.AttackerScore,
				Value = statisticBulkDto.AttackerScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.DefenderScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.DefenderScore,
				Value = statisticBulkDto.DefenderScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.SupporterScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.SupporterScore,
				Value = statisticBulkDto.SupporterScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDto.TeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountId = epicAccountId,
				Type = StatisticType.TeamKills,
				Value = statisticBulkDto.TeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		return newStatistics;
	}

	private async Task UpdateAllTimeAccountStatistics(EpicID accountId, List<Statistic> newStatistics)
	{
		logger.LogInformation("Updating all-time statistics for account: {AccountId}.", accountId);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountId, accountId) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime) &
					 Builders<Statistic>.Filter.In(f => f.Type, newStatistics.Select(s => s.Type).ToList());

		var existingStatistics = await statisticsCollection.Find(filter).ToListAsync();

		foreach (var newStatistic in newStatistics)
		{
			if (existingStatistics.FirstOrDefault(f => f.Type == newStatistic.Type) is { } existingStatistic)
			{
				await statisticsCollection.UpdateOneAsync(
					f => f.Id == existingStatistic.Id,
					Builders<Statistic>.Update.Inc(i => i.Value, newStatistic.Value)
											  .Set(s => s.ModifiedAt, DateTime.UtcNow));
			}
			else
			{
				await statisticsCollection.InsertOneAsync(new Statistic()
				{
					AccountId = accountId,
					CreatedAt = DateTime.UtcNow,
					Type = newStatistic.Type,
					Value = newStatistic.Value,
					Window = StatisticWindow.AllTime
				});
			}
		}
	}

	private static int GetValueForStatisticType(StatisticType type, float value)
	{
		switch (type)
		{
			case StatisticType.EnforcerHits:
			case StatisticType.BioRifleHits:
			case StatisticType.BioLauncherHits:
			case StatisticType.ShockRifleHits:
			case StatisticType.LinkHits:
			case StatisticType.MinigunHits:
			case StatisticType.FlakHits:
			case StatisticType.RocketHits:
			case StatisticType.SniperHits:
			case StatisticType.LightningRifleHits:
			case StatisticType.RedeemerHits:
			case StatisticType.InstagibHits:
			case StatisticType.RunDist:
			case StatisticType.SprintDist:
			case StatisticType.InAirDist:
			case StatisticType.SwimDist:
			case StatisticType.TranslocDist:
			case StatisticType.SlideDist:
			case StatisticType.WallRunDist:
				return (int)(value * 100L);

			default:
				return (int)value;
		}
	}

	#endregion
}
