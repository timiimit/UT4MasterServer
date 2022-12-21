using Newtonsoft.Json;

namespace UT4MasterServer;

public class GameServerJsonConverter : JsonConverter
{
	public override bool CanWrite { get => false; }

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(GameServer) || objectType == typeof(string);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		//if (reader.TokenType == JsonToken.String && objectType == typeof(GameServer))
		//{
		//	if (reader.Value == null)
		//		return EpicID.Empty;

		//	var str = reader.Value as GameServer;
		//	if (str == null)
		//		return EpicID.Empty;

		//	return EpicID.FromString(str);
		//}

		return null!;
	}
}