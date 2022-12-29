using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("ut/api")]
[AuthorizeBearer]
[Produces("application/json")]
public class UnrealTournamentStatsController : JsonAPIController
{
	private readonly ILogger<SessionController> logger;
	private readonly AccountService accountService;

	public UnrealTournamentStatsController(ILogger<SessionController> logger, AccountService accountService)
	{
		this.logger = logger;
		this.accountService = accountService;

	}

	[HttpGet("stats/accountId/{id}/bulk/window/{category}")]
	public IActionResult Stats(string id, string leagueName, string category)
	{
		// TODO
		switch (category)
		{
			case "alltime":
				break;
			case "monthly":
				break;
			case "weekly":
				break;
			case "daily":
				break;
			default:
				throw new Exception("unknown stats category");
		}

		var league = new League();
		// for now we just send default/empty values
		return Json(league);
	}
}
