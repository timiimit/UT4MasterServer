using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public class Client
{
	[BsonElement("ID"), BsonId]
	public EpicID ID { get; set; }

	[BsonElement("Secret")]
	public string Secret { get; set; }

	[BsonIgnoreIfNull]
	[BsonElement("Name")]
	public string? Name { get; set; }

	public Client(EpicID id, string secret, string? name = null)
	{
		ID = id;
		Secret = secret;
		Name = name;
	}
}
