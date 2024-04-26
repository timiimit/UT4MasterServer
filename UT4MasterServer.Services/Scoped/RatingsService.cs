using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Requests;
using UT4MasterServer.Models.DTO.Responses;

namespace UT4MasterServer.Services.Scoped;

public sealed class RatingsService
{
	private readonly ILogger<RatingsService> logger;
	private readonly IMongoCollection<Rating> ratingsCollection;
	private readonly IMongoCollection<Account> accountsCollection;

	private const string UnknownUser = "Unknown user";
	private const string DefaultCountryFlag = "Unreal";

	public RatingsService(ILogger<RatingsService> logger, DatabaseContext dbContext)
	{
		this.logger = logger;
		ratingsCollection = dbContext.Database.GetCollection<Rating>("ratings");
		accountsCollection = dbContext.Database.GetCollection<Account>("accounts");
	}

	public async Task CreateIndexesAsync()
	{
		IndexKeysDefinition<Rating>? indexAccountId = new IndexKeysDefinitionBuilder<Rating>().Ascending(f => f.AccountID);
		IndexKeysDefinition<Rating>? indexRatingType = new IndexKeysDefinitionBuilder<Rating>().Ascending(f => f.RatingType);
		IndexKeysDefinition<Rating>? indexesCombined = new IndexKeysDefinitionBuilder<Rating>().Combine(new[] { indexAccountId, indexRatingType });

		var createIndexModel = new CreateIndexModel<Rating>(indexesCombined, new CreateIndexOptions() { Unique = true });
		await ratingsCollection.Indexes.CreateOneAsync(createIndexModel);
	}

	public async Task<MMRBulkResponse> GetRatingsAsync(EpicID accountID, MMRBulkRequest mmrBulk)
	{
		IEnumerable<string>? ratingTypes = mmrBulk.RatingTypes.Intersect(Rating.AllowedRatingTypes);
		FilterDefinition<Rating>? filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID) &
										   Builders<Rating>.Filter.In(f => f.RatingType, ratingTypes);
		List<Rating>? ratings = await ratingsCollection.Find(filter).ToListAsync();

		var result = new MMRBulkResponse();

		foreach (var ratingType in mmrBulk.RatingTypes)
		{
			Rating? rating = ratings.FirstOrDefault(f => f.RatingType == ratingType);

			result.RatingTypes.Add(ratingType);
			if (rating is not null)
			{
				result.Ratings.Add(rating.RatingValue / Rating.Precision);
				result.NumGamesPlayed.Add(rating.GamesPlayed);
			}
			else
			{
				result.Ratings.Add(Rating.DefaultRating);
				result.NumGamesPlayed.Add(0);
			}
		}

