using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UT4MasterServer.Models.Database;

/// <summary>
/// Metadata about a single uploaded bug-report log file.
/// The log file bytes themselves are stored in a GridFS bucket and referenced by <see cref="FileID"/>.
/// </summary>
public sealed class LogReport
{
	[BsonId, BsonIgnoreIfDefault, BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
	public string ID { get; set; } = default!;

	/// <summary>
	/// Short, human-friendly identifier of this report (8 uppercase Crockford base32 characters).
	/// </summary>
	public string ReportID { get; set; } = string.Empty;

	/// <summary>
	/// ID of the GridFS file containing the uploaded log bytes.
	/// </summary>
	public ObjectId FileID { get; set; }

	[BsonIgnoreIfNull]
	public string? Player { get; set; }

	[BsonIgnoreIfNull]
	public string? Note { get; set; }

	/// <summary>
	/// SHA-256 hash (lowercase hex) of the uploader's IP address. The raw IP is never stored.
	/// </summary>
	public string ClientIPHash { get; set; } = string.Empty;

	public string UserAgent { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Size of the uploaded log file in bytes.
	/// </summary>
	public long Length { get; set; }

	public string ContentType { get; set; } = string.Empty;
}
