using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers
{
	[ApiController]
	[Route("account/api/oauth")]
	[AuthorizeBearer]
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

		[HttpPost("token")]
		[AuthorizeBasic]
		public async Task<ActionResult<string>> Authenticate(
			[FromForm(Name = "grant_type")] string grantType,
			[FromForm(Name = "includePerms")] bool? includePerms,
			[FromForm(Name = "code")] string? code,
			[FromForm(Name = "username")] string? username,
			[FromForm(Name = "password")] string? password)
		{
			if (User.Identity is not EpicClientIdentity user)
				return Unauthorized();

			EpicID clientID = user.Client.ID;
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
					//       works when you start the game without launcher (and without UT4UU).

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
					// the actual response can be found in perms.json file in original HttpListener example of this project
				}

				obj.Add("app", "ut");
				obj.Add("in_app_id", account.ID.ToString());
				obj.Add("device_id", "465a117c2b144b5c8222ee71b9bc8da2"); // unsure about this, probably some ip tracking feature
			}
			return obj.ToString(Newtonsoft.Json.Formatting.None);
		}

		[HttpGet("exchange")]
		public async Task<ActionResult<string>> CreateExchangeCode()
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var code = await sessionService.CreateCodeAsync(CodeKind.Exchange, user.Session.AccountID, user.Session.ClientID);
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

		[HttpGet("auth")] // this action is originally on "www.epicgames.com/id/api/redirect" + some query specifying client_id and something else, forgot what
		public async Task<ActionResult<string>> CreateAuthorizationCode()
		{
			if (User.Identity is not EpicUserIdentity user)
				return Unauthorized();

			var authCode = await sessionService.CreateCodeAsync(CodeKind.Authorization, user.Session.AccountID, user.Session.ClientID);
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


		[HttpDelete("sessions/kill/{accessToken}")] // we dont need to use token in url because we have it in 
		public async Task<NoContentResult> KillSession(string accessToken)
		{
			if (User.Identity is not EpicUserIdentity user)
				return NoContent();

			if (accessToken != user.AccessToken)
			{
				logger.LogInformation($"In request to kill session {user.Session.ID}, token in url didnt match the one in header. killing session anyway...");
			}

			await sessionService.RemoveSessionAsync(user.Session.ID);

			logger.LogInformation($"Deleted session '{user.Session.ID}'");
			return NoContent();
		}

		[HttpDelete("sessions/kill")]
		public async Task<NoContentResult> KillSessions([FromQuery] string killType)
		{
			if (User.Identity is not EpicUserIdentity user)
				return NoContent();

			if (killType == "OTHERS_ACCOUNT_CLIENT_SERVICE")
			{
				await sessionService.RemoveOtherSessionsAsync(user.Session.ClientID, user.Session.ID);
				logger.LogInformation($"Deleted all sessions in client '{user.Session.ClientID}', except '{user.Session.ID}'");
			}
			// TODO: find other valid strings

			return NoContent();
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
