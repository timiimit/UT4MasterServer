using System.Text.Json.Serialization;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class CloudFileResponse
{
	[JsonPropertyName("accountId")]
	public EpicID AccountID { get; set; }

	[JsonPropertyName("filename")]
	public string Filename { get; set; }

	[JsonPropertyName("hash")]
	public string Hash { get; set; }

	[JsonPropertyName("hash256")]
	public string Hash256 { get; set; }

	[JsonPropertyName("uploadedAt")]
	public DateTime UploadedAt { get; set; }

	[JsonPropertyName("length")]
	public int Length { get; set; }

	[JsonPropertyName("uniqueFilename")]
	public string UniqueFilename { get; set; }

	[JsonPropertyName("contentType")]
	public string ContentType { get; set; }

	[JsonPropertyName("storageType")]
	public string StorageType { get; set; }

	[JsonPropertyName("doNotCache"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public bool? DoNotCache { get; set; }

	public CloudFileResponse(CloudFile file)
	{
		AccountID = file.AccountID;
		Filename = file.Filename;
		Hash = file.Hash;
		Hash256 = file.Hash256;
		UploadedAt = file.UploadedAt;
		Length = file.Length;

		UniqueFilename = file.Filename;
		ContentType = "text/plain";
		StorageType = "S3";
	}
}
