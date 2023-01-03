using MongoDB.Bson.Serialization;
using UT4MasterServer.Models;

namespace UT4MasterServer.Other;

public class BsonSerializationProvider : IBsonSerializationProvider
{
	public IBsonSerializer GetSerializer(Type type)
	{
		if (type == typeof(EpicID))
			return new EpicIDSerializer();
		if (type == typeof(GameServerAttributes))
			return new GameServerAttributesBsonSerializer();

		// returning null here seems to be fine.
		// it probably signals to the caller that we don't have serializer for specified type.
		return null!;
	}
}
