using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Request;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Services.Singleton;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models;
using UT4MasterServer.Models.Responses;
using Microsoft.Net.Http.Headers;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("admin")]
[AuthorizeBearer]
public sealed class AdminPanelController : ControllerBase
{
	private readonly ILogger<AdminPanelController> logger;
	private readonly AccountService accountService;
	private readonly SessionService sessionService;
	private readonly CodeService codeService;
	private readonly FriendService friendService;
	private readonly CloudStorageService cloudStorageService;
	private readonly StatisticsService statisticsService;
	private readonly ClientService clientService;
	private readonly TrustedGameServerService trustedGameServerService;
	private readonly RatingsService ratingsService;

	public AdminPanelController(
		ILogger<AdminPanelController> logger,
		AccountService accountService,
		SessionService sessionService,
		CodeService codeService,
		FriendService friendService,
		CloudStorageService cloudStorageService,
		StatisticsService statisticsService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService,
		RatingsService ratingsService)
	{
		this.logger = logger;
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.codeService = codeService;
		this.friendService = friendService;
		this.cloudStorageService = cloudStorageService;
		this.statisticsService = statisticsService;
		this.clientService = clientService;
		this.trustedGameServerService = trustedGameServerService;
		this.ratingsService = ratingsService;
	}

	[HttpGet("flags")]
	public async Task<IActionResult> GetAllPossibleFlags()
	{
		await VerifyAdminAsync();

		return Ok(Enum.GetNames<AccountFlags>());
	}

	[HttpGet("flags/{accountID}")]
	public async Task<IActionResult> GetAccountFlags(string accountID)
	{
		await VerifyAdminAsync();

		var account = await accountService.GetAccountAsync(EpicID.FromString(accountID));
		if (account == null)
			return NotFound();

		var result = new List<string>();

		var flagNamesAll = Enum.GetNames<AccountFlags>();
		var flagValuesAll = Enum.GetValues<AccountFlags>();

		for (int i = 0; i < flagNamesAll.Length; i++)
		{
			if (account.Flags.HasFlag(flagValuesAll[i]))
			{
				result.Add(flagNamesAll[i]);
			}
		}

		return Ok(result);
	}

	[HttpPut("flags/{accountID}")]
	public async Task<IActionResult> SetAccountFlags(string accountID, [FromBody] string[] flagNames)
	{
		var admin = await VerifyAdminAsync();

		// TODO: duplicate code in PersonaController
		var flagNamesAll = Enum.GetNames<AccountFlags>();
		var flagValuesAll = Enum.GetValues<AccountFlags>();

		AccountFlags flags = AccountFlags.None;

		for (int i = 0; i < flagNamesAll.Length; i++)
		{
			if (flagNames.Contains(flagNamesAll[i]))
			{
				flags |= flagValuesAll[i];
			}
		}


		var account = await accountService.GetAccountAsync(EpicID.FromString(accountID));
		if (account == null)
			return NotFound();

		var flagsOld = account.Flags;
		var adminFlags = admin.Account.Flags;

		// verify that user is authorized to edit specified flags
		if (adminFlags.HasFlag(AccountFlags.Admin))
		{
			if (flagsOld.HasFlag(AccountFlags.Admin) && !flags.HasFlag(AccountFlags.Admin))
			{
				return Unauthorized("Cannot remove Admin flag, this action must be performed with direct access to database");
			}
		}
		else
		{
			if (flags.HasFlag(AccountFlags.Admin))
			{
				return Unauthorized("Only Admin may add Admin flag to account");
			}

			if (flagsOld.HasFlag(AccountFlags.Admin) && !flags.HasFlag(AccountFlags.Admin))
			{
				logger.LogWarning("Suspicius activity by {User}. Tried to remove Admin privilege of {Admin}.", admin.Account, account);
				return Unauthorized("Cannot remove Admin flag as a non-Admin");
			}

			if (flagsOld.HasFlag(AccountFlags.Moderator) && !flags.HasFlag(AccountFlags.Moderator))
			{
				return Unauthorized("Only an Admin may remove Moderator flag");
			}
		}

		await accountService.UpdateAccountFlagsAsync(account.ID, flags);
		return Ok();
	}


	[HttpPost("clients/new")]
	public async Task<IActionResult> CreateClient([FromBody] string name)
	{
		await VerifyAdminAsync();

		var client = new Client(EpicID.GenerateNew(), EpicID.GenerateNew().ToString(), name);
		await clientService.UpdateAsync(client);

		return Ok(client);
	}

	[HttpGet("clients")]
	public async Task<IActionResult> GetAllClients()
	{
		await VerifyAdminAsync();

		var clients = await clientService.ListAsync();
		return Ok(clients);
	}

