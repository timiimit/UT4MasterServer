using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models.Requests;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

/// <summary>
/// persona-public-service-prod06.ol.epicgames.com
/// </summary>
[ApiController]
[Route("persona/api")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class PersonaController : JsonAPIController
{
	private readonly AccountService accountService;

	public PersonaController(ILogger<PersonaController> logger, AccountService accountService) : base(logger)
	{
		this.accountService = accountService;
	}

	[HttpGet("public/account/lookup")]
	public async Task<IActionResult> AccountLookup([FromQuery(Name = "q")] string query)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		var account = await accountService.GetAccountAsync(query);
		if (account == null)
			return BadRequest();

		var obj = new JObject
		{
			{ "id", account.ID.ToString() },
			{ "displayName", account.Username },
			{ "extenalAuths", new JObject() }
		};

		return Json(obj);
	}

	[HttpPost("accounts/search")]
	public async Task<IActionResult> SearchAccounts([FromBody] AccountSearchRequest request)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
		{
			return Unauthorized();
		}
		var accounts = await accountService.SearchAccountsAsync(request.Query);

		if (request.Roles != null && request.Roles.Length > 0)
		{
			accounts = accounts.Where((a) => a.Roles != null && a.Roles.Intersect(request.Roles).Any());
		}

		var count = accounts.Count();

		accounts = accounts.Skip(request.Skip).Take(request.Take);

		if (request.IncludeRoles)
		{
			return Ok(
				new
				{
					accounts = accounts.Select((account) => new { account.ID, account.Username, account.Roles }),
					count
				}
			);
		}
		return Ok(
				new
				{
					accounts = accounts.Select((account) => new { account.ID, account.Username, }),
					count
				}
			);
	}

	[HttpGet("account/{id}")]
	public async Task<IActionResult> GetAccount(string id)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
		{
			return Unauthorized();
		}

		var eid = EpicID.FromString(id);

		if (eid != authenticatedUser.Session.AccountID)
		{
			return Unauthorized();
		}

		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for account {id}");

		var account = await accountService.GetAccountAsync(eid);
		if (account == null)
		{
			return NotFound();
		}

		return Ok(account);
	}

	[HttpPost("accounts")]
	public async Task<IActionResult> GetAccountsByIds([FromBody] string[] ids)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
		{
			return Unauthorized();
		}

		var eIds = ids.Distinct().Select(x => EpicID.FromString(x));
		var accounts = await accountService.GetAccountsAsync(eIds);
		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for limited accounts by ID");

		return Ok(accounts.Select((account) => new { account.ID, account.Username }));
	}
}
