
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[Route("api")]
public class CustomAPIController : JsonAPIController
{
	private readonly IOptions<ApplicationSettings> configuration;

	public CustomAPIController(ILogger<UnrealTournamentMatchmakingController> logger, IOptions<ApplicationSettings> configuration) : base(logger)
	{
		this.configuration = configuration;
	}

	[HttpGet("ShowMyInfo")]
	public IActionResult ShowMyInfo()
	{
		var ip = GetClientIP(configuration);

		StringBuilder sb = new StringBuilder();

		sb.AppendLine($"Direct IP Address: {HttpContext.Connection.RemoteIpAddress}");
		sb.AppendLine($"Guessed IP Address: {ip}");

		sb.AppendLine();
		sb.AppendLine($"HTTP Method: {HttpContext.Request.Method}");
		sb.AppendLine($"Display URL: {HttpContext.Request.GetDisplayUrl()}");

		sb.AppendLine();
		sb.AppendLine("Query URL Parameters:");
		foreach (var parameter in HttpContext.Request.Query)
		{
			sb.AppendLine($"{parameter.Key}=");
		}

		sb.AppendLine();
		sb.AppendLine("Request Headers:");
		foreach (var header in HttpContext.Request.Headers)
		{
			foreach (var headerInstance in header.Value)
			{
				sb.AppendLine($"{header.Key}: {headerInstance}");
			}
		}

		return Content(sb.ToString());
	}
}
