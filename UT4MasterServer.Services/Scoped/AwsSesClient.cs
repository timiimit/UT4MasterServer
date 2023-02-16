using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using UT4MasterServer.Common.Exceptions;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

public sealed class AwsSesClient
{
	private readonly ILogger<AwsSesClient> _logger;
	private readonly IAmazonSimpleEmailService _amazonSimpleEmailService;

	public AwsSesClient(ILogger<AwsSesClient> logger, IOptions<AWSSettings> awsSettings)
	{
		_logger = logger;
		_amazonSimpleEmailService = new AmazonSimpleEmailServiceClient(
			awsSettings.Value.AccessKey,
			awsSettings.Value.SecretKey,
			Amazon.RegionEndpoint.GetBySystemName(awsSettings.Value.RegionName));
	}

	public async Task SendTextEmailAsync(string fromAddress, List<string> toAddresses, string subject, string body)
	{
		var request = new SendEmailRequest()
		{
			Source = fromAddress,
			Destination = new Destination(toAddresses),
			Message = new Message()
			{
				Subject = new Content(subject),
				Body = new Body()
				{
					Text = new Content()
					{
						Charset = "UTF-8",
						Data = body,
					}
				},
			}
		};

		await SendEmailAsync(request);
	}

	public async Task SendHTMLEmailAsync(string fromAddress, List<string> toAddresses, string subject, string body)
	{
		var request = new SendEmailRequest()
		{
			Source = fromAddress,
			Destination = new Destination(toAddresses),
			Message = new Message()
			{
				Subject = new Content(subject),
				Body = new Body()
				{
					Html = new Content()
					{
						Charset = "UTF-8",
						Data = body,
					}
				},
			}
		};

		await SendEmailAsync(request);
	}

	private async Task SendEmailAsync(SendEmailRequest request)
	{
		try
		{
			_logger.LogInformation("Sending email.");
			var response = await _amazonSimpleEmailService.SendEmailAsync(request);

			if (response is null)
			{
				throw new AwsSesClientException("Error occurred while sending email. Response not received.");
			}

			_logger.LogInformation("Email sent successfully: {Response}.", JsonSerializer.Serialize(response));
		}
		catch (MessageRejectedException ex)
		{
			_logger.LogError(ex, "Error occurred while sending email: {Request}.", JsonSerializer.Serialize(request));
			throw;
		}
		catch (MailFromDomainNotVerifiedException ex)
		{
			_logger.LogError(ex, "Error occurred while sending email: {Request}.", JsonSerializer.Serialize(request));
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while sending email: {Request}.", JsonSerializer.Serialize(request));
			throw;
		}
	}
}
