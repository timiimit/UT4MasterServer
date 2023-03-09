using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public class TrustedGameServer
{
	[BsonId]
	public EpicID ID { get; set; }

	[BsonElement("OwnerID")]
	public EpicID OwnerID { get; set; }

	[BsonIgnoreIfDefault, BsonDefaultValue(GameServerTrust.Trusted)]
	[BsonElement("TrustLevel")]
	public GameServerTrust TrustLevel { get; set; } = GameServerTrust.Trusted;
}
