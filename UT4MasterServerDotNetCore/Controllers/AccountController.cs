using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
	[ApiController]
	[Route("account/api")]
	public class AccountController : ControllerBase
	{
		private readonly AccountService accountService;
		private readonly ILogger<AccountController> logger;

		public AccountController(AccountService accountService, ILogger<AccountController> logger)
		{
			this.accountService = accountService;
			this.logger = logger;
		}

		// IMPORATANT TODO: all methods which have parameter accessToken need to retrieve the value from Authorization header.
		//                  resources: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization
		//                             https://devblogs.microsoft.com/dotnet/bearer-token-authentication-in-asp-net-core/
		//                  we need to figure out how to use [Authorize] attribute. header is composed out of two parts <bearer|basic> <token>.
		//                  if it starts with "bearer" then <token> is the value of AccessToken.
		//                  if it starts with "basic" then <token> is composed of ClientID and ClientSecret. these can be parsed with ClientIdentification class.

		#region SESSION API

		[HttpPost]
		[Route("oauth/token")]
		public async Task<ActionResult<string>> Authenticate(
			[FromForm(Name = "grant_type")] string grantType,
			[FromForm(Name = "includePerms")] bool includePerms)
		{
			// TODO: extract clientID from Autorization header with ClientIdentification
			EpicID clientID = EpicID.GenerateNew();
			Session? session = null;
			switch (grantType)
			{
				case "authorization_code":
					session = await accountService.CreateSessionWithAuthorizationCodeAsync(Request.Form["code"], clientID);
					break;
				case "exchange_code":
					session = await accountService.CreateSessionWithExchangeCodeAsync(Request.Form["code"], clientID);
					break;
				case "client_credentials":
					// TODO: check whether username&password are included in request, currently we assume userless session no matter what
					session = await accountService.CreateSessionWithCredentialsAsync(string.Empty, string.Empty);
					break;
				default:
					return BadRequest(new ErrorResponse()
					{
						Error = "invalid_grant"
					});
			}

			if (session == null) // only here to prevent null warnings, should never happen
				return BadRequest(); 


			var account = await accountService.GetAccountAsync(session.AccountID);
			logger.LogInformation($"User {account} was authorized via {grantType}");

			JObject obj = new JObject();
			obj.Add("access_token", new JValue(session.AccessToken.Value));
			obj.Add("expires_in", new JValue(session.AccessToken.ExpirySeconds));
			obj.Add("expires_at", new JValue(session.AccessToken.Expiration.ToStringISO()));
			obj.Add("token_type", new JValue("bearer"));
			if (!session.AccountID.IsEmpty && account != null)
			{
				obj.Add("refresh_token", new JValue(session.RefreshToken.Value));
				obj.Add("refresh_expires", new JValue(session.RefreshToken.ExpirySeconds));
				obj.Add("refresh_expires_at", new JValue(session.RefreshToken.Expiration.ToStringISO()));
				obj.Add("account_id", new JValue(account.ID.ToString()));
			}
			obj.Add("client_id", new JValue(session.ClientID));
			obj.Add("internal_client", new JValue(false));
			obj.Add("client_service", new JValue("ut"));
			if (!session.AccountID.IsEmpty && account != null)
			{
				obj.Add("displayName", new JValue(account.Username));

				if (includePerms)
				{
					// should probably be okay to send empty array
					obj.Add("perms", new JArray());

					// here is actual response just in case. in original form it was all in one line.
					/*
					[
						{"resource":"entitlement:account:0b0f09b400854b9b98932dd9e5abe7c5:entitlements","action":2},
						{"resource":"ut:cloudstorage:user:*:stats.json","action":2},
						{"resource":"account:accounts:0b0f09b400854b9b98932dd9e5abe7c5:metadata","action":2},
						{"resource":"account:public:account:*","action":2},
						{"resource":"eulatracking:public:displayagreement:0b0f09b400854b9b98932dd9e5abe7c5","action":2},
						{"resource":"ut:cloudstorage:system:*","action":2},
						{"resource":"friends:0b0f09b400854b9b98932dd9e5abe7c5","action":15},
						{"resource":"ut:profile:0b0f09b400854b9b98932dd9e5abe7c5:commands","action":15},
						{"resource":"entitlement","action":2},
						{"resource":"account:token:otherSessionsForAccountClient","action":8},
						{"resource":"xmpp:session:*:0b0f09b400854b9b98932dd9e5abe7c5","action":1},
						{"resource":"persona:settings:account:0b0f09b400854b9b98932dd9e5abe7c5","action":15},
						{"resource":"ut:stats:0b0f09b400854b9b98932dd9e5abe7c5","action":7},
						{"resource":"ut:matchmaking:session:*","action":15},
						{"resource":"ut:replay:event","action":4},
						{"resource":"userList:ut:0b0f09b400854b9b98932dd9e5abe7c5","action":15},
						{"resource":"persona:accounts:0b0f09b400854b9b98932dd9e5abe7c5","action":15},
						{"resource":"persona:account:lookup","action":2},
						{"resource":"ut:cloudstorage:user:*","action":2},
						{"resource":"ut:cloudstorage:user:0b0f09b400854b9b98932dd9e5abe7c5:*","action":15},
						{"resource":"ut:matchmaking:session","action":15},
						{"resource":"userList:ut:0b0f09b400854b9b98932dd9e5abe7c5:*:*","action":15},
						{"resource":"account:public:account","action":2},
						{"resource":"ut:stats:*","action":2},
						{"resource":"userList:ut:0b0f09b400854b9b98932dd9e5abe7c5:*","action":15},
						{"resource":"ut:cloudstorage:user:0b0f09b400854b9b98932dd9e5abe7c5","action":15},
						{"resource":"xmpp:session:tcp:*:0b0f09b400854b9b98932dd9e5abe7c5","action":1},
						{"resource":"account:token:otherSessionsForAccountClientService","action":8},
						{"resource":"ut:cloudstorage:user","action":2},
						{"resource":"ut:cloudstorage:user:*:*","action":2},
						{"resource":"eulatracking:public:response:0b0f09b400854b9b98932dd9e5abe7c5","action":3},
						{"resource":"blockList:0b0f09b400854b9b98932dd9e5abe7c5","action":14},
						{"resource":"ut:cloudstorage:system","action":2}
					]
					*/
				}

				obj.Add("app", new JValue("ut"));
				obj.Add("in_app_id", new JValue(account.ID.ToString()));
				obj.Add("device_id", new JValue("465a117c2b144b5c8222ee71b9bc8da2")); // unsure about this, probably some ip tracking feature
			}
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpGet]
		[Route("oauth/exchange")]
		public async Task<ActionResult<string>> CreateExchangeCode(string accessToken)
		{
			var session = await accountService.GetSessionAsync(accessToken);
			if (session == null)
				return BadRequest(new ErrorResponse()
				{
					Error = "invalid_token" // TODO: find proper response
				});

			var code = await accountService.CreateExchangeCodeAsync(session.ID);
			if (code == null)
				return BadRequest(new ErrorResponse()
				{
					Error = "cannot_create_exchangecode" // TODO: find proper response
				});

			var obj = new JObject();
			obj.Add("expiresInSeconds", code.Token.ExpirySeconds);
			obj.Add("code", code.Token.Value);
			obj.Add("creatingClientId", code.CreatingClientID.ToString());
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpDelete]
		[Route("oauth/sessions/kill/{accessToken}")]
		public async Task<NoContentResult> KillSession(string accessToken)
		{
			logger.LogInformation($"Deleted session with token = {Request.Path.Value}");

			var session = await accountService.GetSessionAsync(accessToken);

			return new NoContentResult();
		}

		[HttpDelete]
		[Route("oauth/sessions/kill")]
		public async Task<NoContentResult> KillSession([FromQuery] string killType, string accessToken)
		{
			logger.LogInformation($"Deleted session with token = {Request.Path.Value}");

			if (killType == "OTHERS_ACCOUNT_CLIENT_SERVICE")
			{
				Session? safeSession = await accountService.GetSessionAsync(accessToken);
				if (safeSession == null)
					return NoContent();

				await accountService.RemoveOtherSessionsAsync(safeSession.ClientID, safeSession.ID);
			}
			// TODO: find other valid strings

			return new NoContentResult();
		}

		#endregion

		#region ACCOUNT LISTING API

		[HttpGet]
		[Route("public/account/{id}")]
		public async Task<ActionResult<string>> GetAccount(EpicID id)
		{
			logger.Log(LogLevel.Information, $"Looking for account {id}");
			var account = await accountService.GetAccountAsync(id);
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

		[HttpGet]
		[Route("public/account")]
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

		[HttpGet]
		[Route("accounts/{idString}/metadata")]
		public ActionResult<string> GetMetadata(string idString)
		{
			EpicID id = new EpicID(idString);

			logger.LogInformation($"Get metadata of {id}");

			// unknown structure, but epic always seems to respond with this
			return "{}";
		}

		[HttpGet]
		[Route("public/account/{idString}/externalAuths")]
		public ActionResult<string> GetExternalAuths(string idString)
		{
			EpicID id = new EpicID(idString);

			logger.LogInformation($"Get external auths of {id}");
			// we dont really care about these, but structure for github is the following:
			/*
			[
			  {
				"accountId": "0b0f09b400854b9b98932dd9e5abe7c5",
				"type": "github",
				"externalAuthId": "timiimit",
				"externalDisplayName": "timiimit",
				"authIds": [
				  {
					"id": "timiimit",
					"type": "github_login"
				  }
				],
				"dateAdded": "2018-01-17T18:58:39.831Z"
			  }
			]
			*/
			return "[]";
		}

		#endregion

		#region OTHER API

		[HttpGet]
		[Route("epicdomains/ssodomains")]
		public ActionResult<string> GetSSODomains()
		{
			logger.LogInformation(@"Get SSO domains");

			// epic responds with this: ["unrealengine.com","unrealtournament.com","fortnite.com","epicgames.com"]

			return "[]";
		}

		#endregion

		#region NON-EPIC API

		[HttpPost]
		[Route("create/account")]
		public async Task<NoContentResult> RegisterAccount([FromForm] string username, [FromForm] string password)
		{
			// TODO: should we also get user's email?
			await accountService.CreateAccountAsync(username, password); // TODO: this cannot fail?

			logger.LogInformation($"Registered new user: {username}");

			return NoContent();
		}

		[HttpPost]
		[Route("login/account")]
		public async Task<ActionResult> LoginAccount([FromForm] string username, [FromForm] string password)
		{
			var session = await accountService.CreateSessionWithCredentialsAsync(username, password);
			if (session == null)
				return NotFound();

			logger.LogInformation($"Created session {session.ID} for user {username}");
			return NoContent();
		}

		[HttpPost]
		[Route("create/authcode")]
		public async Task<ActionResult<string>> CreateAuthorizationCode([FromForm] string username)
		{
			// this action is originally on "www.epicgames.com/id/api/redirect" + some query specitying client_id and something else, forgot what

			// TODO: inspect http when signing in on website and use client_credentials method instead of this method with custom url

			// TODO: this needs to get account id from authorization header
			var account = await accountService.GetAccountAsync(username);
			if (account == null)
				return NotFound();

			var session = await accountService.GetSessionAsync(account.ID, ClientIdentification.Launcher.ID);
			if (session == null)
				return NotFound();

			var authCode = await accountService.CreateAuthorizationCodeAsync(session.ID);
			if (authCode == null)
				return BadRequest();

			logger.LogInformation($"Created authorization code: {authCode.Token}");

			// this is epic's response when you are logged in.
			// when you are not logged in, sid is set, and authorizationCode is null (or maybe just empty?)
			var obj = new JObject();
			obj.Add("redirectUrl", $"https://localhost/launcher/authorized?code={authCode.Token}");
			obj.Add("authorizationCode", authCode.Token.ToString());
			obj.Add("sid", null);
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		#endregion
	}
}
