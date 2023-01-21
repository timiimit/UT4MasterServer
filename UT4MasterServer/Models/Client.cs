using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Authentication;
using UT4MasterServer.Other;

namespace UT4MasterServer.Models;

public class Client
{
	[BsonElement("ID"), BsonId]
	public EpicID ID { get; set; }

	[BsonElement("Secret")]
	public EpicID Secret { get; set; }

	[BsonIgnoreIfNull]
	[BsonElement("Secret")]
	public string? Name { get; set; }

	public Client(EpicID id, EpicID secret, string? name = null)
	{
		ID = id;
		Secret = secret;
		Name = name;
	}
}