	[HttpGet("clients/{id}")]
	public async Task<IActionResult> GetClient(string id)
	{
		await VerifyAdminAsync();

		var client = await clientService.GetAsync(EpicID.FromString(id));
		if (client == null)
			return NotFound();

		return Ok(client);
	}

	[HttpPatch("clients/{id}")]
	public async Task<IActionResult> UpdateClient(string id, [FromBody] Client client)
	{
		await VerifyAdminAsync();

		var eid = EpicID.FromString(id);

		if (eid != client.ID)
			return BadRequest();

		if (IsSpecialClientID(eid))
			return Unauthorized();

		if (string.IsNullOrWhiteSpace(client.Name))
			client.Name = null;

		await clientService.UpdateAsync(client);
		return Ok();
	}

	[HttpDelete("clients/{id}")]
	public async Task<IActionResult> DeleteClient(string id)
	{
		await VerifyAdminAsync();

		var eid = EpicID.FromString(id);

		if (IsSpecialClientID(eid))
			return Unauthorized();

		var success = await clientService.RemoveAsync(eid);
		if (success == null || success == false)
			return BadRequest();

		// in case this client is a trusted server remove it as well
		await trustedGameServerService.RemoveAsync(eid);

		return Ok();
	}

	[HttpGet("trusted_servers")]
	public async Task<IActionResult> GetAllTrustedServers()
	{
		await VerifyAdminAsync();

		var getTrustedServers = trustedGameServerService.ListAsync();
		var getClients = clientService.ListAsync();
		Task.WaitAll(getTrustedServers, getClients);
		var trustedServers = getTrustedServers.Result;
		var clients = getClients.Result;
		var eIds = trustedServers.Select((t) => t.OwnerID).Distinct();
		var accounts = await accountService.GetAccountsAsync(eIds);
		var response = trustedServers.Select((t) => new TrustedGameServerResponse
		{
			ID = t.ID,
			OwnerID = t.OwnerID,
			TrustLevel = t.TrustLevel,
			Client = clients.SingleOrDefault((c) => c.ID == t.ID),
			Owner = accounts.SingleOrDefault((a) => a.ID == t.OwnerID)
		});
		return Ok(response);
	}

	[HttpGet("trusted_servers/{id}")]
	public async Task<IActionResult> GetTrustedServer(string id)
	{
		await VerifyAdminAsync();

		var ret = await trustedGameServerService.GetAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPost("trusted_servers")]
	public async Task<IActionResult> CreateTrustedServer([FromBody] TrustedGameServer body)
	{
		await VerifyAdminAsync();

		var client = await clientService.GetAsync(body.ID);
		if (client is null)
		{
			return BadRequest("Trusted server does not have a matching client with same ID");
		}

		var server = await trustedGameServerService.GetAsync(body.ID);
		if (server is not null)
		{
			return BadRequest($"Trusted server {body.ID} already exists");
		}

		var owner = await accountService.GetAccountAsync(body.OwnerID);
		if (owner is null)
		{
			return BadRequest($"OwnerID {body.OwnerID} is not a valid account ID");
		}

		if (owner.Flags.HasFlag(AccountFlags.HubOwner))
		{
			return BadRequest($"Account specified with OwnerID {body.OwnerID} is not marked as HubOwner");
		}

		await trustedGameServerService.UpdateAsync(body);
		return Ok();
	}

	[HttpPatch("trusted_servers/{id}")]
	public async Task<IActionResult> UpdateTrustedServer(string id, [FromBody] TrustedGameServer server)
	{
		await VerifyAdminAsync();

		var eid = EpicID.FromString(id);

		if (eid != server.ID)
			return BadRequest();

		await trustedGameServerService.UpdateAsync(server);



		return Ok();
	}

