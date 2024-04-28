namespace UT4MasterServer.Services;

/// <summary>
/// This helper is used only for calculating new ratings of players in a team-based mode
/// </summary>
/// <remarks>
/// The standard ELO calculation was adjusted to be more suitable for team-based modes using this article: <see href="https://www.ageofempires.com/news/updates-to-ranked-team-game-elo-calculation/">Updates to Ranked Team Game Elo Calculation</see>
/// </remarks>
public static class EloTeamsCalculationHelper
{
	private const int KRating = 32;
	private const int AverageRatingDifference = 400;

	public static double[] GetExpectedScores(double[] ratingsA, double[] ratingsB)
	{
		var teamASize = ratingsA.Length;
		var ratingsBAverage = ratingsB.Average();
		var expectedScores = new double[teamASize];

		for (var i = 0; i < teamASize; i++)
		{
			expectedScores[i] = 1.0 / (1.0 + Math.Pow(10.0, (ratingsBAverage - ratingsA[i]) / AverageRatingDifference));
		}

		return expectedScores;
	}

	public static double[] GetNewRatings(double[] currentRatings, double[] expectedScores, double actualScore)
	{
		var teamSize = currentRatings.Length;
		var newRatings = new double[teamSize];

		for (var i = 0; i < teamSize; i++)
		{
			newRatings[i] = Math.Round(currentRatings[i] + KRating * (actualScore - expectedScores[i]), 2);
		}

		return newRatings;
	}
}
