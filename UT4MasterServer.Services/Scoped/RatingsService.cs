using MongoDB.Driver;
using UT4MasterServer.Common;
using UT4MasterServer.Models;
using UT4MasterServer.Models.DTO.Response;
using UT4MasterServer.Models.DTO.Responses;

namespace UT4MasterServer.Services;

public sealed class RatingsService
{
	private readonly IMongoCollection<Rating> ratingsCollection;

	public RatingsService(DatabaseContext dbContext)
	{
		ratingsCollection = dbContext.Database.GetCollection<Rating>("ratings");
	}

	public async Task<MMRBulkResponse> GetRatingsAsync(EpicID accountID, MMRBulkResponse mmrBulk)
	{
		var filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID);
		var ratings = await ratingsCollection.Find(filter).FirstOrDefaultAsync();

		var result = new MMRBulkResponse();

		foreach (var ratingType in mmrBulk.RatingTypes)
		{

			switch (ratingType)
			{
				case "SkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.SkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.SkillRatingGamesPlayed);
					break;

				case "TDMSkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.TDMSkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.TDMSkillRatingGamesPlayed);
					break;

				case "CTFSkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.CTFSkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.CTFSkillRatingGamesPlayed);
					break;

				case "ShowdownSkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.ShowdownSkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.ShowdownSkillRatingGamesPlayed);
					break;

				case "FlagRunSkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.FlagRunSkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.FlagRunSkillRatingGamesPlayed);
					break;

				case "DMSkillRating":
					result.RatingTypes.Add(ratingType);
					result.Ratings.Add(ratings.DMSkillRating / Rating.Precision);
					result.NumGamesPlayed.Add(ratings.DMSkillRatingGamesPlayed);
					break;
			}
		}

