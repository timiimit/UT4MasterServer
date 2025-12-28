using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public sealed class Rating
{
	public const int DefaultRating = 1500;
	public const int Precision = 1000;

	public static readonly List<string> DmRatingTypes =
	[
		"DMSkillRating",
	];

	public static readonly List<string> TeamRatingTypes =
	[
		"SkillRating",
		"TDMSkillRating",
		"CTFSkillRating",
		"ShowdownSkillRating",
		"FlagRunSkillRating",
		"RankedDuelSkillRating",
		"RankedCTFSkillRating",
		"RankedShowdownSkillRating",
		"RankedFlagRunSkillRating",
	];

	public static readonly List<string> AllowedRatingTypes = DmRatingTypes.Union(TeamRatingTypes).ToList();

	[BsonId, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; } = default!;

	public EpicID AccountID { get; set; } = default!;

	public string RatingType { get; set; } = string.Empty;

	[BsonDefaultValue(DefaultRating * Precision), BsonIgnoreIfDefault]
	public int RatingValue { get; set; }

	[BsonIgnoreIfDefault]
	public int GamesPlayed { get; set; }
}
