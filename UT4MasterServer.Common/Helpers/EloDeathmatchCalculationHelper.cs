namespace UT4MasterServer.Services;

/// <summary>
/// This helper is used only for calculating new ratings of players in the deathmatch mode
/// </summary>
/// <remarks>
/// The standard ELO calculation was adjusted to be more suitable for the deathmatch mode using this article: <see href="https://towardsdatascience.com/developing-a-generalized-elo-rating-system-for-multiplayer-games-b9b495e87802">Developing a Generalized Elo Rating System for Multiplayer Games</see>
/// </remarks>
public static class EloDeathmatchCalculationHelper
{
	private const int KRating = 32;
	private const int AverageRatingDifference = 400;

	public static double[] GetExpectedScores(double[] currentRatings)
	{
		var playersCount = currentRatings.Length;
		var expectedScores = new double[playersCount];

		for (var i = 0; i < playersCount; i++)
		{
			for (var j = 0; j < playersCount; j++)
			{
				if (i == j)
				{
					continue;
				}

				expectedScores[i] += (1.0 / (1.0 + Math.Pow(10.0, (currentRatings[j] - currentRatings[i]) / AverageRatingDifference))) / (playersCount * (playersCount - 1) / 2.0);
			}
		}

		return expectedScores;
	}

	public static double[] GetLineralyDistributedRelativeScores(int playersCount)
	{
		var lineralyDistributedScores = new double[playersCount];

		for (var i = 0; i < playersCount; i++)
		{
			var playersPlace = i + 1;
			lineralyDistributedScores[i] = (playersCount - playersPlace) / (playersCount * (playersCount - 1) / 2.0);
		}

		return lineralyDistributedScores;
	}

	public static double[] GetNewRatings(double[] currentRatings, double[] expectedScores, double[] relativeScores)
	{
		var playersCount = currentRatings.Length;
		var newRatings = new double[playersCount];

		for (var i = 0; i < playersCount; i++)
		{
			newRatings[i] = Math.Round(currentRatings[i] + KRating * (playersCount - 1) * (relativeScores[i] - expectedScores[i]), 2);
		}

		return newRatings;
	}
}
