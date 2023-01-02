using System.Text.Json;
using System.Text.Json.Serialization;
using UT4MasterServer.Models;

namespace UT4MasterServer.Other;

public class GameServerAttributesJsonConverter : JsonConverter<GameServerAttributes>
{
	public override GameServerAttributes? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartObject)
			return null;

		GameServerAttributes attribs = new GameServerAttributes();

		while (true)
		{
			reader.Read();

			if (reader.TokenType == JsonTokenType.EndObject)
				return attribs;

			if (reader.TokenType != JsonTokenType.PropertyName)
				return null;

			var attributeName = reader.GetString();
			if (attributeName == null)
				return null;

			if (!reader.Read())
				return null;

			if (reader.TokenType == JsonTokenType.String)
			{
				attribs.Set(attributeName, reader.GetString());
			}
			else if (reader.TokenType == JsonTokenType.Number)
			{
				attribs.Set(attributeName, reader.GetInt32());
			}
			else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
			{
				attribs.Set(attributeName, reader.GetBoolean());
			}
			else if (reader.TokenType == JsonTokenType.Null)
			{
				attribs.Set(attributeName, null as string); // need to specify some type
			}
			else
			{
				return null;
			}
		}
	}

	public override void Write(Utf8JsonWriter writer, GameServerAttributes value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		foreach (var attrib in value.GetUnderlyingDict())
		{
			writer.WritePropertyName(attrib.Key);
			if (attrib.Value is string)
				writer.WriteStringValue(attrib.Value as string);
			else if (attrib.Value is int)
				writer.WriteNumberValue((int)attrib.Value);
			else if (attrib.Value is bool)
				writer.WriteBooleanValue((bool)attrib.Value);
		}
		writer.WriteEndObject();
	}
}
