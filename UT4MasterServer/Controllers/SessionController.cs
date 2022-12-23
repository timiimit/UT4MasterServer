using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[AuthorizeBearer]
[Route("account/api/oauth")]
[Produces("application/json")]
public class SessionController : JsonAPIController
{
	private readonly ILogger<SessionController> logger;
	private readonly AccountService accountService;
	private readonly SessionService sessionService;
	private readonly CodeService codeService;
	private readonly bool allowPasswordGrant;

	public SessionController(
		SessionService sessionService, CodeService codeService, AccountService accountService,
		IOptions<DatabaseSettings> settings, ILogger<SessionController> logger)
	{
		this.codeService = codeService;
		this.sessionService = sessionService;
		this.accountService = accountService;
		this.logger = logger;
		allowPasswordGrant = settings.Value.AllowPasswordGrantType;
	}

	[AuthorizeBasic]
	[HttpPost("token")]
	public async Task<IActionResult> Authenticate(
		[FromForm(Name = "grant_type")] string grantType,
		[FromForm(Name = "includePerms")] bool? includePerms,
		[FromForm(Name = "code")] string? code,
		[FromForm(Name = "exchange_code")] string? exchangeCode,
		[FromForm(Name = "refresh_token")] string? refreshToken,
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
			{
				if (code != null)
				{
					var codeAuth = await codeService.TakeCodeAsync(CodeKind.Authorization, code);
					if (codeAuth != null)
						session = await sessionService.CreateSessionAsync(codeAuth.AccountID, clientID, SessionCreationMethod.AuthorizationCode);
				}
				break;
			}
			case "exchange_code":
			{
				if (exchangeCode != null)
				{
					var codeExchange = await codeService.TakeCodeAsync(CodeKind.Exchange, exchangeCode);
					if (codeExchange != null)
						session = await sessionService.CreateSessionAsync(codeExchange.AccountID, clientID, SessionCreationMethod.ExchangeCode);
				}
				break;
			}
			case "refresh_token":
			{
				if (refreshToken != null)
				{
					session = await sessionService.RefreshSessionAsync(refreshToken);
				}
				break;
			}
			case "client_credentials":
			{
				// always just userless session, usually used for access to public cloudstorage
				session = await sessionService.CreateSessionAsync(EpicID.Empty, clientID, SessionCreationMethod.ClientCredentials);
				break;
			}
			case "password":
			{
				if (!allowPasswordGrant)
					break;

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
			}
			default:
			{
				return BadRequest(new ErrorResponse()
				{
					Error = "invalid_grant"
				});
			}
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
		return Json(obj);
	}

	[HttpGet("exchange")]
	public async Task<IActionResult> CreateExchangeCode()
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var code = await codeService.CreateCodeAsync(CodeKind.Exchange, user.Session.AccountID, user.Session.ClientID);
		if (code == null)
			return BadRequest(new ErrorResponse()
			{
				Error = "cannot_create_exchangecode" // TODO: find proper response
			});

