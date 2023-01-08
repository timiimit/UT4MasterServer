using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
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

	public async Task CreateIndexes()
	{
		var statisticsIndexes = new List<CreateIndexModel<Statistic>>()
			{
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.AccountID)),
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.CreatedAt))
			};
		await statisticsCollection.Indexes.CreateManyAsync(statisticsIndexes);
	}

	public async Task<List<StatisticDTO>> GetAggregateAccountStatistics(EpicID accountID, StatisticWindow statisticWindow)
	{
		logger.LogInformation("Calculating {StatisticWindow} statistics for account: {AccountId}.", statisticWindow.ToString().ToLower(), accountID);

		var dateFrom = DateTime.UtcNow.Date;
		if (statisticWindow != StatisticWindow.Daily)
		{
			dateFrom = DateTime.UtcNow.AddDays(-(int)statisticWindow).Date;
		}
		var dateTo = DateTime.UtcNow.AddDays(1).Date;
		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.Daily) &
					 Builders<Statistic>.Filter.Gte(f => f.CreatedAt, dateFrom) &
					 Builders<Statistic>.Filter.Lt(f => f.CreatedAt, dateTo);

		var statistics = await statisticsCollection
			.Aggregate()
			.Match(filter)
			.Group(k => k.Type,
				   g => new { Type = g.Key, Value = g.Sum(s => s.Value) })
			.ToListAsync();

		var result = new List<StatisticDTO>();

		foreach (var type in Enum.GetValues<StatisticType>())
		{
			if (type == StatisticType.Unknown) continue;

			var existingStatistic = statistics.FirstOrDefault(f => f.Type == type);
			result.Add(new StatisticDTO()
			{
				Name = type.ToString(),
				Value = GetValueForStatisticType(type, existingStatistic?.Value ?? 0),
				Window = statisticWindow.ToString().ToLower(),
				OwnerType = OwnerType.Default
			});
		}

		return result;
	}

	public async Task<List<StatisticDTO>> GetAllTimeAccountStatistics(EpicID accountID)
	{
		logger.LogInformation("Calculating all-time statistics for account: {AccountId}.", accountID);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime);

		var statistics = await statisticsCollection.Find(filter).ToListAsync();

		var result = new List<StatisticDTO>();

		foreach (var type in Enum.GetValues<StatisticType>())
		{
			if (type == StatisticType.Unknown) continue;

			var existingStatistic = statistics.FirstOrDefault(f => f.Type == type);
			result.Add(new StatisticDTO()
			{
				Name = type.ToString(),
				Value = GetValueForStatisticType(type, existingStatistic?.Value ?? 0),
				Window = "alltime",
				OwnerType = OwnerType.Default
			});
		}

		return result;
	}

	public async Task CreateAccountStatistics(EpicID accountID, OwnerType ownerType, StatisticBulkDTO statisticBulkDTO)
	{
		logger.LogInformation("Creating statistics for account: {AccountId}.", accountID);

		var newStatistics = GenerateStatistics(accountID, ownerType, statisticBulkDTO);

		await statisticsCollection.InsertManyAsync(newStatistics);
		await UpdateAllTimeAccountStatistics(accountID, newStatistics);
	}

	#region Private methods

	private static List<Statistic> GenerateStatistics(EpicID accountID, OwnerType ownerType, StatisticBulkDTO statisticBulkDTO)
	{
		var currentDateTime = DateTime.UtcNow;
		var newStatistics = new List<Statistic>();

		#region Quick Look

		if (statisticBulkDTO.SkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SkillRating,
				Value = statisticBulkDTO.SkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TDMSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TDMSkillRating,
				Value = statisticBulkDTO.TDMSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.CTFSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.CTFSkillRating,
				Value = statisticBulkDTO.CTFSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.DMSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.DMSkillRating,
				Value = statisticBulkDTO.DMSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShowdownSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShowdownSkillRating,
				Value = statisticBulkDTO.ShowdownSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagRunSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagRunSkillRating,
				Value = statisticBulkDTO.FlagRunSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RankedDuelSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RankedDuelSkillRating,
				Value = statisticBulkDTO.RankedDuelSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RankedCTFSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RankedCTFSkillRating,
				Value = statisticBulkDTO.RankedCTFSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RankedShowdownSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RankedShowdownSkillRating,
				Value = statisticBulkDTO.RankedShowdownSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RankedFlagRunSkillRating.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RankedFlagRunSkillRating,
				Value = statisticBulkDTO.RankedFlagRunSkillRating.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MatchesPlayed.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MatchesPlayed,
				Value = statisticBulkDTO.MatchesPlayed.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MatchesQuit.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MatchesQuit,
				Value = statisticBulkDTO.MatchesQuit.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TimePlayed.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TimePlayed,
				Value = statisticBulkDTO.TimePlayed.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.Wins.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.Wins,
				Value = statisticBulkDTO.Wins.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.Losses.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.Losses,
				Value = statisticBulkDTO.Losses.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.Kills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.Kills,
				Value = statisticBulkDTO.Kills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.Deaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.Deaths,
				Value = statisticBulkDTO.Deaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.Suicides.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.Suicides,
				Value = statisticBulkDTO.Suicides.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Kill Achievements

		if (statisticBulkDTO.MultiKillLevel0.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MultiKillLevel0,
				Value = statisticBulkDTO.MultiKillLevel0.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MultiKillLevel1.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MultiKillLevel1,
				Value = statisticBulkDTO.MultiKillLevel1.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MultiKillLevel2.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MultiKillLevel2,
				Value = statisticBulkDTO.MultiKillLevel2.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MultiKillLevel3.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MultiKillLevel3,
				Value = statisticBulkDTO.MultiKillLevel3.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SpreeKillLevel0.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SpreeKillLevel0,
				Value = statisticBulkDTO.SpreeKillLevel0.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SpreeKillLevel1.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SpreeKillLevel1,
				Value = statisticBulkDTO.SpreeKillLevel1.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SpreeKillLevel2.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SpreeKillLevel2,
				Value = statisticBulkDTO.SpreeKillLevel2.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SpreeKillLevel3.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SpreeKillLevel3,
				Value = statisticBulkDTO.SpreeKillLevel3.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SpreeKillLevel4.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SpreeKillLevel4,
				Value = statisticBulkDTO.SpreeKillLevel4.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BestShockCombo.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BestShockCombo,
				Value = statisticBulkDTO.BestShockCombo.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.AmazingCombos.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.AmazingCombos,
				Value = statisticBulkDTO.AmazingCombos.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.AirRox.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.AirRox,
				Value = statisticBulkDTO.AirRox.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShreds.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShreds,
				Value = statisticBulkDTO.FlakShreds.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.AirSnot.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.AirSnot,
				Value = statisticBulkDTO.AirSnot.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Power Up Achievements

		if (statisticBulkDTO.UDamageTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.UDamageTime,
				Value = statisticBulkDTO.UDamageTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BerserkTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BerserkTime,
				Value = statisticBulkDTO.BerserkTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InvisibilityTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InvisibilityTime,
				Value = statisticBulkDTO.InvisibilityTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.UDamageCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.UDamageCount,
				Value = statisticBulkDTO.UDamageCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BerserkCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BerserkCount,
				Value = statisticBulkDTO.BerserkCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InvisibilityCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InvisibilityCount,
				Value = statisticBulkDTO.InvisibilityCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BootJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BootJumps,
				Value = statisticBulkDTO.BootJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShieldBeltCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShieldBeltCount,
				Value = statisticBulkDTO.ShieldBeltCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ArmorVestCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ArmorVestCount,
				Value = statisticBulkDTO.ArmorVestCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ArmorPadsCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ArmorPadsCount,
				Value = statisticBulkDTO.ArmorPadsCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.HelmetCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.HelmetCount,
				Value = statisticBulkDTO.HelmetCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.KegCount.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.KegCount,
				Value = statisticBulkDTO.KegCount.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Weapon Stats

		if (statisticBulkDTO.ImpactHammerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ImpactHammerKills,
				Value = statisticBulkDTO.ImpactHammerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ImpactHammerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ImpactHammerDeaths,
				Value = statisticBulkDTO.ImpactHammerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.EnforcerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.EnforcerKills,
				Value = statisticBulkDTO.EnforcerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.EnforcerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.EnforcerDeaths,
				Value = statisticBulkDTO.EnforcerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.EnforcerShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.EnforcerShots,
				Value = statisticBulkDTO.EnforcerShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.EnforcerHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.EnforcerHits,
				Value = statisticBulkDTO.EnforcerHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioRifleKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioRifleKills,
				Value = statisticBulkDTO.BioRifleKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioRifleDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioRifleDeaths,
				Value = statisticBulkDTO.BioRifleDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioRifleShots,
				Value = statisticBulkDTO.BioRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioRifleHits,
				Value = statisticBulkDTO.BioRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioLauncherKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioLauncherKills,
				Value = statisticBulkDTO.BioLauncherKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioLauncherDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioLauncherDeaths,
				Value = statisticBulkDTO.BioLauncherDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioLauncherShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioLauncherShots,
				Value = statisticBulkDTO.BioLauncherShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.BioLauncherHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.BioLauncherHits,
				Value = statisticBulkDTO.BioLauncherHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockBeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockBeamKills,
				Value = statisticBulkDTO.ShockBeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockBeamDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockBeamDeaths,
				Value = statisticBulkDTO.ShockBeamDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockCoreKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockCoreKills,
				Value = statisticBulkDTO.ShockCoreKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockCoreDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockCoreDeaths,
				Value = statisticBulkDTO.ShockCoreDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockComboKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockComboKills,
				Value = statisticBulkDTO.ShockComboKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockComboDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockComboDeaths,
				Value = statisticBulkDTO.ShockComboDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockRifleShots,
				Value = statisticBulkDTO.ShockRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ShockRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ShockRifleHits,
				Value = statisticBulkDTO.ShockRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkKills,
				Value = statisticBulkDTO.LinkKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkDeaths,
				Value = statisticBulkDTO.LinkDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkBeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkBeamKills,
				Value = statisticBulkDTO.LinkBeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkBeamDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkBeamDeaths,
				Value = statisticBulkDTO.LinkBeamDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkShots,
				Value = statisticBulkDTO.LinkShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LinkHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LinkHits,
				Value = statisticBulkDTO.LinkHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunKills,
				Value = statisticBulkDTO.MinigunKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunDeaths,
				Value = statisticBulkDTO.MinigunDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunShardKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunShardKills,
				Value = statisticBulkDTO.MinigunShardKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunShardDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunShardDeaths,
				Value = statisticBulkDTO.MinigunShardDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunShots,
				Value = statisticBulkDTO.MinigunShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.MinigunHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.MinigunHits,
				Value = statisticBulkDTO.MinigunHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShardKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShardKills,
				Value = statisticBulkDTO.FlakShardKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShardDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShardDeaths,
				Value = statisticBulkDTO.FlakShardDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShellKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShellKills,
				Value = statisticBulkDTO.FlakShellKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShellDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShellDeaths,
				Value = statisticBulkDTO.FlakShellDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakShots,
				Value = statisticBulkDTO.FlakShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlakHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlakHits,
				Value = statisticBulkDTO.FlakHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RocketKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RocketKills,
				Value = statisticBulkDTO.RocketKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RocketDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RocketDeaths,
				Value = statisticBulkDTO.RocketDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RocketShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RocketShots,
				Value = statisticBulkDTO.RocketShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RocketHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RocketHits,
				Value = statisticBulkDTO.RocketHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperKills,
				Value = statisticBulkDTO.SniperKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperDeaths,
				Value = statisticBulkDTO.SniperDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperHeadshotKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperHeadshotKills,
				Value = statisticBulkDTO.SniperHeadshotKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperHeadshotDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperHeadshotDeaths,
				Value = statisticBulkDTO.SniperHeadshotDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperShots,
				Value = statisticBulkDTO.SniperShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SniperHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SniperHits,
				Value = statisticBulkDTO.SniperHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRiflePrimaryKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRiflePrimaryKills,
				Value = statisticBulkDTO.LightningRiflePrimaryKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRiflePrimaryDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRiflePrimaryDeaths,
				Value = statisticBulkDTO.LightningRiflePrimaryDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRifleSecondaryKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRifleSecondaryKills,
				Value = statisticBulkDTO.LightningRifleSecondaryKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRifleSecondaryDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRifleSecondaryDeaths,
				Value = statisticBulkDTO.LightningRifleSecondaryDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRifleShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRifleShots,
				Value = statisticBulkDTO.LightningRifleShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.LightningRifleHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.LightningRifleHits,
				Value = statisticBulkDTO.LightningRifleHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RedeemerKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RedeemerKills,
				Value = statisticBulkDTO.RedeemerKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RedeemerDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RedeemerDeaths,
				Value = statisticBulkDTO.RedeemerDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RedeemerShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RedeemerShots,
				Value = statisticBulkDTO.RedeemerShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RedeemerHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RedeemerHits,
				Value = statisticBulkDTO.RedeemerHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InstagibKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InstagibKills,
				Value = statisticBulkDTO.InstagibKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InstagibDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InstagibDeaths,
				Value = statisticBulkDTO.InstagibDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InstagibShots.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InstagibShots,
				Value = statisticBulkDTO.InstagibShots.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InstagibHits.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InstagibHits,
				Value = statisticBulkDTO.InstagibHits.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TelefragKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TelefragKills,
				Value = statisticBulkDTO.TelefragKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TelefragDeaths.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TelefragDeaths,
				Value = statisticBulkDTO.TelefragDeaths.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Miscellaneous - Movement

		if (statisticBulkDTO.RunDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RunDist,
				Value = statisticBulkDTO.RunDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SprintDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SprintDist,
				Value = statisticBulkDTO.SprintDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.InAirDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.InAirDist,
				Value = statisticBulkDTO.InAirDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SwimDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SwimDist,
				Value = statisticBulkDTO.SwimDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TranslocDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TranslocDist,
				Value = statisticBulkDTO.TranslocDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumDodges.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumDodges,
				Value = statisticBulkDTO.NumDodges.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumWallDodges.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumWallDodges,
				Value = statisticBulkDTO.NumWallDodges.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumJumps,
				Value = statisticBulkDTO.NumJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumLiftJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumLiftJumps,
				Value = statisticBulkDTO.NumLiftJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumFloorSlides.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumFloorSlides,
				Value = statisticBulkDTO.NumFloorSlides.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumWallRuns.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumWallRuns,
				Value = statisticBulkDTO.NumWallRuns.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumImpactJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumImpactJumps,
				Value = statisticBulkDTO.NumImpactJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.NumRocketJumps.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.NumRocketJumps,
				Value = statisticBulkDTO.NumRocketJumps.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SlideDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SlideDist,
				Value = statisticBulkDTO.SlideDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.WallRunDist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.WallRunDist,
				Value = statisticBulkDTO.WallRunDist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		#region Miscellaneous - Capture the Flag

		if (statisticBulkDTO.FlagCaptures.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagCaptures,
				Value = statisticBulkDTO.FlagCaptures.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagReturns.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagReturns,
				Value = statisticBulkDTO.FlagReturns.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagAssists.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagAssists,
				Value = statisticBulkDTO.FlagAssists.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagHeldDeny.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagHeldDeny,
				Value = statisticBulkDTO.FlagHeldDeny.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagHeldDenyTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagHeldDenyTime,
				Value = statisticBulkDTO.FlagHeldDenyTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagHeldTime.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagHeldTime,
				Value = statisticBulkDTO.FlagHeldTime.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagReturnPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagReturnPoints,
				Value = statisticBulkDTO.FlagReturnPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.CarryAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.CarryAssist,
				Value = statisticBulkDTO.CarryAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.CarryAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.CarryAssistPoints,
				Value = statisticBulkDTO.CarryAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagCapPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagCapPoints,
				Value = statisticBulkDTO.FlagCapPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.DefendAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.DefendAssist,
				Value = statisticBulkDTO.DefendAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.DefendAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.DefendAssistPoints,
				Value = statisticBulkDTO.DefendAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ReturnAssist.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ReturnAssist,
				Value = statisticBulkDTO.ReturnAssist.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.ReturnAssistPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.ReturnAssistPoints,
				Value = statisticBulkDTO.ReturnAssistPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TeamCapPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TeamCapPoints,
				Value = statisticBulkDTO.TeamCapPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.EnemyFCDamage.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.EnemyFCDamage,
				Value = statisticBulkDTO.EnemyFCDamage.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FCKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FCKills,
				Value = statisticBulkDTO.FCKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FCKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FCKillPoints,
				Value = statisticBulkDTO.FCKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagSupportKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagSupportKills,
				Value = statisticBulkDTO.FlagSupportKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagSupportKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagSupportKillPoints,
				Value = statisticBulkDTO.FlagSupportKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.RegularKillPoints.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.RegularKillPoints,
				Value = statisticBulkDTO.RegularKillPoints.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.FlagGrabs.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.FlagGrabs,
				Value = statisticBulkDTO.FlagGrabs.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.AttackerScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.AttackerScore,
				Value = statisticBulkDTO.AttackerScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.DefenderScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.DefenderScore,
				Value = statisticBulkDTO.DefenderScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.SupporterScore.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.SupporterScore,
				Value = statisticBulkDTO.SupporterScore.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		if (statisticBulkDTO.TeamKills.HasValue)
		{
			newStatistics.Add(new Statistic()
			{
				CreatedAt = currentDateTime,
				AccountID = accountID,
				Type = StatisticType.TeamKills,
				Value = statisticBulkDTO.TeamKills.Value,
				Window = StatisticWindow.Daily,
				OwnerType = ownerType
			});
		}

		#endregion

		return newStatistics;
	}

	private async Task UpdateAllTimeAccountStatistics(EpicID accountID, List<Statistic> newStatistics)
	{
		logger.LogInformation("Updating all-time statistics for account: {AccountId}.", accountID);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime) &
					 Builders<Statistic>.Filter.In(f => f.Type, newStatistics.Select(s => s.Type).ToList());

		var existingStatistics = await statisticsCollection.Find(filter).ToListAsync();

		foreach (var newStatistic in newStatistics)
		{
			if (existingStatistics.FirstOrDefault(f => f.Type == newStatistic.Type) is { } existingStatistic)
			{
				await statisticsCollection.UpdateOneAsync(
					f => f.ID == existingStatistic.ID,
					Builders<Statistic>.Update.Inc(i => i.Value, newStatistic.Value)
											  .Set(s => s.ModifiedAt, DateTime.UtcNow));
			}
			else
			{
				await statisticsCollection.InsertOneAsync(new Statistic()
				{
					AccountID = accountID,
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
