using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer.Controllers.Epic;

/// <summary>
/// account-public-service-prod03.ol.epicgames.com
/// </summary>
[ApiController]
[Route("account/api")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class AccountController : JsonAPIController
{
	private readonly SessionService sessionService;
	private readonly AccountService accountService;
	private readonly RateLimitService rateLimitService;
	private readonly IOptions<ApplicationSettings>? applicationSettings;
	private readonly IOptions<ReCaptchaSettings> reCaptchaSettings;

	public AccountController(
		ILogger<AccountController> logger,
		AccountService accountService,
		SessionService sessionService,
		RateLimitService rateLimitService,
		IOptions<ApplicationSettings> applicationSettings,
		IOptions<ReCaptchaSettings> reCaptchaSettings) : base(logger)
	{
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.rateLimitService = rateLimitService;
		this.applicationSettings = applicationSettings;
		this.reCaptchaSettings = reCaptchaSettings;
	}

	#region ACCOUNT LISTING API

	[HttpGet("public/account/{id}")]
	public async Task<IActionResult> GetAccount(string id)
	{
		if (User.Identity is not EpicUserIdentity authenticatedUser)
			return Unauthorized();

		// TODO: EPIC doesn't throw here if id is invalid (like 'abc'). Return this same ErrorResponse like for account_not_found
		EpicID eid = EpicID.FromString(id);

		if (eid != authenticatedUser.Session.AccountID)
			return Unauthorized();

		logger.LogInformation($"{authenticatedUser.Session.AccountID} is looking for account {id}");

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
		obj.Add("email", account.Email);//$"{account.ID}@{Request.Host}"); // fake a random one
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
		obj.Add("emailVerified", false);//true);
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
    public async Task<IActionResult> RegisterAccount([FromForm] string username, [FromForm] string email, [FromForm] string password, [FromForm] string? recaptchaToken)
    {
        var reCaptchaSecret = reCaptchaSettings.Value.SecretKey;
		if (!string.IsNullOrWhiteSpace(reCaptchaSecret))
		{
			if (recaptchaToken is null)
			{
				return Conflict("Recaptcha token is missing");
			}

			var httpClient = new HttpClient();
			var httpResponse = await httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={reCaptchaSecret}&response={recaptchaToken}");
			if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
			{
				return Conflict("Recaptcha validation failed");
			}

			var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
			var jsonData = JObject.Parse(jsonResponse);
			if (jsonData["success"]?.ToObject<bool>() != true)
			{
				return Conflict("Recaptcha validation failed");
			}
		}

		var account = await accountService.GetAccountAsync(username);
		if (account != null)
		{
			logger.LogInformation("Could not register duplicate account: {Username}.", username);
			return Conflict("Username already exists");
		}

		if (!ValidationHelper.ValidateUsername(username))
		{
			logger.LogInformation("Entered an invalid username: {Username}.", username);
			return BadRequest("You have entered an invalid username");
		}

		email = email.ToLower();
		account = await accountService.GetAccountByEmailAsync(email);
		if (account != null)
		{
			logger.LogInformation("Entered an existing email: {Email}.", email);
			return Conflict("Email already exists");
		}

		if (!ValidationHelper.ValidateEmail(email))
		{
			logger.LogInformation("Entered an invalid email: {Email}.", email);
			return BadRequest("You have entered an invalid email address");
		}

		if (!ValidationHelper.ValidatePassword(password))
		{
			logger.LogInformation("Entered an invalid password.");
			return BadRequest("You have entered an invalid password");
		}

		await accountService.CreateAccountAsync(username, email, password); // TODO: this cannot fail?


		logger.LogInformation("Registered new user: {Username}.", username);

		return Ok("Account created successfully");
	}

	[HttpPatch("update/username")]
	public async Task<IActionResult> UpdateUsername([FromForm] string newUsername)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		if (!ValidationHelper.ValidateUsername(newUsername))
		{
			return ValidationProblem();
		}

		var matchingAccount = await accountService.GetAccountAsync(newUsername);
		if (matchingAccount != null)
		{
			logger.LogInformation($"Change Username failed, already taken: {newUsername}");
			return Conflict(new ErrorResponse()
			{
				ErrorMessage = $"Username already taken"
			});
		}

		var account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			return NotFound(new ErrorResponse()
			{
				ErrorMessage = $"Failed to retrieve your account"
			});
		}

		account.Username = newUsername;
		await accountService.UpdateAccountAsync(account);

		logger.LogInformation($"Updated username for {user.Session.AccountID} to: {newUsername}");

		return Ok("Changed username successfully");
	}

	[HttpPatch("update/email")]
	public async Task<IActionResult> UpdateEmail([FromForm] string newEmail)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		newEmail = newEmail.ToLower();
		if (!ValidationHelper.ValidateEmail(newEmail))
		{
			return ValidationProblem();
		}

		var account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			return NotFound(new ErrorResponse()
			{
				ErrorMessage = $"Failed to retrieve your account"
			});
		}

		account.Email = newEmail;
		await accountService.UpdateAccountAsync(account);

		logger.LogInformation($"Updated email for {user.Session.AccountID} to: {newEmail}");

		return Ok("Changed email successfully");
	}

	[HttpPatch("update/password")]
	public async Task<IActionResult> UpdatePassword([FromForm] string currentPassword, [FromForm] string newPassword)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			throw new UnauthorizedAccessException();
		}

		if (user.Session.ClientID != ClientIdentification.Launcher.ID)
		{
			throw new UnauthorizedAccessException("Password can only be changed from the website");
		}

		// passwords should already be hashed, but check its length just in case
		if (!ValidationHelper.ValidatePassword(newPassword))
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"newPassword is not a SHA512 hash"
			});
		}

		var account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			return NotFound(new ErrorResponse()
			{
				ErrorMessage = $"Failed to retrieve your account"
			});
		}

		if (!account.CheckPassword(currentPassword, false))
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"Current Password is invalid"
			});
		}

		await accountService.UpdateAccountPasswordAsync(account.ID, newPassword);

		// logout user to make sure they remember they changed password by being forced to log in again,
		// as well as prevent anyone else from using this account after successful password change.
		await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, user.Session.AccountID, EpicID.Empty);

		logger.LogInformation($"Updated password for {user.Session.AccountID}");

		return Ok("Changed password successfully");
	}

	[AllowAnonymous]
	[HttpPost("verify-email")]
	public async Task<IActionResult> VerifyEmail([FromForm] string accountID, [FromForm] string guid)
	{
		EpicID eid = EpicID.FromString(accountID);
		await accountService.VerifyEmailAsync(eid, guid);
		return Ok();
	}

	[AllowAnonymous]
	[HttpPost("resend-verification-link")]
	public async Task<IActionResult> ResendVerificationLink([FromForm] string email)
	{
		var clientIpAddress = GetClientIP(applicationSettings);
		if (clientIpAddress == null)
		{
			logger.LogError("Could not determine IP Address of remote machine.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		rateLimitService.CheckRateLimit($"{nameof(ResendVerificationLink)}-{clientIpAddress}");

		await accountService.ResendVerificationLinkAsync(email);
		return Ok();
	}

	[AllowAnonymous]
	[HttpPost("initiate-reset-password")]
	public async Task<IActionResult> InitiateResetPassword([FromForm] string email)
	{
		var clientIpAddress = GetClientIP(applicationSettings);
		if (clientIpAddress == null)
		{
			logger.LogError("Could not determine IP Address of remote machine.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		rateLimitService.CheckRateLimit($"{nameof(InitiateResetPassword)}-{clientIpAddress}");

		await accountService.InitiateResetPasswordAsync(email);
		return Ok();
	}

	[AllowAnonymous]
	[HttpPost("reset-password")]
	public async Task<IActionResult> ResetPassword([FromForm] string accountID, [FromForm] string guid, [FromForm] string newPassword)
	{
		EpicID eid = EpicID.FromString(accountID);
		await accountService.ResetPasswordAsync(eid, guid, newPassword);
		return Ok();
	}

	#endregion
}
