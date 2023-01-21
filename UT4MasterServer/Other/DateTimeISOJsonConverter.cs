using System.Text.Json;
using System.Text.Json.Serialization;

namespace UT4MasterServer.Other;

public class DateTimeISOJsonConverter : JsonConverter<DateTime>
{
	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (typeToConvert != typeof(string) && typeToConvert != typeof(DateTime))
			return DateTime.MinValue;

		var str = reader.GetString();
		if (str == null)
			return DateTime.MinValue;

		DateTime ret;
		if (!DateTime.TryParse("yyyy-MM-dd'T'HH:mm:ss.fffK", out ret))
			return DateTime.MinValue;

		return ret;
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		string val = value.ToStringISO();
		writer.WriteStringValue(val);
	}
}
