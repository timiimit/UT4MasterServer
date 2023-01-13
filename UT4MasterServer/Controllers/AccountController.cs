using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Models.Requests;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

/// <summary>
/// account-public-service-prod03.ol.epicgames.com
/// </summary>
[ApiController]
[Route("account/api")]
[AuthorizeBearer]
[Produces("application/json")]
public class AccountController : JsonAPIController
{
	private readonly AccountService accountService;

	public AccountController(ILogger<AccountController> logger, AccountService accountService) : base(logger)
	{
		this.accountService = accountService;
	}

	#region ACCOUNT LISTING API

	[HttpGet("public/account/{id}")]
	public async Task<IActionResult> GetAccount(string id)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for account {id}");

		// TODO: EPIC doesn't throw here if id is invalid (like 'abc'). Return this same ErrorResponse like for account_not_found
		EpicID eid = EpicID.FromString(id);
		var account = await accountService.GetAccountAsync(eid);
		if (account == null)
			return NotFound(new ErrorResponse
			{
				ErrorCode = "errors.com.epicgames.account.account_not_found",
				ErrorMessage = $"Sorry, we couldn't find an account for {id}",
				MessageVars = new[] { id },
				NumericErrorCode = 18007,
				OriginatingService = "com.epicgames.account.public",
				Intent = "prod",
			});

		var obj = new JObject();
		obj.Add("id", account.ID.ToString());
		obj.Add("displayName", account.Username);
		obj.Add("name", $"{account.Username}"); // fake a random one
		obj.Add("email", $"{account.ID}@{Request.Host}"); // fake a random one
		obj.Add("failedLoginAttempts", 0);
		obj.Add("lastLogin", account.LastLoginAt.ToStringISO());
		obj.Add("numberOfDisplayNameChanges", 0);
		obj.Add("ageGroup", "UNKNOWN");
		obj.Add("headless", false);
		obj.Add("country", "US"); // two letter country code
		obj.Add("lastName", $"{account.Username}"); // fake a random one
		obj.Add("preferredLanguage", "en"); // two letter language code
		obj.Add("canUpdateDisplayName", true);
		obj.Add("tfaEnabled", true);
		obj.Add("emailVerified", true);
		obj.Add("minorVerified", false);
		obj.Add("minorExpected", false);
		obj.Add("minorStatus", "UNKNOWN");
		obj.Add("cabinedMode", false);
		obj.Add("hasHashedEmail", false);

		return Json(obj.ToString(Newtonsoft.Json.Formatting.None));
	}

	[HttpGet("public/account")]
	public async Task<IActionResult> GetAccounts([FromQuery(Name = "accountId")] List<string> accountIDs)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		if (accountIDs.Count == 0 || accountIDs.Count > 100)
		{
			return NotFound(new ErrorResponse
			{
				ErrorCode = "errors.com.epicgames.account.invalid_account_id_count",
				ErrorMessage = "Sorry, the number of account id should be at least one and not more than 100.",
				MessageVars = new[] { "100" },
				NumericErrorCode = 18066,
				OriginatingService = "com.epicgames.account.public",
				Intent = "prod",
			});
		}

		var ids = accountIDs.Distinct().Select(x => EpicID.FromString(x));
		var accounts = await accountService.GetAccountsAsync(ids.ToList());

		var retrievedAccountIDs = accounts.Select(x => x.ID.ToString());
		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for {string.Join(", ", retrievedAccountIDs)}");

		// create json response
		var arr = new JArray();
		foreach (var account in accounts)
		{
			var obj = new JObject();
			obj.Add("id", account.ID.ToString());
			obj.Add("displayName", account.Username);
			if (account.ID == authenticatedUser.Session.AccountID)
			{
				// this is returned only when you ask about yourself
				obj.Add("minorVerified", false);
				obj.Add("minorStatus", "UNKNOWN");
				obj.Add("cabinedMode", false);
			}

			obj.Add("externalAuths", new JObject());
			arr.Add(obj);
		}

		return Json(arr);
	}

	#endregion

	#region UNIMPORTANT API

	[HttpGet("accounts/{id}/metadata")]
	public IActionResult GetMetadata(string id)
	{
		EpicID eid = EpicID.FromString(id);

		logger.LogInformation($"Get metadata of {eid}");

		// unknown structure, but epic always seems to respond with this
		return Json("{}");
	}

	[HttpGet("public/account/{id}/externalAuths")]
	public IActionResult GetExternalAuths(string id)
	{
		EpicID eid = EpicID.FromString(id);

		logger.LogInformation($"Get external auths of {eid}");
		// we don't really care about these, but structure for my github externalAuth is the following:
		/*
		[{
			"accountId": "0b0f09b400854b9b98932dd9e5abe7c5", "type": "github",
			"externalAuthId": "timiimit", "externalDisplayName": "timiimit",
			"authIds": [ { "id": "timiimit", "type": "github_login" } ],
			"dateAdded": "2018-01-17T18:58:39.831Z"
		}]
		*/
		return Json("[]");
	}

	[HttpGet("epicdomains/ssodomains")]
	[AllowAnonymous]
	public IActionResult GetSSODomains()
	{
		logger.LogInformation(@"Get SSO domains");

		// epic responds with this: ["unrealengine.com","unrealtournament.com","fortnite.com","epicgames.com"]

		return Json("[]");
	}

	#endregion

	#region NON-EPIC API

	[HttpPost("create/account")]
	[AllowAnonymous]
	public async Task<IActionResult> RegisterAccount([FromBody] RegisterRequest request)
	{
		// TODO: Add validation
		var account = await accountService.GetAccountAsync(request.Username);
		if (account != null)
		{
			logger.LogInformation($"Could not register duplicate account: {request.Username}");
			return Conflict("Username already exists");
		}

		var email = await accountService.GetAccountEmailAsync(request.Email);
		if (email != null)
		{
			logger.LogInformation($"Could not register duplicate email: {request.Email}");
			return Conflict("Email already exists");
		}

		Regex regex = new Regex(@"^(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
		Match match = regex.Match(request.Email);
		if (!match.Success)
		{
			logger.LogInformation($"Entered an incorrect email format: {request.Email}");
			return Conflict("You have entered and invalid email address");
		}

		await accountService.CreateAccountAsync(request.Username, request.Email, request.Password); // TODO: this cannot fail?

		logger.LogInformation($"Registered new user: {request.Username}");

		return Ok("Account created successfully");
	}

	#endregion
}
