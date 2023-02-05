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
	private readonly CloudStorageService cloudStorageService;
	private readonly StatisticsService statisticsService;
	private readonly ClientService clientService;
	private readonly TrustedGameServerService trustedGameServerService;

	public AdminPanelController(
		ILogger<AdminPanelController> logger,
		AccountService accountService,
		SessionService sessionService,
		CodeService codeService,
		CloudStorageService cloudStorageService,
		StatisticsService statisticsService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService)
	{
		this.logger = logger;
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.codeService = codeService;
		this.cloudStorageService = cloudStorageService;
		this.statisticsService = statisticsService;
		this.clientService = clientService;
		this.trustedGameServerService = trustedGameServerService;
	}

	[HttpGet("flags")]
	public async Task<IActionResult> GetAllPossibleFlags()
	{
		await VerifyAdmin();

		return Ok(Enum.GetNames<AccountFlags>());
	}

	[HttpGet("flags/{accountID}")]
	public async Task<IActionResult> GetAccountFlags(string accountID)
	{
		await VerifyAdmin();

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
		var admin = await VerifyAdmin();

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

		if (flags.HasFlag(AccountFlags.Admin) && !admin.Account.Flags.HasFlag(AccountFlags.Admin))
			return Unauthorized();

		if (flags.HasFlag(AccountFlags.Moderator) && (!admin.Account.Flags.HasFlag(AccountFlags.Moderator) && !admin.Account.Flags.HasFlag(AccountFlags.Admin)))
			return Unauthorized();

		var account = await accountService.GetAccountAsync(EpicID.FromString(accountID));
		if (account == null)
			return NotFound();

		account.Flags = flags;

		await accountService.UpdateAccountAsync(account);
		return Ok();
	}


	[HttpPost("clients/new")]
	public async Task<IActionResult> CreateClient([FromBody] string name)
	{
		await VerifyAdmin();

		var client = new Client(EpicID.GenerateNew(), EpicID.GenerateNew().ToString(), name);
		await clientService.UpdateAsync(client);

		return Ok(client);
	}

	[HttpGet("clients")]
	public async Task<IActionResult> GetAllClients()
	{
		await VerifyAdmin();

		var clients = await clientService.ListAsync();
		return Ok(clients);
	}

	[HttpGet("clients/{id}")]
	public async Task<IActionResult> GetClient(string id)
	{
		await VerifyAdmin();

		var client = await clientService.GetAsync(EpicID.FromString(id));
		if (client == null)
			return NotFound();

		return Ok(client);
	}

	[HttpPatch("clients/{id}")]
	public async Task<IActionResult> UpdateClient(string id, [FromBody] Client client)
	{
		await VerifyAdmin();

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
		await VerifyAdmin();

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
		await VerifyAdmin();

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
		await VerifyAdmin();

		var ret = await trustedGameServerService.GetAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPost("trusted_servers")]
	public async Task<IActionResult> CreateTrustedServer([FromBody] TrustedGameServer server)
	{
		await VerifyAdmin();
		// TODO: validate server.ID is valid Client ID and not already in use and owner ID is a valid Account ID and has HubOwner flag

		await trustedGameServerService.UpdateAsync(server);
		return Ok();
	}

	[HttpPatch("trusted_servers/{id}")]
	public async Task<IActionResult> UpdateTrustedServer(string id, [FromBody] TrustedGameServer server)
	{
		await VerifyAdmin();

		var eid = EpicID.FromString(id);

		if (eid != server.ID)
			return BadRequest();

		await trustedGameServerService.UpdateAsync(server);
		return Ok();
	}

	[HttpDelete("trusted_servers/{id}")]
	public async Task<IActionResult> DeleteTrustedServer(string id)
	{
		await VerifyAdmin();

		var ret = await trustedGameServerService.RemoveAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPatch("change_password/{id}")]
	public async Task<IActionResult> ChangePassword(string id, [FromBody] AdminPanelChangePasswordRequest body)
	{
		await VerifyAdmin();

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

		await accountService.UpdateAccountPasswordAsync(account, body.NewPassword);

		// logout user to make sure they remember they changed password by being forced to log in again,
		// as well as prevent anyone else from using this account after successful password change.
		await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, account.ID, EpicID.Empty);

		logger.LogInformation("Updated password for {AccountID}", account.ID);

		return Ok();
	}

	[HttpGet("mcp_files")]
	public async Task<IActionResult> GetMCPFiles()
	{
		await VerifyAdmin();
		return Ok(await cloudStorageService.ListFilesAsync(EpicID.Empty));
	}

	[HttpPost("mcp_files/{filename}")]
	[HttpPatch("mcp_files/{filename}")]
	public async Task<IActionResult> UpdateMCPFile(string filename)
	{
		await VerifyAdmin();
		await cloudStorageService.UpdateFileAsync(EpicID.Empty, filename, HttpContext.Request.Body);
		return Ok();
	}

	[HttpGet("mcp_files/{filename}")]
	public async Task<IActionResult> GetMCPFile(string filename)
	{
		await VerifyAdmin();

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
		await VerifyAdmin();
		await cloudStorageService.DeleteFileAsync(EpicID.Empty, filename);
		return Ok();
	}

	[HttpDelete("account/{id}")]
	public async Task<IActionResult> DeleteAccountInfo(string id, [FromBody] bool? forceCheckBroken)
	{
		var admin = await VerifyAdmin();

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
		await codeService.RemoveCodesByAccountAsync(accountID);
		await cloudStorageService.RemoveFilesByAccountAsync(accountID);
		await statisticsService.RemoveStatisticsByAccountAsync(accountID);

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
	private async Task<(Session Session, Account Account)> VerifyAdmin()
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
