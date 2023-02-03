using System.Text;
using UT4MasterServer.Common.Helpers;
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
	public void TryDecodeBase64_ReturnsExpectedResult(string input, bool expectedResult)
	{
		// Act
		var result = input.TryDecodeBase64(out var parsedBytes);

		// Assert
		Assert.Equal(expectedResult, result);
		if (expectedResult)
		{
			Assert.NotNull(parsedBytes);
			Assert.Equal(Encoding.UTF8.GetString(Convert.FromBase64String(input)), Encoding.UTF8.GetString(parsedBytes));
		}
		else
		{
			Assert.Null(parsedBytes);
		}
	}
}
