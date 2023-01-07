
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

	[HttpGet("ShowMyIP")]
	public IActionResult ShowMyIP()
	{
		var ip = GetClientIP(configuration);

		return Content($"Received IP: {HttpContext.Connection.RemoteIpAddress}\nProxy says it's actually: {ip}");
	}
}
