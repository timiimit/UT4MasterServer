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
	[Route("account/api/oauth")]
	public class SessionController : ControllerBase
	{
		private readonly ILogger<SessionController> logger;
		private readonly AccountService accountService;
		private readonly SessionService sessionService;

		public SessionController(SessionService sessionService, AccountService accountService, ILogger<SessionController> logger)
		{
			this.sessionService = sessionService;
			this.accountService = accountService;
			this.logger = logger;
		}

		// IMPORATANT TODO: all methods which have parameter accessToken need to retrieve the value from Authorization header.
		//                  resources: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization
		//                             https://devblogs.microsoft.com/dotnet/bearer-token-authentication-in-asp-net-core/
		//                  we need to figure out how to use [Authorize] attribute. header is composed out of two parts <bearer|basic> <token>.
		//                  if it starts with "bearer" then <token> is the value of AccessToken.
		//                  if it starts with "basic" then <token> is composed of ClientID and ClientSecret. these can be parsed with ClientIdentification class.

		[HttpPost]
		[Route("token")]
		public async Task<ActionResult<string>> Authenticate(
			[FromForm(Name = "grant_type")] string grantType,
			[FromForm(Name = "includePerms")] bool? includePerms,
			[FromForm(Name = "code")] string? code,
			[FromForm(Name = "username")] string? username,
			[FromForm(Name = "password")] string? password)
		{
			// TODO: extract clientID from Autorization header with ClientIdentification
			EpicID clientID = EpicID.GenerateNew();
			Session? session = null;
			Account? account = null;
			switch (grantType)
			{
				case "authorization_code":
					if (code != null)
					{
						var codeAuth = await sessionService.TakeCodeAsync(CodeKind.Authorization, code);
						if (codeAuth != null)
							session = await sessionService.CreateSessionAsync(codeAuth.AccountID, clientID, SessionCreationMethod.AuthorizationCode);
					}
					break;
				case "exchange_code":
					if (code != null)
					{
						var codeExchange = await sessionService.TakeCodeAsync(CodeKind.Exchange, code);
						if (codeExchange != null)
							session = await sessionService.CreateSessionAsync(codeExchange.AccountID, clientID, SessionCreationMethod.ExchangeCode);
					}
					break;
				case "client_credentials":
				{
					// always just userless session, usually used for access to public cloudstorage
					session = await sessionService.CreateSessionAsync(EpicID.Empty, clientID, SessionCreationMethod.ClientCredentials);
					break;
				}
				case "password":
					// NOTE: this grant_type is not recommended anymore: https://oauth.net/2/grant-types/password/
					//       also this: https://stackoverflow.com/questions/62395052/oauth-password-grant-replacement
					//
					//       we could still support it, since ut understands it cuz its old and we don't
					//       really need multi-factor auth. it is after all the way that ut's login screen
					//       works when you start (stock/without UT4UU) game without launcher.

					if (username != null && password != null)
					{
						account = await accountService.GetAccountAsync(username, password);
						if (account != null)
							session = await sessionService.CreateSessionAsync(account.ID, clientID, SessionCreationMethod.Password);
					}
					break;
				default:
					return BadRequest(new ErrorResponse()
					{
						Error = "invalid_grant"
					});
			}

			if (session == null) // only here to prevent null warnings, should never happen
				return BadRequest();

			if (account == null)
				account = await accountService.GetAccountAsync(session.AccountID);
			logger.LogInformation($"User {account} was authorized via {grantType}");

			JObject obj = new JObject();
			obj.Add("access_token", session.AccessToken.Value);
			obj.Add("expires_in", session.AccessToken.ExpirationDurationInSeconds);
			obj.Add("expires_at", session.AccessToken.ExpirationTime.ToStringISO());
			obj.Add("token_type", "bearer");
			if (!session.AccountID.IsEmpty && account != null)
			{
				obj.Add("refresh_token", session.RefreshToken.Value);
				obj.Add("refresh_expires", session.RefreshToken.ExpirationDurationInSeconds);
				obj.Add("refresh_expires_at", session.RefreshToken.ExpirationTime.ToStringISO());
				obj.Add("account_id", account.ID.ToString());
			}
			obj.Add("client_id", session.ClientID.ToString());
			obj.Add("internal_client", false);
			obj.Add("client_service", "ut");
			if (!session.AccountID.IsEmpty && account != null)
			{
				obj.Add("displayName", account.Username);

				if (includePerms == true)
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

				obj.Add("app", "ut");
				obj.Add("in_app_id", account.ID.ToString());
				obj.Add("device_id", "465a117c2b144b5c8222ee71b9bc8da2"); // unsure about this, probably some ip tracking feature
			}
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpGet]
		[Route("exchange")]
		public async Task<ActionResult<string>> CreateExchangeCode(string accessToken)
		{
			var session = await sessionService.GetSessionAsync(accessToken);
			if (session == null)
				return BadRequest(new ErrorResponse()
				{
					Error = "invalid_token" // TODO: find proper response
				});

			var code = await sessionService.CreateCodeAsync(CodeKind.Exchange, session.AccountID, session.ClientID);
			if (code == null)
				return BadRequest(new ErrorResponse()
				{
					Error = "cannot_create_exchangecode" // TODO: find proper response
				});

			var obj = new JObject();
			obj.Add("expiresInSeconds", code.Token.ExpirationDurationInSeconds);
			obj.Add("code", code.Token.Value);
			obj.Add("creatingClientId", code.CreatingClientID.ToString());
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpGet]
		[Route("auth")]
		public async Task<ActionResult<string>> CreateAuthorizationCode([FromForm] string username)
		{
			// this action is originally on "www.epicgames.com/id/api/redirect" + some query specitying client_id and something else, forgot what

			// TODO: inspect http when signing in on website and use client_credentials method instead of this method with custom url

			// TODO: this needs to get account id from authorization header
			var account = await accountService.GetAccountAsync(username);
			if (account == null)
				return NotFound();

			var session = await sessionService.GetSessionAsync(account.ID, ClientIdentification.Launcher.ID);
			if (session == null)
				return NotFound();

			var authCode = await sessionService.CreateCodeAsync(CodeKind.Authorization, session.AccountID, session.ClientID);
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


		[HttpDelete]
		[Route("sessions/kill/{accessToken}")]
		public async Task<NoContentResult> KillSession(string accessToken)
		{
			logger.LogInformation($"Deleted session with token = {Request.Path.Value}");

			var session = await sessionService.GetSessionAsync(accessToken);
			await sessionService.RemoveSessionAsync(session.ID);

			return new NoContentResult();
		}

		[HttpDelete]
		[Route("sessions/kill")]
		public async Task<NoContentResult> KillSession([FromQuery] string killType, string accessToken)
		{
			logger.LogInformation($"Deleted old sessions with token = {Request.Path.Value}");

			if (killType == "OTHERS_ACCOUNT_CLIENT_SERVICE")
			{
				Session? safeSession = await sessionService.GetSessionAsync(accessToken);
				if (safeSession == null)
					return NoContent();

				await sessionService.RemoveOtherSessionsAsync(safeSession.ClientID, safeSession.ID);
			}
			// TODO: find other valid strings

			return new NoContentResult();
		}

		//[HttpPost]
		//[Route("login/account")]
		//public async Task<ActionResult> LoginAccount([FromForm] string username, [FromForm] string password)
		//{
		//	// this action is originally on "www.epicgames.com/id/api/login"
		//	var account = await accountService.GetAccountAsync(username, password);
		//	if (account == null)
		//		return NotFound();

		//	var session = await sessionService.CreateSessionAsync(account.ID, ClientIdentification.Launcher.ID, SessionCreationMethod.ClientCredentials); ;
		//	if (session == null)
		//		return NotFound();

		//	logger.LogInformation($"Created session {session.ID} for user {username}");
		//	return NoContent();
		//}
	}
}
