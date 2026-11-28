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

	/// <summary>
	/// Identifies the unordered pair of accounts. Set on requests and friendships only, never on blocks.
	/// A unique index on it guarantees at most one request or friendship per pair.
	/// </summary>
	[BsonElement("PairKey"), BsonIgnoreIfNull]
	public string? PairKey { get; set; }
}
