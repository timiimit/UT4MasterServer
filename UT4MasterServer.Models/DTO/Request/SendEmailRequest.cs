namespace UT4MasterServer.Models.DTO.Request;

public sealed class SendEmailRequest
{
	public string From { get; set; } = string.Empty;
	public List<string> To { get; set; } = new();
	public string Subject { get; set; } = string.Empty;
	public string Body { get; set; } = string.Empty;
}
