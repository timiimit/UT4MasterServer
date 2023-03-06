namespace UT4MasterServer.Services.Interfaces;

public interface IEmailService
{
	Task SendTextEmailAsync(string fromAddress, List<string> toAddresses, string subject, string body);
	Task SendHTMLEmailAsync(string fromAddress, List<string> toAddresses, string subject, string body);
}
