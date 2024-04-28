
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.Extensions.Primitives;
using UT4MasterServer.Controllers.UT;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Controllers;

public sealed class CustomAPIController : JsonAPIController
{
	private readonly IOptions<ApplicationSettings> configuration;
	private readonly AccountService accountService;

	public CustomAPIController(ILogger<MatchmakingController> logger, IOptions<ApplicationSettings> configuration, AccountService accountService) : base(logger)
	{
		this.configuration = configuration;
		this.accountService = accountService;
	}

	[HttpGet("api/ShowMyInfo")]
	public IActionResult ShowMyInfo()
	{
		IPAddress? ip = GetClientIP(configuration);

		var sb = new StringBuilder();

		sb.AppendLine($"Direct IP Address: {HttpContext.Connection.RemoteIpAddress}");
		sb.AppendLine($"Guessed IP Address: {ip}");

		sb.AppendLine();
		sb.AppendLine($"HTTP Method: {HttpContext.Request.Method}");
		sb.AppendLine($"Display URL: {HttpContext.Request.GetDisplayUrl()}");

		sb.AppendLine();
		sb.AppendLine("Query URL Parameters:");
		foreach (KeyValuePair<string, StringValues> parameter in HttpContext.Request.Query)
		{
			sb.AppendLine($"{parameter.Key}={parameter.Value}");
		}

		sb.AppendLine();
		sb.AppendLine("Request Headers:");
		foreach (KeyValuePair<string, StringValues> header in HttpContext.Request.Headers)
		{
			foreach (var headerInstance in header.Value)
			{
				sb.AppendLine($"{header.Key}: {headerInstance}");
			}
		}

		return Content(sb.ToString());
	}

	// www.epicgames.com/id/login?noHostRedirect=true
	[HttpGet("id/login")]
	public IActionResult EpicLoginPage()
	{
		return Redirect($"https://{configuration.Value.WebsiteDomain}/Login");
	}
	[HttpGet("id/api/redirect")]
	public IActionResult EpicGetAuthCodePage()
	{
		return Redirect($"https://{configuration.Value.WebsiteDomain}/Login");
	}
}
