using Microsoft.AspNetCore.Mvc;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Other;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[Route("admin")]
[AuthorizeBearer]
public sealed class AdminPanelController : ControllerBase
{
	private readonly AccountService accountService;
	private readonly ClientService clientService;
	private readonly TrustedGameServerService trustedGameServerService;

	public AdminPanelController(
		AccountService accountService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService)
	{
		this.accountService = accountService;
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
