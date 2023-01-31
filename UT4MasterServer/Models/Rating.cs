using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using UT4MasterServer.Other;

namespace UT4MasterServer.Models;

public sealed class Rating
{
	[BsonId, BsonIgnoreIfDefault, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; } = default!;

	public EpicID AccountID { get; set; } = default!;

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int SkillRating { get; set; }

	[BsonIgnoreIfDefault]
    public int SkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int TDMSkillRating { get; set; }

	[BsonIgnoreIfDefault]
	public int TDMSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int DMSkillRating { get; set; }

	[BsonIgnoreIfDefault]
    public int DMSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int CTFSkillRating { get; set; }

	[BsonIgnoreIfDefault]
    public int CTFSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int ShowdownSkillRating { get; set; }

	[BsonIgnoreIfDefault]
    public int ShowdownSkillRatingGamesPlayed { get; set; }

	[BsonDefaultValue(1500), BsonIgnoreIfDefault]
	public int FlagRunSkillRating { get; set; }

	[BsonIgnoreIfDefault]
    public int FlagRunSkillRatingGamesPlayed { get; set; }
}