		return result;
	}

	public async Task<MMRRatingResponse> GetRatingAsync(EpicID accountID, string ratingType)
	{
		FilterDefinition<Rating>? filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID) &
										   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType);
		Rating? rating = await ratingsCollection.Find(filter).FirstOrDefaultAsync();

		var result = new MMRRatingResponse()
		{
			Rating = Rating.DefaultRating,
			GamesPlayed = 0,
		};

		if (rating is not null)
		{
			result.Rating = rating.RatingValue / Rating.Precision;
			result.GamesPlayed = rating.GamesPlayed;
		}

		return result;
	}

	public async Task<RatingResponse> GetAverageTeamRatingAsync(string ratingType, RatingTeam ratingTeam)
	{
		try
		{
			EpicID[]? accountIds = ratingTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			FilterDefinition<Rating>? filter = Builders<Rating>.Filter.In(f => f.AccountID, accountIds) &
											   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType);
			List<Rating>? ratings = await ratingsCollection.Find(filter).ToListAsync();

			List<int> ratingValues = new();

			foreach (RatingTeam.Member? member in ratingTeam.Members.Where(w => !w.IsBot))
			{
				var ratingValue = ratings
					.Where(w => w.AccountID == EpicID.FromString(member.AccountID))
					.FirstOrDefault()?.RatingValue / Rating.Precision ?? Rating.DefaultRating;
				ratingValues.Add(ratingValue);
			}

			return new RatingResponse()
			{
				RatingValue = ratingValues.Any() ? (int)ratingValues.Average() : Rating.DefaultRating
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{MethodName} | {RatingType} | {JSON}", nameof(GetAverageTeamRatingAsync), ratingType, JsonSerializer.Serialize(ratingTeam));
			throw;
		}
	}

	public async Task<RankingsResponse?> GetSelectedRankingAsync(string ratingType, EpicID accountID)
	{
		FilterDefinition<Rating>? filter = Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType) &
										   Builders<Rating>.Filter.Gte(f => f.GamesPlayed, 10);
		SortDefinition<Rating>? sort = Builders<Rating>.Sort.Descending(s => s.RatingValue).Descending(s => s.GamesPlayed);
		var ratingsCount = await ratingsCollection.Find(filter).CountDocumentsAsync();
		List<Rating>? ratings = await ratingsCollection.Find(filter)
			.Sort(sort)
			.ToListAsync();

		Rating? selectedRating = ratings.FirstOrDefault(r => r.AccountID == accountID);

		if(selectedRating == null)
		{
			return null;
		}

		var rank = ratings.IndexOf(selectedRating) + 1;

		FilterDefinition<Account>? filterAccount = Builders<Account>.Filter.Eq(f => f.ID, accountID);
		IAsyncCursor<Account>? accountCursor = await accountsCollection.FindAsync(filterAccount);
		Account? account = await accountCursor.SingleOrDefaultAsync();

		return new RankingsResponse()
		{
			Rank = rank,
			AccountID = account.ID,
			Player = account.Username,
			CountryFlag = account.CountryFlag,
			Rating = selectedRating.RatingValue / Rating.Precision,
			GamesPlayed = selectedRating.GamesPlayed
		};
	}

	public async Task<PagedResponse<RankingsResponse>> GetRankingsAsync(string ratingType, int skip, int limit)
	{
		FilterDefinition<Rating>? filter = Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType) &
										   Builders<Rating>.Filter.Gte(f => f.GamesPlayed, 10);
		SortDefinition<Rating>? sort = Builders<Rating>.Sort.Descending(s => s.RatingValue).Descending(s => s.GamesPlayed);
		var ratingsCount = await ratingsCollection.Find(filter).CountDocumentsAsync();
		List<Rating>? ratings = await ratingsCollection.Find(filter)
			.Sort(sort)
			.Skip(skip)
			.Limit(limit)
			.ToListAsync();

		IEnumerable<EpicID>? accountIds = ratings.Select(s => s.AccountID);
		FilterDefinition<Account>? filterAccounts = Builders<Account>.Filter.In(f => f.ID, accountIds);
		var accounts = await accountsCollection
			.Find(filterAccounts)
			.Project(p => new { p.ID, p.Username, p.CountryFlag })
			.ToListAsync();

		var rank = skip;
		var rankings = ratings
			.Select(s =>
			{
				var userName = UnknownUser;
				var countryFlag = DefaultCountryFlag;
				if (accounts.FirstOrDefault(f => f.ID == s.AccountID) is { } account)
				{
					userName = account.Username;
					countryFlag = account.CountryFlag;
				}

				return new RankingsResponse()
				{
					Rank = ++rank,
					AccountID = s.AccountID,
					Player = userName,
					CountryFlag = countryFlag,
					Rating = s.RatingValue / Rating.Precision,
					GamesPlayed = s.GamesPlayed,
				};
			})
			.ToList();

		var response = new PagedResponse<RankingsResponse>()
		{
			Count = ratingsCount,
			Data = rankings,
		};

		return response;
	}

	public async Task UpdateTeamsRatingsAsync(RatingMatch ratingMatch)
	{
		try
		{
			EpicID[]? redTeamAccountIds = ratingMatch.RedTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			EpicID[]? blueTeamAccountIds = ratingMatch.BlueTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			var redTeamPlayersCount = redTeamAccountIds.Length;
			var blueTeamPlayersCount = blueTeamAccountIds.Length;
			if (redTeamPlayersCount < 1 || blueTeamPlayersCount < 1)
			{
				return;
			}

			double redTeamActualScore = 1;
			double blueTeamActualScore = 0;
			switch (ratingMatch.MatchInfo.RedScore)
			{
				// Red team loses
				case 0:
					redTeamActualScore = 0;
					blueTeamActualScore = 1;
					break;

				// Red team wins
				case 1:
					redTeamActualScore = 1;
					blueTeamActualScore = 0;
					break;

				// Draw
				default:
					redTeamActualScore = 0.5;
					blueTeamActualScore = 0.5;
					break;
			}

			FilterDefinition<Rating>? filter = Builders<Rating>.Filter.In(f => f.AccountID, redTeamAccountIds.Union(blueTeamAccountIds)) &
											   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
			List<Rating>? playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

			var redTeamCurrentRatings = new double[redTeamPlayersCount];
			var blueTeamCurrentRatings = new double[blueTeamPlayersCount];

			for (var i = 0; i < redTeamPlayersCount; i++)
			{
				redTeamCurrentRatings[i] = playersCurrentRatings
					.Where(w => w.AccountID == redTeamAccountIds[i] &&
								w.RatingType == ratingMatch.RatingType)
					.Select(s => s.RatingValue)
					.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
			}

			for (var i = 0; i < blueTeamPlayersCount; i++)
			{
				blueTeamCurrentRatings[i] = playersCurrentRatings
					.Where(w => w.AccountID == blueTeamAccountIds[i] &&
								w.RatingType == ratingMatch.RatingType)
					.Select(s => s.RatingValue)
					.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
			}

			var redTeamExpectedScores = EloTeamsCalculationHelper.GetExpectedScores(redTeamCurrentRatings, blueTeamCurrentRatings);
			var blueTeamExpectedScores = EloTeamsCalculationHelper.GetExpectedScores(blueTeamCurrentRatings, redTeamCurrentRatings);

			var redTeamNewRatings = EloTeamsCalculationHelper.GetNewRatings(redTeamCurrentRatings, redTeamExpectedScores, redTeamActualScore);
			var blueTeamNewRatings = EloTeamsCalculationHelper.GetNewRatings(blueTeamCurrentRatings, blueTeamExpectedScores, blueTeamActualScore);

			var bulkWriteModelList = new List<UpdateOneModel<Rating>>();

			FieldDefinition<Rating, int> setFieldDefinition = ratingMatch.RatingType;
			FieldDefinition<Rating, int> incFieldDefinition = ratingMatch.RatingType + "GamesPlayed";

			for (var i = 0; i < redTeamAccountIds.Length; i++)
			{
				FilterDefinition<Rating>? updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, redTeamAccountIds[i]) &
														 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				UpdateDefinition<Rating>? updateDefinition = Builders<Rating>.Update
					.Set(s => s.RatingType, ratingMatch.RatingType)
					.Set(s => s.RatingValue, (int)(redTeamNewRatings[i] * Rating.Precision))
					.Inc(i => i.GamesPlayed, 1);
				bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
			}

			for (var i = 0; i < blueTeamAccountIds.Length; i++)
			{
				FilterDefinition<Rating>? updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, blueTeamAccountIds[i]) &
														 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				UpdateDefinition<Rating>? updateDefinition = Builders<Rating>.Update
					.Set(s => s.RatingType, ratingMatch.RatingType)
					.Set(s => s.RatingValue, (int)(blueTeamNewRatings[i] * Rating.Precision))
					.Inc(i => i.GamesPlayed, 1);
				bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
			}

			await ratingsCollection.BulkWriteAsync(bulkWriteModelList);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{MethodName} | {JSON}", nameof(UpdateTeamsRatingsAsync), JsonSerializer.Serialize(ratingMatch));
			throw;
		}
	}

	public async Task UpdateDeathmatchRatingsAsync(RatingMatch ratingMatch)
	{
		try
		{
			EpicID[]? playersAccountIds = ratingMatch.RedTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.OrderByDescending(o => o.Score)
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			var playersCount = playersAccountIds.Length;
			if (playersCount < 2)
			{
				return;
			}

			FilterDefinition<Rating>? filter = Builders<Rating>.Filter.In(f => f.AccountID, playersAccountIds) &
											   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
			List<Rating>? playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

			var currentRatings = new double[playersCount];
			for (var i = 0; i < playersCount; i++)
			{
				currentRatings[i] = playersCurrentRatings.FirstOrDefault(f => f.AccountID == playersAccountIds[i])?.RatingValue / Rating.Precision ?? Rating.DefaultRating;
			}

			var expectedScores = EloDeathmatchCalculationHelper.GetExpectedScores(currentRatings);
			var relativeScores = EloDeathmatchCalculationHelper.GetLineralyDistributedRelativeScores(playersCount);
			var newRatings = EloDeathmatchCalculationHelper.GetNewRatings(currentRatings, expectedScores, relativeScores);

			var bulkWriteModelList = new List<UpdateOneModel<Rating>>();
			for (var i = 0; i < playersCount; i++)
			{
				FilterDefinition<Rating>? updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, playersAccountIds[i]) &
														 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				UpdateDefinition<Rating>? updateDefinition = Builders<Rating>.Update
					.Set(s => s.RatingType, ratingMatch.RatingType)
					.Set(s => s.RatingValue, (int)(newRatings[i] * Rating.Precision))
					.Inc(i => i.GamesPlayed, 1);
				bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
			}
			await ratingsCollection.BulkWriteAsync(bulkWriteModelList);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "{MethodName} | {JSON}", nameof(UpdateDeathmatchRatingsAsync), JsonSerializer.Serialize(ratingMatch));
			throw;
		}
	}

	public async Task RemoveAllByAccountAsync(EpicID accountID)
	{
		await ratingsCollection.DeleteManyAsync(x => x.AccountID == accountID);
	}
}
