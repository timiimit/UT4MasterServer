

using MongoDB.Bson.Serialization.Attributes;

namespace UT4MasterServer.Models;

[BsonIgnoreExtraElements]
public class CloudFile
{
	// ID = AccountID + Filename = unique key

	[BsonElement("AccountID")]
	public EpicID AccountID { get; set; } = EpicID.Empty; // EpicID.Empty is used for system files

	[BsonElement("Filename")]
	public string Filename { get; set; } = string.Empty;

	[BsonElement("Hash")]
	public string Hash { get; set; } = string.Empty; // TODO: figure out hash algo

	[BsonElement("Hash256")]
	public string Hash256 { get; set; } = string.Empty; // TODO: figure out hash algo

	[BsonElement("UploadedAt")]
	public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

	[BsonElement("RawContent")]
	[BsonRepresentation(MongoDB.Bson.BsonType.Binary)]
	public byte[] RawContent { get; set; } = Array.Empty<byte>();

	[BsonElement("Length")]
	public int Length { get; set; } = 0;
}