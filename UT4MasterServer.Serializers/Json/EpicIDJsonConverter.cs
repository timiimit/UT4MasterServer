using System.Text.Json;
using System.Text.Json.Serialization;
using UT4MasterServer.Common;

namespace UT4MasterServer.Serializers.Json;

public class EpicIDJsonConverter : JsonConverter<EpicID>
{
    public override EpicID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert != typeof(string) && typeToConvert != typeof(EpicID))
        {
	        return EpicID.Empty;
        }

        var str = reader.GetString();
        if (str == null)
        {
	        return EpicID.Empty;
        }

        return EpicID.FromString(str);
    }

    public override void Write(Utf8JsonWriter writer, EpicID value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
