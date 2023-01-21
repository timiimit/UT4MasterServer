using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Authentication;
using UT4MasterServer.Other;

namespace UT4MasterServer.Models;

public class TrustedGameServer
{
	[BsonElement("ID"), BsonId]
	public EpicID ID { get; set; }

	[BsonElement("Secret")]
	public EpicID Secret { get; set; }

	[BsonElement("OwnerID")]
	public EpicID OwnerID { get; set; }

	[BsonIgnoreIfDefault, BsonDefaultValue(GameServerTrust.Trusted)]
	[BsonElement("TrustLevel")]
	public GameServerTrust TrustLevel { get; set; } = GameServerTrust.Trusted;
}
