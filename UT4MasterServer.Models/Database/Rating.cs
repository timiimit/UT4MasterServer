using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models;

public sealed class Rating
{
	public const int DefaultRating = 1500;
	public const int Precision = 1000;

	[BsonId, BsonIgnoreIfDefault, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; } = default!;

	public EpicID AccountID { get; set; } = default!;

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int SkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int SkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int TDMSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int TDMSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int DMSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int DMSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int CTFSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int CTFSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int ShowdownSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int ShowdownSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int FlagRunSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int FlagRunSkillRatingGamesPlayed { get; set; }
}
