using UT4MasterServer.Helpers;

namespace XUnit.Tests;

public class StringHelperTest
{
	public static TheoryData<string, bool> IsHexStringTestCases = new()
	{
		{ "deadbeef", true },
		{ "0123456789abcdef", true },
		{ "0123456789ABCDEF", true },
		{ "0123456789abcdef0123456789ABCDEF", true },
		{ "0123456789ABCDEF0123456789abcdef", true },
		{ "gfedcba987654321", false },
		{ "Gfedcba987654321", false },
		{ "deadbeefz", false },
		{ "deadbeef ", false },
		{ "deadbeef\n", false },
		{ "", false },
		{ "   ", false },
		{ null, false },
	};

	[Theory]
	[MemberData(nameof(IsHexStringTestCases))]
	public void TestIsHexString(string input, bool expected)
	{
		Assert.Equal(expected, input.IsHexString());
	}

	[Theory]
	[InlineData("VGhpcyBpcyBhIHZhbGlkIGJhc2U2NCBzdHJpbmc=", true)]
	[InlineData("This is not a valid base64 string", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	public void IsBase64_ReturnsExpectedResult(string input, bool expectedResult)
	{
		// Act
		var result = input.IsBase64();

		// Assert
		Assert.Equal(expectedResult, result);
	}
}
