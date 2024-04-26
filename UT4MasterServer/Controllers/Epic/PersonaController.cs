using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Requests;
using UT4MasterServer.Services.Scoped;

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
		{
			return Unauthorized();
		}

		Account? account = await accountService.GetAccountAsync(query);
		if (account == null)
		{
			return BadRequest();
		}

		var obj = new JObject
		{
			{ "id", account.ID.ToString() },
			{ "displayName", account.Username },
			{ "extenalAuths", new JObject() }
		};

		return Json(obj);
	}

	// TODO: modify into a GET with query parameters
	[HttpPost("accounts/search")]
	public async Task<IActionResult> SearchAccounts([FromBody] AccountSearchRequest request)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
		{
			return Unauthorized();
		}

		// do not allow more than 50 entries
		if (request.Take > 50)
		{
			request.Take = 50;
		}

		AccountFlags flagsMask;
		if (request.Roles != null && request.Roles.Length > 0)
		{
			flagsMask = EnumHelpers.StringsToEnum<AccountFlags>(request.Roles);
		}
		else
		{
			flagsMask = (AccountFlags)~0;
		}

		PagedResponse<Account>? result = await accountService.SearchAccountsAsync(request.Query, flagsMask, request.Skip, request.Take);

		if (request.IncludeRoles)
		{
			return Ok(
				new
				{
					accounts = result.Data.Select((account) => new { account.ID, account.Username, account.Roles }),
					count = result.Count
				}
			);
		}
		return Ok(
				new
				{
					accounts = result.Data.Select((account) => new { account.ID, account.Username, }),
					count = result.Count
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

		logger.LogInformation("{AccountId} is looking for account {id}", authenticatedUser.Session.AccountID, id);

		Account? account = await accountService.GetAccountAsync(eid);
		if (account == null)
		{
			return NotFound();
		}

		return Ok(account);
	}

	// TODO: modify into a GET with query parameters
	[HttpPost("accounts")]
	public async Task<IActionResult> GetAccountsByIds([FromBody] string[] ids)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
		{
			return Unauthorized();
		}

		IEnumerable<EpicID>? eIds = ids.Distinct().Select(x => EpicID.FromString(x));
		IEnumerable<Account>? accounts = await accountService.GetAccountsAsync(eIds);
		logger.LogInformation("{AccountId} is looking for limited accounts by ID", authenticatedUser.Session.AccountID);

		return Ok(accounts.Select((account) => new { account.ID, account.Username }));
	}
}
