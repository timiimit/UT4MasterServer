using System.Text;
using System.Text.Json;
using UT4MasterServer.Models;

namespace XUnit.Tests;

#if false
public class GameServerAttributeTest
{
	public static TheoryData<object?, object?, bool, bool, bool> TestCases = new()
	{
		{ "3", "3", true, false, true },
		{ "3", 3, false, false, false },
		{ "3", true, false, false, false },
		{ "3", false, false, false, false },
		{ "3", null, false, false, false },
		{ "2", "3", false, true, true },
		{ "4", "3", false, false, false },

		{ 3, "3", false, false, false },
		{ 3, 3, true, false, true },
		{ 3, true, false, false, false },
		{ 3, false, false, false, false },
		{ 3, null, false, false, false },

		{ true, true, true, false, true },
		{ true, "true", false, false, false },
		{ true, 1, false, false, false },
		{ true, null, false, false, false },
		{ false, false, true, false, true },
		{ false, null, false, false, false },
		{ true, false, false, false, false },
		{ false, true, false, true, true },

		{ " ", "", false, false, false },
		{ " ", " ", true, false, true },
		{ "", " ", false, true, true },
	};

	public static TheoryData<object?, bool, bool, bool> TestCasesNull = new()
	{

		{ null, true, false, true },
		{ true, false, false, false },
		{ 3, false, false, false },
		{ "null", false, false, false },
	};

	[Theory]
	[MemberData(nameof(TestCases))]
	public void TestAttributesNonNull(object? attrValue, object? compareValue, bool expectedEq, bool expectedLt, bool expectedLte)
	{
		var jsonElem = CreateJsonElement(compareValue);
		GameServerAttributes gsa = new GameServerAttributes();
		if (attrValue is string attrValueString)
			gsa.Set("key", attrValueString);
		else if (attrValue is int attrValueInt)
			gsa.Set("key", attrValueInt);
		else if (attrValue is bool attrValueBool)
			gsa.Set("key", attrValueBool);
		else
			Assert.Fail("undesired test case");

		Assert.Equal(expectedEq, gsa.Eq("key", jsonElem));
		Assert.Equal(expectedLt, gsa.Lt("key", jsonElem));
		Assert.Equal(expectedLte, gsa.Lte("key", jsonElem));
	}

	[Theory]
	[MemberData(nameof(TestCasesNull))]
	public void TestAttributesNull(object? compareValue, bool expectedEq, bool expectedLt, bool expectedLte)
	{
		var jsonElem = CreateJsonElement(compareValue);
		GameServerAttributes gsa = new GameServerAttributes();
		gsa.Set("key", null as string);
		Assert.Equal(expectedEq, gsa.Eq("key", jsonElem));
		Assert.Equal(expectedLt, gsa.Lt("key", jsonElem));
		Assert.Equal(expectedLte, gsa.Lte("key", jsonElem));

		gsa.Set("key", null as int?);
		Assert.Equal(expectedEq, gsa.Eq("key", jsonElem));
		Assert.Equal(expectedLt, gsa.Lt("key", jsonElem));
		Assert.Equal(expectedLte, gsa.Lte("key", jsonElem));

		gsa.Set("key", null as bool?);
		Assert.Equal(expectedEq, gsa.Eq("key", jsonElem));
		Assert.Equal(expectedLt, gsa.Lt("key", jsonElem));
		Assert.Equal(expectedLte, gsa.Lte("key", jsonElem));
	}

	private static JsonElement CreateJsonElement(object? obj)
	{
		StringBuilder sb = new();
		if (obj is null)
			sb.Append("null");
		else if (obj is string objString)
			sb.Append($"\"{objString}\"");
		else if (obj is int objInt)
			sb.Append(objInt.ToString());
		else if (obj is bool objBool)
			sb.Append(objBool ? "true" : "false");
		else
			Assert.Fail("undesired test case");

		var utf8 = Encoding.UTF8.GetBytes(sb.ToString());

		Utf8JsonReader jsonReader = new(utf8);
		return JsonElement.ParseValue(ref jsonReader);
	}
}
#endif
