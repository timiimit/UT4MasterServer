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

	public AdminPanelController(AccountService accountService)
	{
		this.accountService = accountService;
	}

	[HttpGet("flags")]
	public async Task<IActionResult> GetAllPossibleFlags(EpicID accountID)
	{
		await VerifyAdmin();

		return Ok(Enum.GetNames<AccountFlags>());
	}

	[HttpGet("flags/{accountID}")]
	public async Task<IActionResult> GetAccountFlags(EpicID accountID)
	{
		await VerifyAdmin();

		var account = await accountService.GetAccountAsync(accountID);
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
	public async Task<IActionResult> SetAccountFlags(EpicID accountID, [FromBody] string[] flagNames)
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

		var account = await accountService.GetAccountAsync(accountID);
		if (account == null)
			return NotFound();

		account.Flags = flags;

		await accountService.UpdateAccountAsync(account);

		return Ok();
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
