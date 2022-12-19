using MongoDB.Bson.Serialization;

namespace UT4MasterServer
{
	public class EpicIDSerializationProvider : IBsonSerializationProvider
	{
		public IBsonSerializer GetSerializer(Type type)
		{
			if (type == typeof(EpicID))
				return new EpicIDSerializer();
			throw new ArgumentException("Unexpected type");
		}
	}
}