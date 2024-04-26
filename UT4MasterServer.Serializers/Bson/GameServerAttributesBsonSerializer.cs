using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using UT4MasterServer.Models;

namespace UT4MasterServer.Serializers.Bson;

public class GameServerAttributesBsonSerializer : SerializerBase<GameServerAttributes>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, GameServerAttributes value)
    {
        context.Writer.WriteStartDocument();

        foreach (var key in value.GetKeys())
        {
            var val = value.Get(key);

            if (val is string valString)
            {
	            context.Writer.WriteString(key, valString);
            }
            else if (val is int valInt)
            {
	            context.Writer.WriteInt32(key, valInt);
            }
            else if (val is bool valBool)
            {
	            context.Writer.WriteBoolean(key, valBool);
            }
            else
            {
                // Other kv-pairs are ignored because they are invalid
                // TODO: Is throw more appropriate?
                continue;
            }
        }

        context.Writer.WriteEndDocument();
    }

    public override GameServerAttributes Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var result = new GameServerAttributes();

        if (context.Reader.CurrentBsonType != MongoDB.Bson.BsonType.Document)
        {
	        throw new FormatException("Cannot deserialize into GameServerAttributes");
        }

        context.Reader.ReadStartDocument();

        while (true)
        {
            var t = context.Reader.ReadBsonType();
            if (t == MongoDB.Bson.BsonType.EndOfDocument)
            {
	            break;
            }

            var key = context.Reader.ReadName();

            if (t == MongoDB.Bson.BsonType.String)
            {
	            result.Set(key, context.Reader.ReadString());
            }
            else if (t == MongoDB.Bson.BsonType.Int32)
            {
	            result.Set(key, context.Reader.ReadInt32());
            }
            else if (t == MongoDB.Bson.BsonType.Boolean)
            {
	            result.Set(key, context.Reader.ReadBoolean());
            }
            else
            {
	            throw new FormatException("Cannot deserialize into GameServerAttributes");
            }
        }


        context.Reader.ReadEndDocument();

        return result;
    }
}