		var obj = new JObject();
		obj.Add("expiresInSeconds", code.Token.ExpirationDurationInSeconds);
		obj.Add("code", code.Token.Value);
		obj.Add("creatingClientId", code.CreatingClientID.ToString());
		return Json(obj);
	}

	[HttpGet("auth")] // this action is originally on "www.epicgames.com/id/api/redirect" + some query specifying client_id and something else, forgot what
	public async Task<IActionResult> CreateAuthorizationCode()
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		var authCode = await codeService.CreateCodeAsync(CodeKind.Authorization, user.Session.AccountID, user.Session.ClientID);
		if (authCode == null)
			return BadRequest();

		logger.LogInformation($"Created authorization code: {authCode.Token}");

		// this is epic's response when you are logged in.
		// when you are not logged in, sid is set, and authorizationCode is null (or maybe just empty?)
		var obj = new JObject();
		obj.Add("redirectUrl", $"https://localhost/launcher/authorized?code={authCode.Token}");
		obj.Add("authorizationCode", authCode.Token.ToString());
		obj.Add("sid", null);
		return Json(obj);
	}


	[HttpDelete("sessions/kill/{accessToken}")] // we dont need to use token in url because we have it in 
	public async Task<IActionResult> KillSession(string accessToken)
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
	public async Task<IActionResult> KillSessions([FromQuery] string killType)
	{
		if (User.Identity is not EpicUserIdentity user)
			return NoContent();

		if (killType == "ALL")
		{
			await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, user.Session.AccountID, EpicID.Empty);
		}
		else if (killType == "OTHERS")
		{
			await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, user.Session.AccountID, user.Session.ID);
		}
		else if (killType == "ALL_ACCOUNT_CLIENT")
		{
			await sessionService.RemoveSessionsWithFilterAsync(user.Session.ClientID, user.Session.AccountID, EpicID.Empty);
		}
		else if (killType == "OTHERS_ACCOUNT_CLIENT")
		{
			await sessionService.RemoveSessionsWithFilterAsync(user.Session.ClientID, user.Session.AccountID, user.Session.ID);
		}
		else if (killType == "OTHERS_ACCOUNT_CLIENT_SERVICE")
		{
			// i am not sure how this is supposed to differ from OTHERS_ACCOUNT_CLIENT
			// perhaps service as in epic games launcher and/or website?
			await sessionService.RemoveSessionsWithFilterAsync(user.Session.ClientID, user.Session.AccountID, user.Session.ID);
		}

		return NoContent();
	}
  
	// TODO: Make sure this does what it's supposed to. 200 OK should be enough for the client to know the session is still valid.
	[HttpGet]
	[Route("verify")]
	public async Task<IActionResult> Verify()
	{
		if (User.Identity is not EpicUserIdentity user)
			return NoContent();

		// no refresh needed, but we should respond with this:
		/*
		
		{
			"token": "06577db463064b89af0657b5445b08b7",
			"session_id": "06577db463064b89af0657b5445b08b7",
			"token_type": "bearer",
			"client_id": "1252412dc7704a9690f6ea4611bc81ee",
			"internal_client": false,
			"client_service": "ut",
			"account_id": "64bf8c6d81004e88823d577abe157373",
			"expires_in": 28799,
			"expires_at": "2022-12-20T23:26:25.049Z",
			"auth_method": "exchange_code",
			"display_name": "norandomemails",
			"app": "ut",
			"in_app_id": "64bf8c6d81004e88823d577abe157373",
			"device_id": "ee64ee5f292b45f089a368cb7e43d82d",
			"perms": [...]
		}

		*/

		string auth_method = user.Session.CreationMethod switch
		{
			SessionCreationMethod.AuthorizationCode => "authorization_code",
			SessionCreationMethod.ExchangeCode => "exchange_code",
			SessionCreationMethod.ClientCredentials => "client_credentials",
			SessionCreationMethod.Password => "password",
			_ => throw new NotImplementedException(),
		};

		var account = await accountService.GetAccountAsync(user.Session.AccountID);

		var obj = new JObject()
		{
			{ "token", user.AccessToken },
			{ "session_id", user.Session.ID.ToString() },
			{ "token_type", "bearer" },
			{ "client_id", user.Session.ClientID.ToString() },
			{ "internal_client", false },
			{ "client_service", "ut" },
			{ "account_id", user.Session.AccountID.ToString() },
			{ "expires_in", user.Session.AccessToken.ExpirationDurationInSeconds },
			{ "expires_at", user.Session.AccessToken.ExpirationTime.ToStringISO() },
			{ "auth_method", auth_method },
			{ "display_name", account?.Username },
			{ "app", "ut" },
			{ "in_app_id", user.Session.AccountID.ToString() },
			{ "device_id", "ee64ee5f292b45f089a368cb7e43d82d" }, // TODO: figure out proper handling of device id
			{ "perms", new JArray() }, // TODO: none for now
		};

		return Json(obj);
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
