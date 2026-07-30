using System.Text.Json.Nodes;
using UT4MasterServer.Models;

namespace XUnit.Tests;

public class GameServerAttributeTest
{
	[Fact]
	public void ToJObject_EmitsEachStoredType()
	{
		var attributes = new GameServerAttributes();
		attributes.Set("UT_SERVERNAME_s", "My Server");
		attributes.Set("UT_RANKED_i", 1);
		attributes.Set("UT_PRIVATE_b", true);

		JsonObject obj = attributes.ToJObject();

		Assert.Equal(3, obj.Count);
		Assert.Equal("My Server", obj["UT_SERVERNAME_s"]!.GetValue<string>());
		Assert.Equal(1, obj["UT_RANKED_i"]!.GetValue<int>());
		Assert.True(obj["UT_PRIVATE_b"]!.GetValue<bool>());
	}

	[Fact]
	public void ToJObject_KeyWithoutTypeSuffix_DoesNotThrow()
	{
		var attributes = new GameServerAttributes();
		attributes.Set("CUSTOMKEY", "value");

		// used to throw while serializing the server list because the key
		// does not end in a known type suffix
		JsonObject obj = attributes.ToJObject();

		Assert.Equal("value", obj["CUSTOMKEY"]!.GetValue<string>());
	}

	[Fact]
	public void ToJObject_KeyWithMismatchedTypeSuffix_EmitsActualStoredType()
	{
		var attributes = new GameServerAttributes();
		attributes.Set("UT_MISMATCH_i", "not an int");
		attributes.Set("UT_MISMATCH_s", 42);
		attributes.Set("UT_MISMATCH_b", 7);

		// used to throw an invalid cast because the value type did not
		// match the key suffix
		JsonObject obj = attributes.ToJObject();

		Assert.Equal("not an int", obj["UT_MISMATCH_i"]!.GetValue<string>());
		Assert.Equal(42, obj["UT_MISMATCH_s"]!.GetValue<int>());
		Assert.Equal(7, obj["UT_MISMATCH_b"]!.GetValue<int>());
	}

	[Fact]
	public void ToJObject_MixedAttributes_SerializesToJson()
	{
		var attributes = new GameServerAttributes();
		attributes.Set("UT_SERVERNAME_s", "Server");
		attributes.Set("UT_SERVERTRUSTLEVEL_i", 2);
		attributes.Set("BADKEY", true);
		attributes.Set("UT_WEIRD_i", "string stored under int key");

		var json = attributes.ToJObject().ToJsonString();

		Assert.False(string.IsNullOrEmpty(json));
	}

	[Fact]
	public void ToJObject_NullValue_RemovesAttribute()
	{
		var attributes = new GameServerAttributes();
		attributes.Set("UT_SERVERNAME_s", "Server");
		attributes.Set("UT_SERVERNAME_s", (string?)null);

		JsonObject obj = attributes.ToJObject();

		Assert.False(attributes.Contains("UT_SERVERNAME_s"));
		Assert.Empty(obj);
	}
}