	[HttpDelete("trusted_servers/{id}")]
	public async Task<IActionResult> DeleteTrustedServer(string id)
	{
		await VerifyAdminAsync();

		var ret = await trustedGameServerService.RemoveAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPatch("change_password/{id}")]
	public async Task<IActionResult> ChangePassword(string id, [FromBody] AdminPanelChangePasswordRequest body)
	{
		await VerifyAdminAsync();

		var account = await accountService.GetAccountAsync(EpicID.FromString(id));
		if (account is null)
		{
			return NotFound(new ErrorResponse() { ErrorMessage = $"Failed to find account {id}" });
		}

		if (account.Flags.HasFlag(AccountFlags.Moderator) || account.Flags.HasFlag(AccountFlags.Admin))
		{
			throw new UnauthorizedAccessException("Cannot change password of other admins or moderators");
		}

		// passwords should already be hashed, but check its length just in case
		if (!ValidationHelper.ValidatePassword(body.NewPassword))
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"newPassword is not a SHA512 hash"
			});
		}

		if (body.IAmSure != true)
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"'iAmSure' was not 'true'"
			});
		}

		await accountService.UpdateAccountPasswordAsync(account.ID, body.NewPassword);

		// logout user to make sure they remember they changed password by being forced to log in again,
		// as well as prevent anyone else from using this account after successful password change.
		await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, account.ID, EpicID.Empty);

		logger.LogInformation("Updated password for {AccountID}", account.ID);

		return Ok();
	}

	[HttpGet("mcp_files")]
	public async Task<IActionResult> GetMCPFiles()
	{
		await VerifyAdminAsync();
		return Ok(await cloudStorageService.ListFilesAsync(EpicID.Empty, false));
	}

	[HttpPost("mcp_files")]
	public async Task<IActionResult> UpdateMCPFile()
	{
		await VerifyAdminAsync();
		var formCollection = await Request.ReadFormAsync();
		var file = formCollection.Files.First();
		var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
		if (file.Length > 0)
		{
			using (var stream = file.OpenReadStream())
			{
				await cloudStorageService.UpdateFileAsync(EpicID.Empty, filename, stream);
			}
			return Ok();
		}
		else
		{
			return BadRequest();
		}
	}

	[HttpGet("mcp_files/{filename}"), Produces("application/octet-stream")]
	public async Task<IActionResult> GetMCPFile(string filename)
	{
		await VerifyAdminAsync();

		var file = await cloudStorageService.GetFileAsync(EpicID.Empty, filename);
		if (file is null)
		{
			return NotFound(new ErrorResponse() { ErrorMessage = "File not found" });
		}

		return new FileContentResult(file.RawContent, "application/octet-stream");
	}

	[HttpDelete("mcp_files/{filename}")]
	public async Task<IActionResult> DeleteMCPFile(string filename)
	{
		await VerifyAdminAsync();
		await cloudStorageService.DeleteFileAsync(EpicID.Empty, filename);
		return Ok();
	}

	[HttpDelete("account/{id}")]
	public async Task<IActionResult> DeleteAccountInfo(string id, [FromBody] bool? forceCheckBroken)
	{
		var admin = await VerifyAdminAsync();

		var accountID = EpicID.FromString(id);
		var account = await accountService.GetAccountAsync(accountID);
		if (account is null)
		{
			if (forceCheckBroken != true)
			{
				return NotFound(new ErrorResponse() { ErrorMessage = "Account not found" });
			}
		}
		else
		{
			if (admin.Account.Flags.HasFlag(AccountFlags.Admin) && account.Flags.HasFlag(AccountFlags.Admin))
			{
				throw new UnauthorizedAccessException("Cannot delete account of other admin");
			}

			if (admin.Account.Flags.HasFlag(AccountFlags.Moderator) &&
				(account.Flags.HasFlag(AccountFlags.Admin) || account.Flags.HasFlag(AccountFlags.Moderator)))
			{
				throw new UnauthorizedAccessException("Cannot delete account of other admin or moderator");
			}

			await accountService.RemoveAccountAsync(account.ID);
		}

		// remove all associated data
		await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, accountID, EpicID.Empty);
		await codeService.RemoveAllByAccountAsync(accountID);
		await cloudStorageService.RemoveAllByAccountAsync(accountID);
		await statisticsService.RemoveAllByAccountAsync(accountID);
		await ratingsService.RemoveAllByAccountAsync(accountID);
		await friendService.RemoveAllByAccountAsync(accountID);
		await trustedGameServerService.RemoveAllByAccountAsync(accountID);
		// NOTE: missing removal of account from live servers. this should take care of itself in a relatively short time.

		return Ok();
	}


	[NonAction]
	private bool IsSpecialClientID(EpicID id)
	{
		if (id == ClientIdentification.Game.ID || id == ClientIdentification.ServerInstance.ID || id == ClientIdentification.Launcher.ID)
		{
			return true;
		}

		return false;
	}

	[NonAction]
	private async Task<(Session Session, Account Account)> VerifyAdminAsync()
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			throw new UnauthorizedAccessException("User not logged in");
		}

		var account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			throw new UnauthorizedAccessException("User not found");
		}

		if (!account.Flags.HasFlag(AccountFlags.Admin) && !account.Flags.HasFlag(AccountFlags.Moderator))
		{
			throw new UnauthorizedAccessException("User has insufficient privileges");
		}

		return (user.Session, account);
	}
}
