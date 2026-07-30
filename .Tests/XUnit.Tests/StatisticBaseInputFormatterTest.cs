using UT4MasterServer.Formatters;
using UT4MasterServer.Models.Database;

namespace XUnit.Tests;

public class StatisticBaseInputFormatterTest
{
	[Theory]
	[InlineData("")]
	[InlineData("\0")]
	[InlineData("\0\0\0")]
	[InlineData("   ")]
	[InlineData(" \t\r\n")]
	[InlineData("   \0\0")]
	[InlineData("null")]
	[InlineData("null\0")]
	[InlineData("{ not valid json")]
	[InlineData("[]")]
	[InlineData("\"just a string\"")]
	public void TryParse_InvalidBody_ReturnsFalse(string rawValue)
	{
		var ok = StatisticBaseInputFormatter.TryParse(rawValue, out StatisticBase? result);

		Assert.False(ok);
		Assert.Null(result);
	}

	[Fact]
	public void TryParse_ValidBodyWithTrailingNul_ReturnsParsedStatistic()
	{
		// the game terminates the json body with a trailing NUL character
		const string rawValue = "{\"MatchesPlayed\":3,\"Kills\":15,\"Deaths\":7}\0";

		var ok = StatisticBaseInputFormatter.TryParse(rawValue, out StatisticBase? result);

		Assert.True(ok);
		Assert.NotNull(result);
		Assert.Equal(3, result!.MatchesPlayed);
		Assert.Equal(15, result.Kills);
		Assert.Equal(7, result.Deaths);
	}

	[Fact]
	public void TryParse_ValidBodyWithoutTrailingNul_ReturnsParsedStatistic()
	{
		const string rawValue = "{\"Wins\":2,\"Losses\":1}";

		var ok = StatisticBaseInputFormatter.TryParse(rawValue, out StatisticBase? result);

		Assert.True(ok);
		Assert.NotNull(result);
		Assert.Equal(2, result!.Wins);
		Assert.Equal(1, result.Losses);
	}

	[Fact]
	public void TryParse_EmptyJsonObject_ReturnsEmptyStatistic()
	{
		var ok = StatisticBaseInputFormatter.TryParse("{}", out StatisticBase? result);

		Assert.True(ok);
		Assert.NotNull(result);
		Assert.Null(result!.MatchesPlayed);
	}
}
