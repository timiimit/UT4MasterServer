using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
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

		var obj = new JObject();
		obj.Add("id", account.ID.ToString());
		obj.Add("displayName", account.Username);
		obj.Add("extenalAuths", new JObject());

		return Json(obj);
	}

	[HttpGet("account/{id}")]
	public async Task<IActionResult> GetAccount(string id)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		EpicID eid = EpicID.FromString(id);

		if (eid != authenticatedUser.Session.AccountID)
			return Unauthorized();

		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for account {id}");

		var account = await accountService.GetAccountAsync(eid);
		if (account == null)
			return NotFound();

		return Json(account);
	}

	[HttpGet("accounts")]
	public async Task<IActionResult> GetAllAccounts()
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		var accounts = await accountService.GetAllAccountsAsync();
		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for all accounts");

		// create json response
		var arr = new JArray();
		// TODO: Limit to 1000 for now, just to not allow unlimited access to the accounts collection. Should be replaced with paging or a search function at some point.
		foreach (var account in accounts.Take(1000))
		{
			var obj = new JObject();
			obj.Add("ID", account.ID.ToString());
			obj.Add("Username", account.Username);
			arr.Add(obj);
		}

		return Json(arr);
	}

}
