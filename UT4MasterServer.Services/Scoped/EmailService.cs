using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

/// <summary>
/// Minimal SMTP email dispatcher. Used today by password reset; safe to
/// generalize to any transactional email.
///
/// If <see cref="MailSettings.IsConfigured"/> is false, <see cref="SendAsync"/>
/// becomes a no-op (returns false). Callers must NOT change their externally
/// observable behavior based on the return — the anti-enumeration guarantee
/// of forgot-password depends on the response being identical whether or
/// not an email actually fired.
/// </summary>
public sealed class EmailService
{
	private readonly ILogger<EmailService> logger;
	private readonly MailSettings settings;

	public EmailService(IOptions<ApplicationSettings> appSettings, ILogger<EmailService> logger)
	{
		this.logger = logger;
		this.settings = appSettings.Value.Mail;
	}

	/// <summary>
	/// Send a plaintext + HTML email. Returns true if SMTP accepted the
	/// message, false if mail is disabled or sending failed.
	/// </summary>
	public async Task<bool> SendAsync(
		string toAddress,
		string subject,
		string plainBody,
		string htmlBody,
		CancellationToken cancellationToken = default)
	{
		if (settings.LogToConsole)
		{
			logger.LogInformation(
				"EmailService.SendAsync: to={To} subject={Subject}\n--- plain ---\n{Plain}\n--- html ---\n{Html}",
				toAddress, subject, plainBody, htmlBody);
		}

		if (!settings.IsConfigured)
		{
			logger.LogInformation("EmailService: skipping send to {To} — Mail.Host/Port not configured", toAddress);
			return false;
		}

		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
		message.To.Add(MailboxAddress.Parse(toAddress));
		message.Subject = subject;
		message.Body = new BodyBuilder
		{
			TextBody = plainBody,
			HtmlBody = htmlBody,
		}.ToMessageBody();

		try
		{
			using var client = new SmtpClient();
			var sslOption = settings.UseSsl
				? SecureSocketOptions.SslOnConnect
				: (settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

			await client.ConnectAsync(settings.Host, settings.Port, sslOption, cancellationToken);

			if (!string.IsNullOrEmpty(settings.Username))
			{
				await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
			}

			await client.SendAsync(message, cancellationToken);
			await client.DisconnectAsync(quit: true, cancellationToken);
			logger.LogInformation("EmailService: delivered to={To} subject={Subject}", toAddress, subject);
			return true;
		}
		catch (Exception ex)
		{
			// Swallow but record. The caller relies on us NOT throwing to keep
			// the API response uniform regardless of SMTP outcome.
			logger.LogWarning(ex, "EmailService: SMTP send failed to={To}", toAddress);
			return false;
		}
	}
}
