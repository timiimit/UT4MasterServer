using UT4MasterServer.Controllers.UT;

namespace XUnit.Tests;

public class RatingsControllerTest
{
	[Theory]
	[InlineData(0, 10, 0, 10)]      // in range, unchanged
	[InlineData(50, 100, 50, 100)]  // upper bounds, unchanged
	[InlineData(-5, 10, 0, 10)]     // negative skip clamps to 0
	[InlineData(-1, 10, 0, 10)]
	[InlineData(0, 0, 0, 100)]      // limit below range falls back to default
	[InlineData(0, -20, 0, 100)]
	[InlineData(0, 101, 0, 100)]    // limit above range falls back to default
	[InlineData(0, 1000, 0, 100)]
	[InlineData(-3, 5000, 0, 100)]  // both out of range
	[InlineData(0, 1, 0, 1)]        // lower limit bound, unchanged
	public void ClampPaging_KeepsValuesWithinSaneBounds(int skip, int limit, int expectedSkip, int expectedLimit)
	{
		var (clampedSkip, clampedLimit) = RatingsController.ClampPaging(skip, limit);

		Assert.Equal(expectedSkip, clampedSkip);
		Assert.Equal(expectedLimit, clampedLimit);
	}
}
