using System.Text.Json.Serialization;
using UT4MasterServer.Models.Database;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class LogReportResponse
{
	[JsonPropertyName("reportId")]
	public string ReportID { get; set; }

	[JsonPropertyName("createdAt")]
	public DateTime CreatedAt { get; set; }

	[JsonPropertyName("player")]
	public string? Player { get; set; }

	[JsonPropertyName("note")]
	public string? Note { get; set; }

	[JsonPropertyName("length")]
	public long Length { get; set; }

	[JsonPropertyName("contentType")]
	public string ContentType { get; set; }

	public LogReportResponse(LogReport report)
	{
		ReportID = report.ReportID;
		CreatedAt = report.CreatedAt;
		Player = report.Player;
		Note = report.Note;
		Length = report.Length;
		ContentType = report.ContentType;
	}
}
