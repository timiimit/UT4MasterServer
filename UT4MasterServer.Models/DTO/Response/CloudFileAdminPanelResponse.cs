using System.Text.Json.Serialization;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class CloudFileAdminPanelResponse
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

	[JsonPropertyName("isReadonly")]
	public bool IsWritable { get; set; }

	public CloudFileAdminPanelResponse(CloudFile file, bool isWritable)
	{
		AccountID = file.AccountID;
		Filename = file.Filename;
		Hash = file.Hash;
		Hash256 = file.Hash256;
		UploadedAt = file.UploadedAt;
		Length = file.Length;

		IsWritable = isWritable;
	}
}
