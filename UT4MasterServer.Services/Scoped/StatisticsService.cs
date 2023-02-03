using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Common;
using Microsoft.Extensions.Logging;

namespace UT4MasterServer.Services.Scoped;

public sealed class StatisticsService
{
	private readonly ILogger<StatisticsService> logger;
	private readonly IMongoCollection<Statistic> statisticsCollection;

	private const int MinimumDaysForDeletingOldStatistics = 30;

	public StatisticsService(
		ILogger<StatisticsService> logger,
		DatabaseContext dbContext)
	{
		this.logger = logger;
		statisticsCollection = dbContext.Database.GetCollection<Statistic>("statistics");
	}

	public async Task CreateIndexesAsync()
	{
		var statisticsIndexes = new List<CreateIndexModel<Statistic>>()
			{
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.AccountID)),
				new CreateIndexModel<Statistic>(Builders<Statistic>.IndexKeys.Ascending(indexKey => indexKey.CreatedAt)),
			};
		await statisticsCollection.Indexes.CreateManyAsync(statisticsIndexes);
	}

	public async Task<List<StatisticDTO>> GetAggregateAccountStatisticsAsync(EpicID accountID, StatisticWindow statisticWindow)
	{
		logger.LogInformation("Calculating {StatisticWindow} statistics for account: {AccountID}.", statisticWindow.ToString().ToLower(), accountID);

		var dateFrom = DateTime.UtcNow.Date;
		if (statisticWindow != StatisticWindow.Daily)
		{
			dateFrom = DateTime.UtcNow.AddDays(-(int)statisticWindow).Date;
		}
		var dateTo = DateTime.UtcNow.AddDays(1).Date;
		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Statistic>.Filter.In(f => f.Window, new[] { StatisticWindow.Daily, StatisticWindow.DailyMerged }) &
					 Builders<Statistic>.Filter.Gte(f => f.CreatedAt, dateFrom) &
					 Builders<Statistic>.Filter.Lt(f => f.CreatedAt, dateTo);

		var statisticsGrouped = await statisticsCollection
			.Aggregate()
			.Match(filter)
			.Group(k => k.AccountID,
				   g => new StatisticBase
				   {
					   MatchesPlayed = g.Sum(s => s.MatchesPlayed),
					   MatchesQuit = g.Sum(s => s.MatchesQuit),
					   TimePlayed = g.Sum(s => s.TimePlayed),
					   Wins = g.Sum(s => s.Wins),
					   Losses = g.Sum(s => s.Losses),
					   Kills = g.Sum(s => s.Kills),
					   Deaths = g.Sum(s => s.Deaths),
					   Suicides = g.Sum(s => s.Suicides),
					   MultiKillLevel0 = g.Sum(s => s.MultiKillLevel0),
					   MultiKillLevel1 = g.Sum(s => s.MultiKillLevel1),
					   MultiKillLevel2 = g.Sum(s => s.MultiKillLevel2),
					   MultiKillLevel3 = g.Sum(s => s.MultiKillLevel3),
					   SpreeKillLevel0 = g.Sum(s => s.SpreeKillLevel0),
					   SpreeKillLevel1 = g.Sum(s => s.SpreeKillLevel1),
					   SpreeKillLevel2 = g.Sum(s => s.SpreeKillLevel2),
					   SpreeKillLevel3 = g.Sum(s => s.SpreeKillLevel3),
					   SpreeKillLevel4 = g.Sum(s => s.SpreeKillLevel4),
					   BestShockCombo = g.Sum(s => s.BestShockCombo),
					   AmazingCombos = g.Sum(s => s.AmazingCombos),
					   AirRox = g.Sum(s => s.AirRox),
					   FlakShreds = g.Sum(s => s.FlakShreds),
					   AirSnot = g.Sum(s => s.AirSnot),
					   UDamageTime = g.Sum(s => s.UDamageTime),
					   BerserkTime = g.Sum(s => s.BerserkTime),
					   InvisibilityTime = g.Sum(s => s.InvisibilityTime),
					   UDamageCount = g.Sum(s => s.UDamageCount),
					   BerserkCount = g.Sum(s => s.BerserkCount),
					   InvisibilityCount = g.Sum(s => s.InvisibilityCount),
					   BootJumps = g.Sum(s => s.BootJumps),
					   ShieldBeltCount = g.Sum(s => s.ShieldBeltCount),
					   ArmorVestCount = g.Sum(s => s.ArmorVestCount),
					   ArmorPadsCount = g.Sum(s => s.ArmorPadsCount),
					   HelmetCount = g.Sum(s => s.HelmetCount),
					   KegCount = g.Sum(s => s.KegCount),
					   ImpactHammerKills = g.Sum(s => s.ImpactHammerKills),
					   ImpactHammerDeaths = g.Sum(s => s.ImpactHammerDeaths),
					   EnforcerKills = g.Sum(s => s.EnforcerKills),
					   EnforcerDeaths = g.Sum(s => s.EnforcerDeaths),
					   EnforcerShots = g.Sum(s => s.EnforcerShots),
					   EnforcerHits = g.Sum(s => s.EnforcerHits),
					   BioRifleKills = g.Sum(s => s.BioRifleKills),
					   BioRifleDeaths = g.Sum(s => s.BioRifleDeaths),
					   BioRifleShots = g.Sum(s => s.BioRifleShots),
					   BioRifleHits = g.Sum(s => s.BioRifleHits),
					   BioLauncherKills = g.Sum(s => s.BioLauncherKills),
					   BioLauncherDeaths = g.Sum(s => s.BioLauncherDeaths),
					   BioLauncherShots = g.Sum(s => s.BioLauncherShots),
					   BioLauncherHits = g.Sum(s => s.BioLauncherHits),
					   ShockBeamKills = g.Sum(s => s.ShockBeamKills),
					   ShockBeamDeaths = g.Sum(s => s.ShockBeamDeaths),
					   ShockCoreKills = g.Sum(s => s.ShockCoreKills),
					   ShockCoreDeaths = g.Sum(s => s.ShockCoreDeaths),
					   ShockComboKills = g.Sum(s => s.ShockComboKills),
					   ShockComboDeaths = g.Sum(s => s.ShockComboDeaths),
					   ShockRifleShots = g.Sum(s => s.ShockRifleShots),
					   ShockRifleHits = g.Sum(s => s.ShockRifleHits),
					   LinkKills = g.Sum(s => s.LinkKills),
					   LinkDeaths = g.Sum(s => s.LinkDeaths),
					   LinkBeamKills = g.Sum(s => s.LinkBeamKills),
					   LinkBeamDeaths = g.Sum(s => s.LinkBeamDeaths),
					   LinkShots = g.Sum(s => s.LinkShots),
					   LinkHits = g.Sum(s => s.LinkHits),
					   MinigunKills = g.Sum(s => s.MinigunKills),
					   MinigunDeaths = g.Sum(s => s.MinigunDeaths),
					   MinigunShardKills = g.Sum(s => s.MinigunShardKills),
					   MinigunShardDeaths = g.Sum(s => s.MinigunShardDeaths),
					   MinigunShots = g.Sum(s => s.MinigunShots),
					   MinigunHits = g.Sum(s => s.MinigunHits),
					   FlakShardKills = g.Sum(s => s.FlakShardKills),
					   FlakShardDeaths = g.Sum(s => s.FlakShardDeaths),
					   FlakShellKills = g.Sum(s => s.FlakShellKills),
					   FlakShellDeaths = g.Sum(s => s.FlakShellDeaths),
					   FlakShots = g.Sum(s => s.FlakShots),
					   FlakHits = g.Sum(s => s.FlakHits),
					   RocketKills = g.Sum(s => s.RocketKills),
					   RocketDeaths = g.Sum(s => s.RocketDeaths),
					   RocketShots = g.Sum(s => s.RocketShots),
					   RocketHits = g.Sum(s => s.RocketHits),
					   SniperKills = g.Sum(s => s.SniperKills),
					   SniperDeaths = g.Sum(s => s.SniperDeaths),
					   SniperHeadshotKills = g.Sum(s => s.SniperHeadshotKills),
					   SniperHeadshotDeaths = g.Sum(s => s.SniperHeadshotDeaths),
					   SniperShots = g.Sum(s => s.SniperShots),
					   SniperHits = g.Sum(s => s.SniperHits),
					   LightningRiflePrimaryKills = g.Sum(s => s.LightningRiflePrimaryKills),
					   LightningRiflePrimaryDeaths = g.Sum(s => s.LightningRiflePrimaryDeaths),
					   LightningRifleSecondaryKills = g.Sum(s => s.LightningRifleSecondaryKills),
					   LightningRifleSecondaryDeaths = g.Sum(s => s.LightningRifleSecondaryDeaths),
					   LightningRifleShots = g.Sum(s => s.LightningRifleShots),
					   LightningRifleHits = g.Sum(s => s.LightningRifleHits),
					   RedeemerKills = g.Sum(s => s.RedeemerKills),
					   RedeemerDeaths = g.Sum(s => s.RedeemerDeaths),
					   RedeemerShots = g.Sum(s => s.RedeemerShots),
					   RedeemerHits = g.Sum(s => s.RedeemerHits),
					   InstagibKills = g.Sum(s => s.InstagibKills),
					   InstagibDeaths = g.Sum(s => s.InstagibDeaths),
					   InstagibShots = g.Sum(s => s.InstagibShots),
					   InstagibHits = g.Sum(s => s.InstagibHits),
					   TelefragKills = g.Sum(s => s.TelefragKills),
					   TelefragDeaths = g.Sum(s => s.TelefragDeaths),
					   RunDist = g.Sum(s => s.RunDist),
					   SprintDist = g.Sum(s => s.SprintDist),
					   InAirDist = g.Sum(s => s.InAirDist),
					   SwimDist = g.Sum(s => s.SwimDist),
					   TranslocDist = g.Sum(s => s.TranslocDist),
					   NumDodges = g.Sum(s => s.NumDodges),
					   NumWallDodges = g.Sum(s => s.NumWallDodges),
					   NumJumps = g.Sum(s => s.NumJumps),
					   NumLiftJumps = g.Sum(s => s.NumLiftJumps),
					   NumFloorSlides = g.Sum(s => s.NumFloorSlides),
					   NumWallRuns = g.Sum(s => s.NumWallRuns),
					   NumImpactJumps = g.Sum(s => s.NumImpactJumps),
					   NumRocketJumps = g.Sum(s => s.NumRocketJumps),
					   SlideDist = g.Sum(s => s.SlideDist),
					   WallRunDist = g.Sum(s => s.WallRunDist),
					   FlagCaptures = g.Sum(s => s.FlagCaptures),
					   FlagReturns = g.Sum(s => s.FlagReturns),
					   FlagAssists = g.Sum(s => s.FlagAssists),
					   FlagHeldDeny = g.Sum(s => s.FlagHeldDeny),
					   FlagHeldDenyTime = g.Sum(s => s.FlagHeldDenyTime),
					   FlagHeldTime = g.Sum(s => s.FlagHeldTime),
					   FlagReturnPoints = g.Sum(s => s.FlagReturnPoints),
					   CarryAssist = g.Sum(s => s.CarryAssist),
					   CarryAssistPoints = g.Sum(s => s.CarryAssistPoints),
					   FlagCapPoints = g.Sum(s => s.FlagCapPoints),
					   DefendAssist = g.Sum(s => s.DefendAssist),
					   DefendAssistPoints = g.Sum(s => s.DefendAssistPoints),
					   ReturnAssist = g.Sum(s => s.ReturnAssist),
					   ReturnAssistPoints = g.Sum(s => s.ReturnAssistPoints),
					   TeamCapPoints = g.Sum(s => s.TeamCapPoints),
					   EnemyFCDamage = g.Sum(s => s.EnemyFCDamage),
					   FCKills = g.Sum(s => s.FCKills),
					   FCKillPoints = g.Sum(s => s.FCKillPoints),
					   FlagSupportKills = g.Sum(s => s.FlagSupportKills),
					   FlagSupportKillPoints = g.Sum(s => s.FlagSupportKillPoints),
					   RegularKillPoints = g.Sum(s => s.RegularKillPoints),
					   FlagGrabs = g.Sum(s => s.FlagGrabs),
					   AttackerScore = g.Sum(s => s.AttackerScore),
					   DefenderScore = g.Sum(s => s.DefenderScore),
					   SupporterScore = g.Sum(s => s.SupporterScore),
					   TeamKills = g.Sum(s => s.TeamKills),
				   })
			.FirstOrDefaultAsync();

		return MapStatisticBaseToStatisticDTO(statisticsGrouped, statisticWindow);
	}

	public async Task<List<StatisticDTO>> GetAllTimeAccountStatisticsAsync(EpicID accountID)
	{
		logger.LogInformation("Calculating all-time statistics for account: {AccountID}.", accountID);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime);

		var statistics = await statisticsCollection.Find(filter).FirstOrDefaultAsync();

		var result = new List<StatisticDTO>();

		if (statistics is not null)
		{
			result = MapStatisticBaseToStatisticDTO(statistics, StatisticWindow.AllTime);
		}

		return result;
	}

	public async Task CreateAccountStatisticsAsync(EpicID accountID, OwnerType _, StatisticBase statisticBase)
	{
		logger.LogInformation("Creating statistics for account: {AccountID}.", accountID);

		var newStatistic = new Statistic(statisticBase)
		{
			AccountID = accountID,
			CreatedAt = DateTime.UtcNow,
			Window = StatisticWindow.Daily,
		};

		var flags = statisticBase.Validate();
		if (flags.Any())
		{
			newStatistic.Flagged = flags;
		}

		await statisticsCollection.InsertOneAsync(newStatistic);
		await UpdateAllTimeAccountStatisticsAsync(newStatistic);
	}

	/// <summary>
	/// This method merges multiple statistics into one. It looks only for daily non-flagged statistics to merge that are at least 7 days old.
	/// </summary>
	/// <returns></returns>
	public async Task MergeOldStatisticsAsync()
	{
		var dateBefore = DateTime.UtcNow.AddDays(-7).Date;

		logger.LogInformation("Merging statistics older than: {Date}.", dateBefore);

		var filter = Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.Daily) &
					 Builders<Statistic>.Filter.Lt(f => f.CreatedAt, dateBefore) &
					 Builders<Statistic>.Filter.Exists(f => f.Flagged, false);

		var statistics = await statisticsCollection.Find(filter).ToListAsync();

		if (!statistics.Any()) return;

		var statisticsGrouped = statistics
			.GroupBy(g => new { g.AccountID, g.CreatedAt.Date })
			.Select(s => new
			{
				AccountId = s.Key.AccountID,
				CreatedAt = s.Key.Date,
				Statistics = s.ToList(),
				Count = s.Count(),
			})
			.ToList();

		var accountIds = statisticsGrouped.Select(s => s.AccountId).ToArray();
		var dates = statisticsGrouped.Select(s => s.CreatedAt).ToArray();
		var alreadyMergedFilter = Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.DailyMerged) &
								  Builders<Statistic>.Filter.In(f => f.AccountID, accountIds) &
								  Builders<Statistic>.Filter.In(f => f.CreatedAt, dates);

		// This will fetch previously merged records so that un-flagged records can be merged with them
		var alreadyMergedStatistics = await statisticsCollection.Find(alreadyMergedFilter).ToListAsync();

		var mergedStatistics = new List<Statistic>();
		var modifiedStatisticIds = new List<string>();
		var statisticIdsToDelete = new List<string>();

		foreach (var statisticsGroup in statisticsGrouped)
		{
			// Statistics that have multiple records per day should be merged together or merged with already merged statistics
			if (statisticsGroup.Count > 1)
			{
				var statisticsToMerge = new List<Statistic>(statisticsGroup.Statistics);

				if (alreadyMergedStatistics.FirstOrDefault(f => f.AccountID == statisticsGroup.AccountId &&
																f.CreatedAt.Date == statisticsGroup.CreatedAt.Date) is { } alreadyMergedStatistic)
				{
					statisticsToMerge.Add(alreadyMergedStatistic);
				}

				var mergedStatistic = MergeStatistics(statisticsToMerge);
				mergedStatistic.CreatedAt = mergedStatistic.CreatedAt.Date;
				mergedStatistic.Window = StatisticWindow.DailyMerged;
				mergedStatistics.Add(mergedStatistic);

				var mergedStatisticsIds = statisticsToMerge.Select(s => s.ID).ToArray();

				statisticIdsToDelete.AddRange(mergedStatisticsIds);

				logger.LogInformation("Merged the following statistics into one record: {StatisticIds}.", string.Join(", ", mergedStatisticsIds));
			}
			// Statistics that have one record per day should be either modified or merged with already merged statistics
			else
			{
				if (alreadyMergedStatistics.FirstOrDefault(f => f.AccountID == statisticsGroup.AccountId &&
																f.CreatedAt.Date == statisticsGroup.CreatedAt.Date) is { } alreadyMergedStatistic)
				{
					var statisticsToMerge = new List<Statistic>(statisticsGroup.Statistics) { alreadyMergedStatistic };
					var mergedStatistic = MergeStatistics(statisticsToMerge);
					mergedStatistic.CreatedAt = mergedStatistic.CreatedAt.Date;
					mergedStatistic.Window = StatisticWindow.DailyMerged;
					mergedStatistics.Add(mergedStatistic);

					var mergedStatisticsIds = statisticsToMerge.Select(s => s.ID).ToArray();

					statisticIdsToDelete.AddRange(mergedStatisticsIds);

					logger.LogInformation("Merged the following statistics into one record: {StatisticIds}.", string.Join(", ", mergedStatisticsIds));
				}
				else
				{
					var modifiedStatisticId = statisticsGroup.Statistics.First().ID;
					modifiedStatisticIds.Add(modifiedStatisticId);
					logger.LogInformation("Modifying the window for statistic: {StatisticId}.", modifiedStatisticId);
				}
			}
		}

		// Inserting merged statistics and deleting the old ones
		if (mergedStatistics.Any())
		{
			await statisticsCollection.InsertManyAsync(mergedStatistics);

			var deleteFilter = Builders<Statistic>.Filter.In(f => f.ID, statisticIdsToDelete);
			await statisticsCollection.DeleteManyAsync(deleteFilter);
			logger.LogInformation("Deleting the following statistics: {StatisticIds}.", string.Join(", ", statisticIdsToDelete));
		}

		// Modifying Window and removing Flagged property for statistics which are single per day
		if (modifiedStatisticIds.Any())
		{
			var bulkWriteModelList = new List<WriteModel<Statistic>>();
			foreach (var modifiedStatisticId in modifiedStatisticIds)
			{
				var updateFilter = Builders<Statistic>.Filter.Eq(f => f.ID, modifiedStatisticId);
				var updateDefinition = Builders<Statistic>.Update
					.Set(s => s.Window, StatisticWindow.DailyMerged)
					.Unset(u => u.Flagged);
				bulkWriteModelList.Add(new UpdateOneModel<Statistic>(updateFilter, updateDefinition));
			}
			await statisticsCollection.BulkWriteAsync(bulkWriteModelList);
		}
	}

	#region Private methods

	/// <summary>
	/// This method will create or update all-time statistics record in the DB
	/// </summary>
	/// <param name="newStatistic"></param>
	/// <returns></returns>
	private async Task UpdateAllTimeAccountStatisticsAsync(Statistic newStatistic)
	{
		logger.LogInformation("Updating all-time statistics for account: {AccountID}.", newStatistic.AccountID);

		var filter = Builders<Statistic>.Filter.Eq(f => f.AccountID, newStatistic.AccountID) &
					 Builders<Statistic>.Filter.Eq(f => f.Window, StatisticWindow.AllTime);

		var existingStatistic = await statisticsCollection.Find(filter).FirstOrDefaultAsync();

		if (existingStatistic is not null)
		{
			var mergedStatistic = MergeStatistics(new List<Statistic>() { existingStatistic, newStatistic });
			mergedStatistic.Window = StatisticWindow.AllTime;
			mergedStatistic.ModifiedAt = DateTime.UtcNow;

			await statisticsCollection.ReplaceOneAsync(Builders<Statistic>.Filter.Eq(f => f.ID, existingStatistic.ID), mergedStatistic);
		}
		else
		{
			var newAllTimeStatistics = new Statistic(newStatistic)
			{
				AccountID = newStatistic.AccountID,
				Window = StatisticWindow.AllTime,
			};

			await statisticsCollection.InsertOneAsync(newAllTimeStatistics);
		}
	}

	/// <summary>
	/// This method will convert statistics from flat structure to array structure
	/// Float values will be multiplied by 100
	/// </summary>
	/// <param name="statisticWindow"></param>
	/// <returns></returns>
	private static List<StatisticDTO> MapStatisticBaseToStatisticDTO(StatisticBase statisticBase, StatisticWindow statisticWindow)
	{
		var result = new List<StatisticDTO>();

		if (statisticBase is not null)
		{
			foreach (var element in statisticBase.ToBsonDocument().Elements)
			{
				if (!StatisticBase.StatisticProperties.Contains(element.Name.ToLower())) continue;

				var value = element.Value.ToInt64();

				if (value > 0)
				{
					result.Add(new StatisticDTO()
					{
						Name = element.Name,
						Value = value,
						Window = statisticWindow.ToString().ToLower(),
						OwnerType = OwnerType.Default,
					});
				}
			}
		}

		return result;
	}

	/// <summary>
	/// This method will merge list of Statistic objects by account ID and it will sum up all values
	/// </summary>
	/// <param name="statistics"></param>
	/// <returns>Statistic object</returns>
	private static Statistic MergeStatistics(List<Statistic> statistics)
	{
		var merged = statistics
			.GroupBy(g => g.AccountID)
			.Select(s => new Statistic
			{
				AccountID = s.Key,
				CreatedAt = s.First().CreatedAt,
				Window = s.First().Window,
				MatchesPlayed = s.Any(a => a.MatchesPlayed.HasValue) ? s.Sum(sm => sm.MatchesPlayed) : null,
				MatchesQuit = s.Any(a => a.MatchesQuit.HasValue) ? s.Sum(sm => sm.MatchesQuit) : null,
				TimePlayed = s.Any(a => a.TimePlayed.HasValue) ? s.Sum(sm => sm.TimePlayed) : null,
				Wins = s.Any(a => a.Wins.HasValue) ? s.Sum(sm => sm.Wins) : null,
				Losses = s.Any(a => a.Losses.HasValue) ? s.Sum(sm => sm.Losses) : null,
				Kills = s.Any(a => a.Kills.HasValue) ? s.Sum(sm => sm.Kills) : null,
				Deaths = s.Any(a => a.Deaths.HasValue) ? s.Sum(sm => sm.Deaths) : null,
				Suicides = s.Any(a => a.Suicides.HasValue) ? s.Sum(sm => sm.Suicides) : null,
				MultiKillLevel0 = s.Any(a => a.MultiKillLevel0.HasValue) ? s.Sum(sm => sm.MultiKillLevel0) : null,
				MultiKillLevel1 = s.Any(a => a.MultiKillLevel1.HasValue) ? s.Sum(sm => sm.MultiKillLevel1) : null,
				MultiKillLevel2 = s.Any(a => a.MultiKillLevel2.HasValue) ? s.Sum(sm => sm.MultiKillLevel2) : null,
				MultiKillLevel3 = s.Any(a => a.MultiKillLevel3.HasValue) ? s.Sum(sm => sm.MultiKillLevel3) : null,
				SpreeKillLevel0 = s.Any(a => a.SpreeKillLevel0.HasValue) ? s.Sum(sm => sm.SpreeKillLevel0) : null,
				SpreeKillLevel1 = s.Any(a => a.SpreeKillLevel1.HasValue) ? s.Sum(sm => sm.SpreeKillLevel1) : null,
				SpreeKillLevel2 = s.Any(a => a.SpreeKillLevel2.HasValue) ? s.Sum(sm => sm.SpreeKillLevel2) : null,
				SpreeKillLevel3 = s.Any(a => a.SpreeKillLevel3.HasValue) ? s.Sum(sm => sm.SpreeKillLevel3) : null,
				SpreeKillLevel4 = s.Any(a => a.SpreeKillLevel4.HasValue) ? s.Sum(sm => sm.SpreeKillLevel4) : null,
				BestShockCombo = s.Any(a => a.BestShockCombo.HasValue) ? s.Sum(sm => sm.BestShockCombo) : null,
				AmazingCombos = s.Any(a => a.AmazingCombos.HasValue) ? s.Sum(sm => sm.AmazingCombos) : null,
				AirRox = s.Any(a => a.AirRox.HasValue) ? s.Sum(sm => sm.AirRox) : null,
				FlakShreds = s.Any(a => a.FlakShreds.HasValue) ? s.Sum(sm => sm.FlakShreds) : null,
				AirSnot = s.Any(a => a.AirSnot.HasValue) ? s.Sum(sm => sm.AirSnot) : null,
				UDamageTime = s.Any(a => a.UDamageTime.HasValue) ? s.Sum(sm => sm.UDamageTime) : null,
				BerserkTime = s.Any(a => a.BerserkTime.HasValue) ? s.Sum(sm => sm.BerserkTime) : null,
				InvisibilityTime = s.Any(a => a.InvisibilityTime.HasValue) ? s.Sum(sm => sm.InvisibilityTime) : null,
				UDamageCount = s.Any(a => a.UDamageCount.HasValue) ? s.Sum(sm => sm.UDamageCount) : null,
				BerserkCount = s.Any(a => a.BerserkCount.HasValue) ? s.Sum(sm => sm.BerserkCount) : null,
				InvisibilityCount = s.Any(a => a.InvisibilityCount.HasValue) ? s.Sum(sm => sm.InvisibilityCount) : null,
				BootJumps = s.Any(a => a.BootJumps.HasValue) ? s.Sum(sm => sm.BootJumps) : null,
				ShieldBeltCount = s.Any(a => a.ShieldBeltCount.HasValue) ? s.Sum(sm => sm.ShieldBeltCount) : null,
				ArmorVestCount = s.Any(a => a.ArmorVestCount.HasValue) ? s.Sum(sm => sm.ArmorVestCount) : null,
				ArmorPadsCount = s.Any(a => a.ArmorPadsCount.HasValue) ? s.Sum(sm => sm.ArmorPadsCount) : null,
				HelmetCount = s.Any(a => a.HelmetCount.HasValue) ? s.Sum(sm => sm.HelmetCount) : null,
				KegCount = s.Any(a => a.KegCount.HasValue) ? s.Sum(sm => sm.KegCount) : null,
				ImpactHammerKills = s.Any(a => a.ImpactHammerKills.HasValue) ? s.Sum(sm => sm.ImpactHammerKills) : null,
				ImpactHammerDeaths = s.Any(a => a.ImpactHammerDeaths.HasValue) ? s.Sum(sm => sm.ImpactHammerDeaths) : null,
				EnforcerKills = s.Any(a => a.EnforcerKills.HasValue) ? s.Sum(sm => sm.EnforcerKills) : null,
				EnforcerDeaths = s.Any(a => a.EnforcerDeaths.HasValue) ? s.Sum(sm => sm.EnforcerDeaths) : null,
				EnforcerShots = s.Any(a => a.EnforcerShots.HasValue) ? s.Sum(sm => sm.EnforcerShots) : null,
				EnforcerHits = s.Any(a => a.EnforcerHits.HasValue) ? s.Sum(sm => sm.EnforcerHits) : null,
				BioRifleKills = s.Any(a => a.BioRifleKills.HasValue) ? s.Sum(sm => sm.BioRifleKills) : null,
				BioRifleDeaths = s.Any(a => a.BioRifleDeaths.HasValue) ? s.Sum(sm => sm.BioRifleDeaths) : null,
				BioRifleShots = s.Any(a => a.BioRifleShots.HasValue) ? s.Sum(sm => sm.BioRifleShots) : null,
				BioRifleHits = s.Any(a => a.BioRifleHits.HasValue) ? s.Sum(sm => sm.BioRifleHits) : null,
				BioLauncherKills = s.Any(a => a.BioLauncherKills.HasValue) ? s.Sum(sm => sm.BioLauncherKills) : null,
				BioLauncherDeaths = s.Any(a => a.BioLauncherDeaths.HasValue) ? s.Sum(sm => sm.BioLauncherDeaths) : null,
				BioLauncherShots = s.Any(a => a.BioLauncherShots.HasValue) ? s.Sum(sm => sm.BioLauncherShots) : null,
				BioLauncherHits = s.Any(a => a.BioLauncherHits.HasValue) ? s.Sum(sm => sm.BioLauncherHits) : null,
				ShockBeamKills = s.Any(a => a.ShockBeamKills.HasValue) ? s.Sum(sm => sm.ShockBeamKills) : null,
				ShockBeamDeaths = s.Any(a => a.ShockBeamDeaths.HasValue) ? s.Sum(sm => sm.ShockBeamDeaths) : null,
				ShockCoreKills = s.Any(a => a.ShockCoreKills.HasValue) ? s.Sum(sm => sm.ShockCoreKills) : null,
				ShockCoreDeaths = s.Any(a => a.ShockCoreDeaths.HasValue) ? s.Sum(sm => sm.ShockCoreDeaths) : null,
				ShockComboKills = s.Any(a => a.ShockComboKills.HasValue) ? s.Sum(sm => sm.ShockComboKills) : null,
				ShockComboDeaths = s.Any(a => a.ShockComboDeaths.HasValue) ? s.Sum(sm => sm.ShockComboDeaths) : null,
				ShockRifleShots = s.Any(a => a.ShockRifleShots.HasValue) ? s.Sum(sm => sm.ShockRifleShots) : null,
				ShockRifleHits = s.Any(a => a.ShockRifleHits.HasValue) ? s.Sum(sm => sm.ShockRifleHits) : null,
				LinkKills = s.Any(a => a.LinkKills.HasValue) ? s.Sum(sm => sm.LinkKills) : null,
				LinkDeaths = s.Any(a => a.LinkDeaths.HasValue) ? s.Sum(sm => sm.LinkDeaths) : null,
				LinkBeamKills = s.Any(a => a.LinkBeamKills.HasValue) ? s.Sum(sm => sm.LinkBeamKills) : null,
				LinkBeamDeaths = s.Any(a => a.LinkBeamDeaths.HasValue) ? s.Sum(sm => sm.LinkBeamDeaths) : null,
				LinkShots = s.Any(a => a.LinkShots.HasValue) ? s.Sum(sm => sm.LinkShots) : null,
				LinkHits = s.Any(a => a.LinkHits.HasValue) ? s.Sum(sm => sm.LinkHits) : null,
				MinigunKills = s.Any(a => a.MinigunKills.HasValue) ? s.Sum(sm => sm.MinigunKills) : null,
				MinigunDeaths = s.Any(a => a.MinigunDeaths.HasValue) ? s.Sum(sm => sm.MinigunDeaths) : null,
				MinigunShardKills = s.Any(a => a.MinigunShardKills.HasValue) ? s.Sum(sm => sm.MinigunShardKills) : null,
				MinigunShardDeaths = s.Any(a => a.MinigunShardDeaths.HasValue) ? s.Sum(sm => sm.MinigunShardDeaths) : null,
				MinigunShots = s.Any(a => a.MinigunShots.HasValue) ? s.Sum(sm => sm.MinigunShots) : null,
				MinigunHits = s.Any(a => a.MinigunHits.HasValue) ? s.Sum(sm => sm.MinigunHits) : null,
				FlakShardKills = s.Any(a => a.FlakShardKills.HasValue) ? s.Sum(sm => sm.FlakShardKills) : null,
				FlakShardDeaths = s.Any(a => a.FlakShardDeaths.HasValue) ? s.Sum(sm => sm.FlakShardDeaths) : null,
				FlakShellKills = s.Any(a => a.FlakShellKills.HasValue) ? s.Sum(sm => sm.FlakShellKills) : null,
				FlakShellDeaths = s.Any(a => a.FlakShellDeaths.HasValue) ? s.Sum(sm => sm.FlakShellDeaths) : null,
				FlakShots = s.Any(a => a.FlakShots.HasValue) ? s.Sum(sm => sm.FlakShots) : null,
				FlakHits = s.Any(a => a.FlakHits.HasValue) ? s.Sum(sm => sm.FlakHits) : null,
				RocketKills = s.Any(a => a.RocketKills.HasValue) ? s.Sum(sm => sm.RocketKills) : null,
				RocketDeaths = s.Any(a => a.RocketDeaths.HasValue) ? s.Sum(sm => sm.RocketDeaths) : null,
				RocketShots = s.Any(a => a.RocketShots.HasValue) ? s.Sum(sm => sm.RocketShots) : null,
				RocketHits = s.Any(a => a.RocketHits.HasValue) ? s.Sum(sm => sm.RocketHits) : null,
				SniperKills = s.Any(a => a.SniperKills.HasValue) ? s.Sum(sm => sm.SniperKills) : null,
				SniperDeaths = s.Any(a => a.SniperDeaths.HasValue) ? s.Sum(sm => sm.SniperDeaths) : null,
				SniperHeadshotKills = s.Any(a => a.SniperHeadshotKills.HasValue) ? s.Sum(sm => sm.SniperHeadshotKills) : null,
				SniperHeadshotDeaths = s.Any(a => a.SniperHeadshotDeaths.HasValue) ? s.Sum(sm => sm.SniperHeadshotDeaths) : null,
				SniperShots = s.Any(a => a.SniperShots.HasValue) ? s.Sum(sm => sm.SniperShots) : null,
				SniperHits = s.Any(a => a.SniperHits.HasValue) ? s.Sum(sm => sm.SniperHits) : null,
				LightningRiflePrimaryKills = s.Any(a => a.LightningRiflePrimaryKills.HasValue) ? s.Sum(sm => sm.LightningRiflePrimaryKills) : null,
				LightningRiflePrimaryDeaths = s.Any(a => a.LightningRiflePrimaryDeaths.HasValue) ? s.Sum(sm => sm.LightningRiflePrimaryDeaths) : null,
				LightningRifleSecondaryKills = s.Any(a => a.LightningRifleSecondaryKills.HasValue) ? s.Sum(sm => sm.LightningRifleSecondaryKills) : null,
				LightningRifleSecondaryDeaths = s.Any(a => a.LightningRifleSecondaryDeaths.HasValue) ? s.Sum(sm => sm.LightningRifleSecondaryDeaths) : null,
				LightningRifleShots = s.Any(a => a.LightningRifleShots.HasValue) ? s.Sum(sm => sm.LightningRifleShots) : null,
				LightningRifleHits = s.Any(a => a.LightningRifleHits.HasValue) ? s.Sum(sm => sm.LightningRifleHits) : null,
				RedeemerKills = s.Any(a => a.RedeemerKills.HasValue) ? s.Sum(sm => sm.RedeemerKills) : null,
				RedeemerDeaths = s.Any(a => a.RedeemerDeaths.HasValue) ? s.Sum(sm => sm.RedeemerDeaths) : null,
				RedeemerShots = s.Any(a => a.RedeemerShots.HasValue) ? s.Sum(sm => sm.RedeemerShots) : null,
				RedeemerHits = s.Any(a => a.RedeemerHits.HasValue) ? s.Sum(sm => sm.RedeemerHits) : null,
				InstagibKills = s.Any(a => a.InstagibKills.HasValue) ? s.Sum(sm => sm.InstagibKills) : null,
				InstagibDeaths = s.Any(a => a.InstagibDeaths.HasValue) ? s.Sum(sm => sm.InstagibDeaths) : null,
				InstagibShots = s.Any(a => a.InstagibShots.HasValue) ? s.Sum(sm => sm.InstagibShots) : null,
				InstagibHits = s.Any(a => a.InstagibHits.HasValue) ? s.Sum(sm => sm.InstagibHits) : null,
				TelefragKills = s.Any(a => a.TelefragKills.HasValue) ? s.Sum(sm => sm.TelefragKills) : null,
				TelefragDeaths = s.Any(a => a.TelefragDeaths.HasValue) ? s.Sum(sm => sm.TelefragDeaths) : null,
				RunDist = s.Any(a => a.RunDist.HasValue) ? s.Sum(sm => sm.RunDist) : null,
				SprintDist = s.Any(a => a.SprintDist.HasValue) ? s.Sum(sm => sm.SprintDist) : null,
				InAirDist = s.Any(a => a.InAirDist.HasValue) ? s.Sum(sm => sm.InAirDist) : null,
				SwimDist = s.Any(a => a.SwimDist.HasValue) ? s.Sum(sm => sm.SwimDist) : null,
				TranslocDist = s.Any(a => a.TranslocDist.HasValue) ? s.Sum(sm => sm.TranslocDist) : null,
				NumDodges = s.Any(a => a.NumDodges.HasValue) ? s.Sum(sm => sm.NumDodges) : null,
				NumWallDodges = s.Any(a => a.NumWallDodges.HasValue) ? s.Sum(sm => sm.NumWallDodges) : null,
				NumJumps = s.Any(a => a.NumJumps.HasValue) ? s.Sum(sm => sm.NumJumps) : null,
				NumLiftJumps = s.Any(a => a.NumLiftJumps.HasValue) ? s.Sum(sm => sm.NumLiftJumps) : null,
				NumFloorSlides = s.Any(a => a.NumFloorSlides.HasValue) ? s.Sum(sm => sm.NumFloorSlides) : null,
				NumWallRuns = s.Any(a => a.NumWallRuns.HasValue) ? s.Sum(sm => sm.NumWallRuns) : null,
				NumImpactJumps = s.Any(a => a.NumImpactJumps.HasValue) ? s.Sum(sm => sm.NumImpactJumps) : null,
				NumRocketJumps = s.Any(a => a.NumRocketJumps.HasValue) ? s.Sum(sm => sm.NumRocketJumps) : null,
				SlideDist = s.Any(a => a.SlideDist.HasValue) ? s.Sum(sm => sm.SlideDist) : null,
				WallRunDist = s.Any(a => a.WallRunDist.HasValue) ? s.Sum(sm => sm.WallRunDist) : null,
				FlagCaptures = s.Any(a => a.FlagCaptures.HasValue) ? s.Sum(sm => sm.FlagCaptures) : null,
				FlagReturns = s.Any(a => a.FlagReturns.HasValue) ? s.Sum(sm => sm.FlagReturns) : null,
				FlagAssists = s.Any(a => a.FlagAssists.HasValue) ? s.Sum(sm => sm.FlagAssists) : null,
				FlagHeldDeny = s.Any(a => a.FlagHeldDeny.HasValue) ? s.Sum(sm => sm.FlagHeldDeny) : null,
				FlagHeldDenyTime = s.Any(a => a.FlagHeldDenyTime.HasValue) ? s.Sum(sm => sm.FlagHeldDenyTime) : null,
				FlagHeldTime = s.Any(a => a.FlagHeldTime.HasValue) ? s.Sum(sm => sm.FlagHeldTime) : null,
				FlagReturnPoints = s.Any(a => a.FlagReturnPoints.HasValue) ? s.Sum(sm => sm.FlagReturnPoints) : null,
				CarryAssist = s.Any(a => a.CarryAssist.HasValue) ? s.Sum(sm => sm.CarryAssist) : null,
				CarryAssistPoints = s.Any(a => a.CarryAssistPoints.HasValue) ? s.Sum(sm => sm.CarryAssistPoints) : null,
				FlagCapPoints = s.Any(a => a.FlagCapPoints.HasValue) ? s.Sum(sm => sm.FlagCapPoints) : null,
				DefendAssist = s.Any(a => a.DefendAssist.HasValue) ? s.Sum(sm => sm.DefendAssist) : null,
				DefendAssistPoints = s.Any(a => a.DefendAssistPoints.HasValue) ? s.Sum(sm => sm.DefendAssistPoints) : null,
				ReturnAssist = s.Any(a => a.ReturnAssist.HasValue) ? s.Sum(sm => sm.ReturnAssist) : null,
				ReturnAssistPoints = s.Any(a => a.ReturnAssistPoints.HasValue) ? s.Sum(sm => sm.ReturnAssistPoints) : null,
				TeamCapPoints = s.Any(a => a.TeamCapPoints.HasValue) ? s.Sum(sm => sm.TeamCapPoints) : null,
				EnemyFCDamage = s.Any(a => a.EnemyFCDamage.HasValue) ? s.Sum(sm => sm.EnemyFCDamage) : null,
				FCKills = s.Any(a => a.FCKills.HasValue) ? s.Sum(sm => sm.FCKills) : null,
				FCKillPoints = s.Any(a => a.FCKillPoints.HasValue) ? s.Sum(sm => sm.FCKillPoints) : null,
				FlagSupportKills = s.Any(a => a.FlagSupportKills.HasValue) ? s.Sum(sm => sm.FlagSupportKills) : null,
				FlagSupportKillPoints = s.Any(a => a.FlagSupportKillPoints.HasValue) ? s.Sum(sm => sm.FlagSupportKillPoints) : null,
				RegularKillPoints = s.Any(a => a.RegularKillPoints.HasValue) ? s.Sum(sm => sm.RegularKillPoints) : null,
				FlagGrabs = s.Any(a => a.FlagGrabs.HasValue) ? s.Sum(sm => sm.FlagGrabs) : null,
				AttackerScore = s.Any(a => a.AttackerScore.HasValue) ? s.Sum(sm => sm.AttackerScore) : null,
				DefenderScore = s.Any(a => a.DefenderScore.HasValue) ? s.Sum(sm => sm.DefenderScore) : null,
				SupporterScore = s.Any(a => a.SupporterScore.HasValue) ? s.Sum(sm => sm.SupporterScore) : null,
				TeamKills = s.Any(a => a.TeamKills.HasValue) ? s.Sum(sm => sm.TeamKills) : null,
			})
			.First();

		return merged;
	}

	#endregion

	/// <summary>
	/// This method will delete statistics that are older than X <paramref name="days"/>
	/// </summary>
	/// <param name="days">Number of days that should be kept for statistic records. It shouldn't be less than 30 since they are used for monthly statistics calculation.</param>
	/// <param name="skipFlagged">When set to <see langword="false" /> it will also delete flagged statistics. Flagged statistics should be deleted only after they are inspected.</param>
	/// <returns>Number of deleted statistic records</returns>
	public async Task<long> DeleteOldStatisticsAsync(int days = 30, bool skipFlagged = true)
	{
		logger.LogInformation("Deleting statistics that are older than: {Days}, {Flagged}.", days, skipFlagged);

		if (days < MinimumDaysForDeletingOldStatistics)
		{
			throw new ArgumentException($"You can delete only statistics that are at least {MinimumDaysForDeletingOldStatistics} days old.", nameof(days));
		}

		var removeBeforeDate = DateTime.UtcNow.Date.AddDays(-days);

		var filter = Builders<Statistic>.Filter.Lt(f => f.CreatedAt, removeBeforeDate) &
					 Builders<Statistic>.Filter.In(f => f.Window, new List<StatisticWindow>() { StatisticWindow.Daily });

		if (skipFlagged)
		{
			filter &= Builders<Statistic>.Filter.Exists(f => f.Flagged, false);
		}

		var result = await statisticsCollection.DeleteManyAsync(filter);

		logger.LogInformation("Deleted {Count} statistics successfully.", result.DeletedCount);

		return result.DeletedCount;
	}

	public async Task<long?> RemoveStatisticsByAccountAsync(EpicID accountID)
	{
		var result = await statisticsCollection.DeleteManyAsync(x => x.AccountID == accountID);
		if (!result.IsAcknowledged)
			return null;

		return result.DeletedCount;
	}
}
