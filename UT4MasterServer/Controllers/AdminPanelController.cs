using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Helpers;
using UT4MasterServer.Models;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("admin")]
[AuthorizeBearer]
public sealed class AdminPanelController : ControllerBase
{
	private readonly ILogger<AdminPanelController> logger;
	private readonly AccountService accountService;
	private readonly SessionService sessionService;
	private readonly CloudStorageService cloudStorageService;
	private readonly ClientService clientService;
	private readonly TrustedGameServerService trustedGameServerService;

	public AdminPanelController(
		ILogger<AdminPanelController> logger,
		AccountService accountService,
		SessionService sessionService,
		CloudStorageService cloudStorageService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService)
	{
		this.logger = logger;
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.cloudStorageService = cloudStorageService;
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

		for (int i = 0; i < flagNames.Length; i++)
		{
			if (flagNamesAll.Contains(flagNames[i]))
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
	public async Task<IActionResult> CreateClient()
	{
		await VerifyAdmin();

		var client = new Client(EpicID.GenerateNew(), EpicID.GenerateNew().ToString());
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

		var ret = await trustedGameServerService.ListAsync();
		return Ok(ret);
	}

	[HttpGet("trusted_servers/{id}")]
	public async Task<IActionResult> GetTrustedServer(string id)
	{
		await VerifyAdmin();

		var ret = await trustedGameServerService.GetAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPatch("trusted_servers/{id}")]
	[HttpPost("trusted_servers/{id}")]
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
	public async Task<IActionResult> ChangePassword(string id, [FromBody] string newPassword, [FromBody] bool? iAmSure)
	{
		await VerifyAdmin();

		var account = await accountService.GetAccountAsync(EpicID.FromString(id));
		if (account is null)
			return NotFound(new ErrorResponse() { ErrorMessage = $"Failed to find account {id}" });

		if (account.Flags.HasFlag(AccountFlags.Moderator) || account.Flags.HasFlag(AccountFlags.Admin))
			throw new UnauthorizedAccessException("Cannot change password of other admins or moderators");

		// passwords should already be hashed, but check its length just in case
		if (!ValidationHelper.ValidatePassword(newPassword))
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"newPassword is not a SHA512 hash"
			});
		}

		if (iAmSure != true)
		{
			return BadRequest(new ErrorResponse()
			{
				ErrorMessage = $"'areYouSure' was not 'true'"
			});
		}

		await accountService.UpdateAccountPasswordAsync(account, newPassword);

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
		await cloudStorageService.UpdateFileAsync(EpicID.Empty, filename, HttpContext.Request.BodyReader);
		return Ok();
	}

	[HttpGet("mcp_files/{filename}")]
	public async Task<IActionResult> GetMCPFile(string filename)
	{
		await VerifyAdmin();

		var file = await cloudStorageService.GetFileAsync(EpicID.Empty, filename);
		if (file is null)
			return NotFound(new ErrorResponse() { ErrorMessage = "File not found" });

		return new FileContentResult(file.RawContent, "application/octet-stream");
	}



	[NonAction]
	private bool IsSpecialClientID(EpicID id)
	{
		if (id == ClientIdentification.Game.ID || id == ClientIdentification.ServerInstance.ID || id == ClientIdentification.Launcher.ID)
			return true;

		return false;
	}

	[NonAction]
	private async Task<(Session Session, Account Account)> VerifyAdmin()
	{
		if (User.Identity is not EpicUserIdentity user)
			throw new UnauthorizedAccessException("User not logged in");

		var account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
			throw new UnauthorizedAccessException("User not found");

		if (!account.Flags.HasFlag(AccountFlags.Admin) && !account.Flags.HasFlag(AccountFlags.Moderator))
			throw new UnauthorizedAccessException("User has insufficient privileges");

		return (user.Session, account);
	}
}
