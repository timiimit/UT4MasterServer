using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public enum FriendStatus
{
	Blocked,
	Pending,
	Accepted
}

[BsonIgnoreExtraElements]
public class FriendRequest
{
	[BsonElement("Sender")]
	public EpicID Sender { get; set; } = EpicID.Empty;

	[BsonElement("Receiver")]
	public EpicID Receiver { get; set; } = EpicID.Empty;

	[BsonElement("Status")]
	public FriendStatus Status { get; set; } = FriendStatus.Pending;
}
