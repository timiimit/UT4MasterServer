using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common;

namespace UT4MasterServer.Models.Database;

public sealed class Statistic : StatisticBase
{
	public Statistic() { }
	public Statistic(StatisticBase statisticBase) : base(statisticBase) { }

	[BsonId, BsonIgnoreIfDefault, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; } = default!;

	public EpicID AccountID { get; set; } = default!;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[BsonIgnoreIfNull]
	public DateTime? ModifiedAt { get; set; }

	public StatisticWindow Window { get; set; } = StatisticWindow.Daily;

	[BsonIgnoreIfNull]
	public List<string>? Flagged { get; set; }
}
