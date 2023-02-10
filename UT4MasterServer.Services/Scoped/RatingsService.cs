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
		var indexAccountId = new IndexKeysDefinitionBuilder<Rating>().Ascending(f => f.AccountID);
		var indexRatingType = new IndexKeysDefinitionBuilder<Rating>().Ascending(f => f.RatingType);
		var indexesCombined = new IndexKeysDefinitionBuilder<Rating>().Combine(new[] { indexAccountId, indexRatingType });

		var createIndexModel = new CreateIndexModel<Rating>(indexesCombined, new CreateIndexOptions() { Unique = true });
		await ratingsCollection.Indexes.CreateOneAsync(createIndexModel);
	}

	public async Task<MMRBulkResponse> GetRatingsAsync(EpicID accountID, MMRBulkRequest mmrBulk)
	{
		var ratingTypes = mmrBulk.RatingTypes.Intersect(Rating.AllowedRatingTypes);
		var filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Rating>.Filter.In(f => f.RatingType, ratingTypes);
		var ratings = await ratingsCollection.Find(filter).ToListAsync();

		var result = new MMRBulkResponse();

		foreach (var ratingType in mmrBulk.RatingTypes)
		{
			var rating = ratings.FirstOrDefault(f => f.RatingType == ratingType);

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
		var filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID) &
					 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType);
		var rating = await ratingsCollection.Find(filter).FirstOrDefaultAsync();

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
			var accountIds = ratingTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			var filter = Builders<Rating>.Filter.In(f => f.AccountID, accountIds) &
						 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType);
			var ratings = await ratingsCollection.Find(filter).ToListAsync();

			List<int> ratingValues = new();

			foreach (var member in ratingTeam.Members.Where(w => !w.IsBot))
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
		var filter = Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType) &
					 Builders<Rating>.Filter.Gte(f => f.GamesPlayed, 10);
		var sort = Builders<Rating>.Sort.Descending(s => s.RatingValue).Descending(s => s.GamesPlayed);
		var ratingsCount = await ratingsCollection.Find(filter).CountDocumentsAsync();
		var ratings = await ratingsCollection.Find(filter)
			.Sort(sort)
			.ToListAsync();

		var selectedRating = ratings.FirstOrDefault(r => r.AccountID == accountID);

		if(selectedRating == null)
		{
			return null;
		}

		var rank = ratings.IndexOf(selectedRating) + 1;

		var filterAccount = Builders<Account>.Filter.Eq(f => f.ID, accountID);
		var accountCursor = await accountsCollection.FindAsync(filterAccount);
		var account = await accountCursor.SingleOrDefaultAsync();

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
		var filter = Builders<Rating>.Filter.Eq(f => f.RatingType, ratingType) &
					 Builders<Rating>.Filter.Gte(f => f.GamesPlayed, 10);
		var sort = Builders<Rating>.Sort.Descending(s => s.RatingValue).Descending(s => s.GamesPlayed);
		var ratingsCount = await ratingsCollection.Find(filter).CountDocumentsAsync();
		var ratings = await ratingsCollection.Find(filter)
			.Sort(sort)
			.Skip(skip)
			.Limit(limit)
			.ToListAsync();

		var accountIds = ratings.Select(s => s.AccountID);
		var filterAccounts = Builders<Account>.Filter.In(f => f.ID, accountIds);
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
			var redTeamAccountIds = ratingMatch.RedTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			var blueTeamAccountIds = ratingMatch.BlueTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			int redTeamPlayersCount = redTeamAccountIds.Length;
			int blueTeamPlayersCount = blueTeamAccountIds.Length;
			if (redTeamPlayersCount < 1 || blueTeamPlayersCount < 1) return;

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

			var filter = Builders<Rating>.Filter.In(f => f.AccountID, redTeamAccountIds.Union(blueTeamAccountIds)) &
						 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
			var playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

			double[] redTeamCurrentRatings = new double[redTeamPlayersCount];
			double[] blueTeamCurrentRatings = new double[blueTeamPlayersCount];

			for (int i = 0; i < redTeamPlayersCount; i++)
			{
				redTeamCurrentRatings[i] = playersCurrentRatings
					.Where(w => w.AccountID == redTeamAccountIds[i] &&
								w.RatingType == ratingMatch.RatingType)
					.Select(s => s.RatingValue)
					.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
			}

			for (int i = 0; i < blueTeamPlayersCount; i++)
			{
				blueTeamCurrentRatings[i] = playersCurrentRatings
					.Where(w => w.AccountID == blueTeamAccountIds[i] &&
								w.RatingType == ratingMatch.RatingType)
					.Select(s => s.RatingValue)
					.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
			}

			double[] redTeamExpectedScores = EloTeamsCalculationHelper.GetExpectedScores(redTeamCurrentRatings, blueTeamCurrentRatings);
			double[] blueTeamExpectedScores = EloTeamsCalculationHelper.GetExpectedScores(blueTeamCurrentRatings, redTeamCurrentRatings);

			double[] redTeamNewRatings = EloTeamsCalculationHelper.GetNewRatings(redTeamCurrentRatings, redTeamExpectedScores, redTeamActualScore);
			double[] blueTeamNewRatings = EloTeamsCalculationHelper.GetNewRatings(blueTeamCurrentRatings, blueTeamExpectedScores, blueTeamActualScore);

			var bulkWriteModelList = new List<UpdateOneModel<Rating>>();

			FieldDefinition<Rating, int> setFieldDefinition = ratingMatch.RatingType;
			FieldDefinition<Rating, int> incFieldDefinition = ratingMatch.RatingType + "GamesPlayed";

			for (int i = 0; i < redTeamAccountIds.Length; i++)
			{
				var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, redTeamAccountIds[i]) &
								   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				var updateDefinition = Builders<Rating>.Update
					.Set(s => s.RatingType, ratingMatch.RatingType)
					.Set(s => s.RatingValue, (int)(redTeamNewRatings[i] * Rating.Precision))
					.Inc(i => i.GamesPlayed, 1);
				bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
			}

			for (int i = 0; i < blueTeamAccountIds.Length; i++)
			{
				var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, blueTeamAccountIds[i]) &
								   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				var updateDefinition = Builders<Rating>.Update
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
			var playersAccountIds = ratingMatch.RedTeam.Members
				.Where(w => !w.IsBot && !string.IsNullOrWhiteSpace(w.AccountID))
				.OrderByDescending(o => o.Score)
				.Select(s => EpicID.FromString(s.AccountID))
				.ToArray();
			int playersCount = playersAccountIds.Length;
			if (playersCount < 2) return;

			var filter = Builders<Rating>.Filter.In(f => f.AccountID, playersAccountIds) &
						 Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
			var playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

			double[] currentRatings = new double[playersCount];
			for (int i = 0; i < playersCount; i++)
			{
				currentRatings[i] = playersCurrentRatings.FirstOrDefault(f => f.AccountID == playersAccountIds[i])?.RatingValue / Rating.Precision ?? Rating.DefaultRating;
			}

			double[] expectedScores = EloDeathmatchCalculationHelper.GetExpectedScores(currentRatings);
			double[] relativeScores = EloDeathmatchCalculationHelper.GetLineralyDistributedRelativeScores(playersCount);
			double[] newRatings = EloDeathmatchCalculationHelper.GetNewRatings(currentRatings, expectedScores, relativeScores);

			var bulkWriteModelList = new List<UpdateOneModel<Rating>>();
			for (int i = 0; i < playersCount; i++)
			{
				var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, playersAccountIds[i]) &
								   Builders<Rating>.Filter.Eq(f => f.RatingType, ratingMatch.RatingType);
				var updateDefinition = Builders<Rating>.Update
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
