using System.Text.Json;
using System.Text.Json.Serialization;

namespace UT4MasterServer;

public class EpicIDJsonConverter : JsonConverter<EpicID>
{
	//public override bool CanWrite { get => true; }

	//public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	//{
	//	if (value is EpicID eid)
	//		writer.WriteToken(JsonToken.String, eid.ToString());
	//}

	//public override bool CanConvert(Type objectType)
	//{
	//	return objectType == typeof(string) || objectType == typeof(EpicID);
	//}

	//public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	//{
	//	if (reader.TokenType == JsonToken.String && objectType == typeof(EpicID))
	//	{
	//		if (reader.Value == null)
	//			return EpicID.Empty;

	//		var str = reader.Value as string;
	//		if (str == null)
	//			return EpicID.Empty;

	//		return EpicID.FromString(str);
	//	}

	//	return null!;
	//}


	public override EpicID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var str = reader.GetString();
		if (str == null)
			return EpicID.Empty;

		return EpicID.FromString(str);
	}

	public override void Write(Utf8JsonWriter writer, EpicID value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}