using System.Text.Json.Serialization;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class LogReportCreatedResponse
{
	[JsonPropertyName("reportId")]
	public string ReportID { get; set; }

	public LogReportCreatedResponse(string reportID)
	{
		ReportID = reportID;
	}
}
