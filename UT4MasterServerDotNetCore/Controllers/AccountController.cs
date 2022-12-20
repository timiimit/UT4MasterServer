using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
	[ApiController]
	[Route("account/api")]
	public class AccountController : ControllerBase
	{
		private readonly ILogger<AccountController> logger;
		private readonly AccountService accountService;
		private readonly SessionService sessionService;

		public AccountController(AccountService accountService, SessionService sessionService, ILogger<AccountController> logger)
		{
			this.logger = logger;
			this.accountService = accountService;
			this.sessionService = sessionService;
		}

		#region ACCOUNT LISTING API

		[HttpGet("public/account/{id}")]
		[AuthorizeBearer]
		public async Task<ActionResult<string>> GetAccount(string id)
		{
			logger.Log(LogLevel.Information, $"Looking for account {id}");
			var epicID = new EpicID(id);
			var account = await accountService.GetAccountAsync(epicID);
			if (account == null)
				return NotFound();

			var obj = new JObject();
			obj.Add("id", account.ID.ToString());
			obj.Add("displayName", account.Username);
			obj.Add("name", $"{account.Username}"); // fake a random one
			obj.Add("email", $"{account.ID}@{Request.Host}"); // fake a random one
			obj.Add("failedLoginAttempts", 0);
			obj.Add("lastLogin", account.LastLogin.ToStringISO());
			obj.Add("numberOfDisplayNameChanges", 0);
			obj.Add("ageGroup", "UNKNOWN");
			obj.Add("headless", false);
			obj.Add("country", "SI"); // two letter country code
			obj.Add("lastName", $"{account.Username}"); // fake a random one
			obj.Add("preferredLanguage", "en");
			obj.Add("canUpdateDisplayName", true);
			obj.Add("tfaEnabled", true);
			obj.Add("emailVerified", true);
			obj.Add("minorExpected", false);
			obj.Add("minorStatus", "UNKNOWN");
			obj.Add("cabinedMode", false);
			obj.Add("hasHashedEmail", false);

			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpGet("public/account")]
		public async Task<ActionResult<string>> GetAccounts([FromQuery(Name = "accountId")] List<EpicID> accountIDs)
		{
			logger.LogInformation($"List accounts: {string.Join(", ", accountIDs)}");

			// TODO: remove duplicates from accountIDs
			var accounts = await accountService.GetAccountsAsync(accountIDs);
			if (accounts is null)
				return NotFound();


			// create json response
			JArray arr = new JArray();
			foreach (var account in accounts)
			{
				var obj = new JObject();
				obj.Add("id", account.ID.ToString());
				obj.Add("displayName", account.Username);
				//if (account.ID == ???)
				{
					// this is returned only when you ask about yourself
					obj.Add("minorVerified", false);
					obj.Add("minorStatus", "UNKNOWN");
					obj.Add("cabinedMode", false);
				}
				obj.Add("externalAuths", new JObject());
				arr.Add(obj);
			}

			return arr.ToString(Newtonsoft.Json.Formatting.None);
		}

		#endregion

		#region UNIMPORTANT API

		[HttpGet("accounts/{idString}/metadata")]
		public ActionResult<string> GetMetadata(string idString)
		{
			EpicID id = new EpicID(idString);

			logger.LogInformation($"Get metadata of {id}");

			// unknown structure, but epic always seems to respond with this
			return "{}";
		}

		[HttpGet("public/account/{idString}/externalAuths")]
		public ActionResult<string> GetExternalAuths(string idString)
		{
			EpicID id = new EpicID(idString);

			logger.LogInformation($"Get external auths of {id}");
			// we dont really care about these, but structure for github is the following:
			/*
			[{
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5", "type": "github",
				"externalAuthId": "timiimit", "externalDisplayName": "timiimit",
				"authIds": [ { "id": "timiimit", "type": "github_login" } ],
				"dateAdded": "2018-01-17T18:58:39.831Z"
			}]
			*/
			return "[]";
		}

		[HttpGet("epicdomains/ssodomains")]
		public ActionResult<string> GetSSODomains()
		{
			logger.LogInformation(@"Get SSO domains");

			// epic responds with this: ["unrealengine.com","unrealtournament.com","fortnite.com","epicgames.com"]

			return "[]";
		}

		#endregion

		#region NON-EPIC API

		[HttpPost("create/account")]
		public async Task<IActionResult> RegisterAccount([FromForm] string username, [FromForm] string password)
		{
			if (await accountService.GetAccountAsync(username) != null)
			{
				logger.LogInformation($"Could not register duplicate account: {username}");
				// This is a generic HTTP 400. A 409 (Conflict) might be more appropriate?	
				// Depends how our create account form is built. It will need to know why
				// it failed (dupe account, invalid name, bad password, etc)
				return new BadRequestObjectResult("Username already exists");
			}

			// TODO: should we also get user's email?
			await accountService.CreateAccountAsync(username, password); // TODO: this cannot fail?

			logger.LogInformation($"Registered new user: {username}");

			return NoContent();
		}

		#endregion
	}
}