		return result;
	}

	public async Task<MMRRatingResponse> GetRatingAsync(EpicID accountID, string ratingType)
	{
		var filter = Builders<Rating>.Filter.Eq(f => f.AccountID, accountID);
		var ratings = await ratingsCollection.Find(filter).FirstOrDefaultAsync();

		MMRRatingResponse result = new();

		switch (ratingType)
		{
			case "SkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.SkillRating / Rating.Precision,
					GamesPlayed = ratings.SkillRatingGamesPlayed,
				};
				break;

			case "TDMSkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.TDMSkillRating / Rating.Precision,
					GamesPlayed = ratings.TDMSkillRatingGamesPlayed,
				};
				break;

			case "CTFSkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.CTFSkillRating / Rating.Precision,
					GamesPlayed = ratings.CTFSkillRatingGamesPlayed,
				};
				break;

			case "ShowdownSkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.ShowdownSkillRating / Rating.Precision,
					GamesPlayed = ratings.ShowdownSkillRatingGamesPlayed,
				};
				break;

			case "FlagRunSkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.FlagRunSkillRating / Rating.Precision,
					GamesPlayed = ratings.FlagRunSkillRatingGamesPlayed,
				};
				break;

			case "DMSkillRating":
				result = new MMRRatingResponse
				{
					Rating = ratings.DMSkillRating / Rating.Precision,
					GamesPlayed = ratings.DMSkillRatingGamesPlayed,
				};
				break;
		}

		return result;
	}

	public async Task UpdateTeamsRatingsAsync(RatingMatch ratingMatch, Func<Rating, int> propertySelector)
	{
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

		var redTeamAccountIds = ratingMatch.RedTeam.Members
			.Where(w => !w.IsBot)
			.Select(s => EpicID.FromString(s.AccountID))
			.ToArray();
		var blueTeamAccountIds = ratingMatch.BlueTeam.Members
			.Where(w => !w.IsBot)
			.Select(s => EpicID.FromString(s.AccountID))
			.ToArray();

		var filter = Builders<Rating>.Filter.In(f => f.AccountID, redTeamAccountIds.Union(blueTeamAccountIds));
		var playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

		int redTeamPlayersCount = redTeamAccountIds.Length;
		int blueTeamPlayersCount = blueTeamAccountIds.Length;
		double[] redTeamCurrentRatings = new double[redTeamPlayersCount];
		double[] blueTeamCurrentRatings = new double[blueTeamPlayersCount];

		for (int i = 0; i < redTeamPlayersCount; i++)
		{
			redTeamCurrentRatings[i] = playersCurrentRatings
				.Where(w => w.AccountID == redTeamAccountIds[i])
				.Select(propertySelector)
				.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
		}

		for (int i = 0; i < blueTeamPlayersCount; i++)
		{
			blueTeamCurrentRatings[i] = playersCurrentRatings
				.Where(w => w.AccountID == blueTeamAccountIds[i])
				.Select(propertySelector)
				.FirstOrDefault(Rating.DefaultRating * Rating.Precision) / Rating.Precision;
		}

		double[] redTeamExpectedScores = EloTeamsCalculationService.GetExpectedScores(redTeamCurrentRatings, blueTeamCurrentRatings);
		double[] blueTeamExpectedScores = EloTeamsCalculationService.GetExpectedScores(blueTeamCurrentRatings, redTeamCurrentRatings);

		double[] redTeamNewRatings = EloTeamsCalculationService.GetNewRatings(redTeamCurrentRatings, redTeamExpectedScores, redTeamActualScore);
		double[] blueTeamNewRatings = EloTeamsCalculationService.GetNewRatings(blueTeamCurrentRatings, blueTeamExpectedScores, blueTeamActualScore);

		var bulkWriteModelList = new List<UpdateOneModel<Rating>>();

		FieldDefinition<Rating, int> setFieldDefinition = ratingMatch.RatingType;
		FieldDefinition<Rating, int> incFieldDefinition = ratingMatch.RatingType + "GamesPlayed";

		for (int i = 0; i < redTeamAccountIds.Length; i++)
		{
			var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, redTeamAccountIds[i]);
			var updateDefinition = Builders<Rating>.Update
				.Set(setFieldDefinition, (int)(redTeamNewRatings[i] * Rating.Precision))
				.Inc(incFieldDefinition, 1);
			bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
		}

		for (int i = 0; i < blueTeamAccountIds.Length; i++)
		{
			var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, blueTeamAccountIds[i]);
			var updateDefinition = Builders<Rating>.Update
				.Set(setFieldDefinition, (int)(blueTeamNewRatings[i] * Rating.Precision))
				.Inc(incFieldDefinition, 1);
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

		var filter = Builders<Rating>.Filter.In(f => f.AccountID, playersAccountIds);

		var playersCurrentRatings = await ratingsCollection.Find(filter).ToListAsync();

		double[] currentRatings = new double[playersCount];

		for (int i = 0; i < playersCount; i++)
		{
			currentRatings[i] = playersCurrentRatings.FirstOrDefault(f => f.AccountID == playersAccountIds[i])?.DMSkillRating / Rating.Precision ?? Rating.DefaultRating;
		}

		double[] expectedScores = EloDeathmatchCalculationService.GetExpectedScores(currentRatings);
		double[] relativeScores = EloDeathmatchCalculationService.GetLineralyDistributedRelativeScores(playersCount);
		double[] newRatings = EloDeathmatchCalculationService.GetNewRatings(currentRatings, expectedScores, relativeScores);

		var bulkWriteModelList = new List<UpdateOneModel<Rating>>();

		for (int i = 0; i < playersCount; i++)
		{
			var updateFilter = Builders<Rating>.Filter.Eq(f => f.AccountID, playersAccountIds[i]);
			var updateDefinition = Builders<Rating>.Update
				.Set(s => s.DMSkillRating, (int)(newRatings[i] * Rating.Precision))
				.Inc(i => i.DMSkillRatingGamesPlayed, 1);
			bulkWriteModelList.Add(new UpdateOneModel<Rating>(updateFilter, updateDefinition) { IsUpsert = true });
		}

		await ratingsCollection.BulkWriteAsync(bulkWriteModelList);
	}
}
