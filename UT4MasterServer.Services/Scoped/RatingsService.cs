using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Request;
using UT4MasterServer.Models.DTO.Response;

namespace UT4MasterServer.Services.Scoped;

public sealed class RatingsService
{
	private readonly IMongoCollection<Rating> ratingsCollection;

	public RatingsService(DatabaseContext dbContext)
	{
		ratingsCollection = dbContext.Database.GetCollection<Rating>("ratings");
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
		var accountIds = ratingTeam.Members
			.Where(w => !w.IsBot)
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

	public async Task UpdateTeamsRatingsAsync(RatingMatch ratingMatch)
	{
		var redTeamAccountIds = ratingMatch.RedTeam.Members
			.Where(w => !w.IsBot)
			.Select(s => EpicID.FromString(s.AccountID))
			.ToArray();
		var blueTeamAccountIds = ratingMatch.BlueTeam.Members
			.Where(w => !w.IsBot)
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

	public async Task UpdateDeathmatchRatingsAsync(RatingMatch ratingMatch)
	{
		var playersAccountIds = ratingMatch.RedTeam.Members
			.Where(w => !w.IsBot)
			.OrderByDescending(o => o.Score)
			.Select(s => EpicID.FromString(s.AccountID))
			.ToArray();
		int playersCount = playersAccountIds.Length;

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
}
