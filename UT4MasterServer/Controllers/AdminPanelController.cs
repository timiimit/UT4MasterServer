using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Request;
using UT4MasterServer.Models.DTO.Requests;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Responses;
using UT4MasterServer.Services.Interfaces;
using UT4MasterServer.Services.Scoped;

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
	private readonly MatchmakingService matchmakingService;
	private readonly CleanupService cleanupService;
	private readonly IEmailService emailService;

	public AdminPanelController(
		ILogger<AdminPanelController> logger,
		AccountService accountService,
		SessionService sessionService,
		CloudStorageService cloudStorageService,
		ClientService clientService,
		TrustedGameServerService trustedGameServerService,
		MatchmakingService matchmakingService,
		CleanupService cleanupService,
		IEmailService emailService)
	{
		this.logger = logger;
		this.accountService = accountService;
		this.sessionService = sessionService;
		this.cloudStorageService = cloudStorageService;
		this.clientService = clientService;
		this.trustedGameServerService = trustedGameServerService;
		this.matchmakingService = matchmakingService;
		this.cleanupService = cleanupService;
		this.emailService = emailService;
	}

	#region Accounts

	[HttpGet("flags")]
	public async Task<IActionResult> GetAllPossibleFlags()
	{
		await VerifyAccessAsync(AccountFlags.ACL_AccountsLow, AccountFlags.ACL_AccountsHigh);

		return Ok(Enum.GetNames<AccountFlags>().OrderBy(x => x));
	}

	[HttpGet("flags/{accountID}")]
	public async Task<IActionResult> GetAccountFlags(string accountID)
	{
		await VerifyAccessAsync(AccountFlags.ACL_AccountsLow, AccountFlags.ACL_AccountsHigh);

		var flags = await accountService.GetAccountFlagsAsync(EpicID.FromString(accountID));
		if (flags == null)
			return NotFound();

		var result = EnumHelpers.EnumToStrings(flags.Value);
		return Ok(result);
	}

	[HttpPut("flags/{accountID}")]
	public async Task<IActionResult> SetAccountFlags(string accountID, [FromBody] string[] flagNames)
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_AccountsLow, AccountFlags.ACL_AccountsHigh);

		var flags = EnumHelpers.StringsToEnumArray<AccountFlags>(flagNames);

		var account = await accountService.GetAccountAsync(EpicID.FromString(accountID));
		if (account == null)
			return NotFound();

		var flagsOld = EnumHelpers.EnumFlagsToEnumArray(account.Flags);
		var adminFlags = admin.Account.Flags;

		var flagsAdded = EnumHelpers.EnumArrayToEnumFlags(flags.Where(x => !flagsOld.Contains(x)));
		var flagsRemoved = EnumHelpers.EnumArrayToEnumFlags(flagsOld.Where(x => !flags.Contains(x)));

		var logLevel = LogLevel.Warning;

		try
		{
			if (adminFlags.HasFlag(AccountFlags.Admin))
			{
				if (flagsRemoved.HasFlag(AccountFlags.Admin))
				{
					// TODO: there should be something like voting in order to pass this decision
					return Unauthorized($"Cannot remove {nameof(AccountFlags.Admin)} flag, this action must be performed with direct access to database");
				}

				if (flagsAdded.HasFlag(AccountFlags.Admin))
				{
					// TODO: there should be something like voting in order to pass this decision
				}
			}
			else if (adminFlags.HasFlag(AccountFlags.ACL_AccountsHigh))
			{
				if (flagsAdded.HasFlag(AccountFlags.Admin))
				{
					return Unauthorized($"Only {nameof(AccountFlags.Admin)} may add {nameof(AccountFlags.Admin)} flag to account");
				}

				if (flagsAdded.HasFlag(AccountFlags.ACL_AccountsHigh))
				{
					return Unauthorized($"Only {nameof(AccountFlags.Admin)} may add {nameof(AccountFlags.ACL_AccountsHigh)} flag to account");
				}

				if (flagsRemoved.HasFlag(AccountFlags.Admin))
				{
					return Unauthorized($"Cannot remove {nameof(AccountFlags.Admin)} flag");
				}

				if (AccountFlagsHelper.IsACLFlag(flagsRemoved))
				{
					return Unauthorized($"Only {nameof(AccountFlags.Admin)} may remove ACL flags");
				}
			}
			else // if (adminFlags.HasFlag(AccountFlags.ACL_AccountsLow))
			{
				if (AccountFlagsHelper.IsACLFlag(flagsAdded))
				{
					return Unauthorized($"Only {nameof(AccountFlags.Admin)} may add ACL flags");
				}

				if (AccountFlagsHelper.IsACLFlag(flagsRemoved))
				{
					return Unauthorized($"Only {nameof(AccountFlags.Admin)} may remove ACL flags");
				}
			}

			logLevel = LogLevel.Information;

			if (flagsRemoved.HasFlag(AccountFlags.HubOwner))
			{
				await trustedGameServerService.RemoveAllByAccountAsync(account.ID);
			}
			await accountService.UpdateAccountFlagsAsync(account.ID, EnumHelpers.EnumArrayToEnumFlags(flags));

			return Ok();
		}
		finally
		{
			logger.Log(
				logLevel,
				"{User} {OperationResultText} flags of {EditedUser}. | Added: {FlagsAdded} | Removed: {FlagsRemoved}",
				admin.Account,
				logLevel <= LogLevel.Information ? "edited" : "failed to edit",
				account,
				string.Join(", ", EnumHelpers.EnumToStrings(flagsAdded)),
				string.Join(", ", EnumHelpers.EnumToStrings(flagsRemoved))
			);
		}
	}

	[HttpPatch("change_password/{id}")]
	public async Task<IActionResult> ChangePassword(string id, [FromBody] AdminPanelChangePasswordRequest body)
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_AccountsLow, AccountFlags.ACL_AccountsHigh);

		var account = await accountService.GetAccountAsync(EpicID.FromString(id));
		if (account is null)
		{
			return NotFound(new ErrorResponse() { ErrorMessage = $"Failed to find account {id}" });
		}

		var flags = account.Flags;
		var logLevel = LogLevel.Warning;

		try
		{
			if (admin.Account.Flags.HasFlag(AccountFlags.Admin))
			{
				if (flags.HasFlag(AccountFlags.Admin))
				{
					return Unauthorized($"Cannot change password of another {nameof(AccountFlags.Admin)}");
				}
			}
			else // if (admin.Account.Flags.HasFlag(AccountFlags.ACL_AccountsHigh) || adminFlags.HasFlag(AccountFlags.ACL_AccountsLow))
			{
				if (flags.HasFlag(AccountFlags.Admin))
				{
					return Unauthorized($"Cannot change password of {nameof(AccountFlags.Admin)} account");
				}

				if (AccountFlagsHelper.IsACLFlag(flags))
				{
					return Unauthorized($"Cannot change password of an account with ACL flag");
				}
			}

			// passwords should already be hashed, but check its length just in case
			if (!ValidationHelper.ValidatePassword(body.NewPassword))
			{
				return BadRequest($"newPassword is not a SHA512 hash");
			}

			if (account.Email != body.Email)
			{
				return BadRequest("Invalid email");
			}

			if (body.IAmSure != true)
			{
				return BadRequest($"'iAmSure' was not 'true'");
			}

			await accountService.UpdateAccountPasswordAsync(account.ID, body.NewPassword);

			// logout user to make sure they remember the changed password by being forced to log in again,
			// as well as prevent anyone else from using this account after successful password change.
			await sessionService.RemoveSessionsWithFilterAsync(EpicID.Empty, account.ID, EpicID.Empty);

			return Ok();
		}
		finally
		{
			logger.Log(
				logLevel,
				"{User} {OperationResultText} password of {EditedUser}.",
				admin.Account,
				logLevel <= LogLevel.Information ? "changed" : "was not authorized to change",
				account
			);
		}
	}

	[HttpDelete("account/{id}")]
	public async Task<IActionResult> DeleteAccountInfo(string id, [FromBody] bool? forceCheckBroken)
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_AccountsHigh | AccountFlags.ACL_Maintenance);

		var accountID = EpicID.FromString(id);
		var account = await accountService.GetAccountUsernameAndFlagsAsync(accountID);

		var logLevel = LogLevel.Warning;
		try
		{
			if (account is null)
			{
				if (!admin.Account.Flags.HasFlag(AccountFlags.Admin) && !admin.Account.Flags.HasFlag(AccountFlags.ACL_Maintenance))
				{
					return NotFound(new ErrorResponse() { ErrorMessage = "Account not found" });
				}
			}
			else
			{
				if (admin.Account.Flags.HasFlag(AccountFlags.Admin))
				{
					if (account.Flags.HasFlag(AccountFlags.Admin))
					{
						return Unauthorized($"Cannot delete account of {nameof(AccountFlags.Admin)}. Account needs to be demoted first.");
					}
				}
				else if (admin.Account.Flags.HasFlag(AccountFlags.ACL_AccountsHigh))
				{
					if (account.Flags.HasFlag(AccountFlags.Admin))
					{
						return Unauthorized($"Cannot delete account of {nameof(AccountFlags.Admin)}");
					}

					if (AccountFlagsHelper.IsACLFlag(account.Flags))
					{
						return Unauthorized($"Cannot delete account with ACL flag");
					}
				}
				else // if (admin.Account.Flags.HasFlag(AccountFlags.ACL_Maintenance))
				{
					return Unauthorized("You do not possess sufficient permissions to delete an existing account");
				}
			}

			// Remove account and all of its associated data
			await cleanupService.RemoveAccountAndAssociatedDataAsync(accountID);

			logLevel = LogLevel.Information;

			return Ok();
		}
		finally
		{
			logger.Log(
				logLevel,
				"{User} {OperationResultText} account of {EditedUser}.",
				admin.Account,
				logLevel <= LogLevel.Information ? "deleted" : "was not authorized to delete",
				account
			);
		}
	}

	#endregion

	#region Clients

	[HttpPost("clients/new")]
	public async Task<IActionResult> CreateClient([FromBody] string name)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Clients);

		var client = new Client(EpicID.GenerateNew(), EpicID.GenerateNew().ToString(), name);
		await clientService.UpdateAsync(client);

		return Ok(client);
	}

	[HttpGet("clients")]
	public async Task<IActionResult> GetAllClients()
	{
		await VerifyAccessAsync(AccountFlags.ACL_Clients);

		var clients = await clientService.ListAsync();
		return Ok(clients);
	}

	[HttpGet("clients/{id}")]
	public async Task<IActionResult> GetClient(string id)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Clients);

		var client = await clientService.GetAsync(EpicID.FromString(id));
		if (client == null)
			return NotFound();

		return Ok(client);
	}

	[HttpPatch("clients/{id}")]
	public async Task<IActionResult> UpdateClient(string id, [FromBody] Client client)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Clients);

		var eid = EpicID.FromString(id);

		if (eid != client.ID)
		{
			return BadRequest();
		}

		if (IsSpecialClientID(eid))
		{
			return Forbid("Cannot modify reserved clients");
		}

		var taskUpdateClient = clientService.UpdateAsync(client);
		var taskUpdateServerName = matchmakingService.UpdateServerNameAsync(client.ID, client.Name);

		await taskUpdateClient;
		await taskUpdateServerName;

		return Ok();
	}

	[HttpDelete("clients/{id}")]
	public async Task<IActionResult> DeleteClient(string id)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Clients);

		var eid = EpicID.FromString(id);

		if (IsSpecialClientID(eid))
		{
			return Forbid("Cannot delete reserved clients");
		}

		var success = await clientService.RemoveAsync(eid);
		if (success != true)
		{
			return BadRequest();
		}

		// in case this client is a trusted server remove, it as well
		await trustedGameServerService.RemoveAsync(eid);

		return Ok();
	}

	#endregion

	#region Trusted Servers

	[HttpGet("trusted_servers")]
	public async Task<IActionResult> GetAllTrustedServers()
	{
		await VerifyAccessAsync(AccountFlags.ACL_TrustedServers);

		var trustedServers = await trustedGameServerService.ListAsync();

		var trustedServerIDs = trustedServers.Select(t => t.ID);
		var trustedServerOwnerIDs = trustedServers.Select(t => t.OwnerID).Distinct();

		var taskClients = clientService.GetManyAsync(trustedServerIDs);
		var taskAccounts = accountService.GetAccountsAsync(trustedServerOwnerIDs);

		var clients = await taskClients;
		var accounts = await taskAccounts;

		var response = trustedServers.Select(t => new TrustedGameServerResponse
		{
			ID = t.ID,
			OwnerID = t.OwnerID,
			TrustLevel = t.TrustLevel,
			Client = clients.SingleOrDefault(c => c.ID == t.ID),
			Owner = accounts.SingleOrDefault(a => a.ID == t.OwnerID)
		});
		return Ok(response);
	}

	[HttpGet("trusted_servers/{id}")]
	public async Task<IActionResult> GetTrustedServer(string id)
	{
		await VerifyAccessAsync(AccountFlags.ACL_TrustedServers);

		var ret = await trustedGameServerService.GetAsync(EpicID.FromString(id));
		return Ok(ret);
	}

	[HttpPost("trusted_servers")]
	public async Task<IActionResult> CreateTrustedServer([FromBody] TrustedGameServer body)
	{
		await VerifyAccessAsync(AccountFlags.ACL_TrustedServers);

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

		if (!owner.Flags.HasFlag(AccountFlags.HubOwner))
		{
			return BadRequest($"Account specified with OwnerID {body.OwnerID} is not marked as HubOwner");
		}

		await trustedGameServerService.UpdateAsync(body);
		await matchmakingService.UpdateTrustLevelAsync(body.ID, body.TrustLevel);

		return Ok();
	}

	[HttpPatch("trusted_servers/{id}")]
	public async Task<IActionResult> UpdateTrustedServer(string id, [FromBody] TrustedGameServer server)
	{
		await VerifyAccessAsync(AccountFlags.ACL_TrustedServers);

		var eid = EpicID.FromString(id);

		if (eid != server.ID)
			return BadRequest();

		await trustedGameServerService.UpdateAsync(server);
		await matchmakingService.UpdateTrustLevelAsync(server.ID, server.TrustLevel);

		return Ok();
	}

	[HttpDelete("trusted_servers/{id}")]
	public async Task<IActionResult> DeleteTrustedServer(string id)
	{
		await VerifyAccessAsync(AccountFlags.ACL_TrustedServers);

		var eid = EpicID.FromString(id);
		var ret = await trustedGameServerService.RemoveAsync(eid);

		await matchmakingService.UpdateTrustLevelAsync(eid, GameServerTrust.Untrusted);

		return Ok(ret);
	}

	#endregion

	#region Cloud Storage

	[HttpGet("mcp_files")]
	public async Task<IActionResult> GetMCPFiles()
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_CloudStorageAnnouncements | AccountFlags.ACL_CloudStorageRulesets | AccountFlags.ACL_CloudStorageChallenges);
		var files = await cloudStorageService.ListFilesAsync(EpicID.Empty, false);
		var responseFiles = files.Select(x => new CloudFileAdminPanelResponse(x, IsAccessibleCloudStorageFile(admin.Account.Flags, x.Filename)));
		return Ok(responseFiles);
	}

	[HttpPost("mcp_files")]
	public async Task<IActionResult> UpdateMCPFile()
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_CloudStorageAnnouncements | AccountFlags.ACL_CloudStorageRulesets | AccountFlags.ACL_CloudStorageChallenges);

		var formCollection = await Request.ReadFormAsync();
		if (formCollection.Files.Count < 1)
		{
			return BadRequest("Missing file");
		}

		var file = formCollection.Files[0];
		var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
		if (file.Length <= 0)
		{
			return BadRequest("Cannot upload empty file");
		}

		if (!IsAccessibleCloudStorageFile(admin.Account.Flags, filename))
		{
			return Unauthorized("You are not authorized to upload this file");
		}

		using (var stream = file.OpenReadStream())
		{
			await cloudStorageService.UpdateFileAsync(EpicID.Empty, filename, stream);
		}
		return Ok();
	}

	[HttpGet("mcp_files/{filename}"), Produces("application/octet-stream")]
	public async Task<IActionResult> GetMCPFile(string filename)
	{
		var admin = await VerifyAccessAsync(AccountFlags.ACL_CloudStorageAnnouncements | AccountFlags.ACL_CloudStorageRulesets | AccountFlags.ACL_CloudStorageChallenges);

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
		var admin = await VerifyAccessAsync(AccountFlags.ACL_CloudStorageAnnouncements | AccountFlags.ACL_CloudStorageRulesets | AccountFlags.ACL_CloudStorageChallenges);

		if (!IsAccessibleCloudStorageFile(admin.Account.Flags, filename))
		{
			return Unauthorized("You are not authorized to delete this file");
		}

		if (await cloudStorageService.DeleteFileAsync(EpicID.Empty, filename) != true)
		{
			return Forbid("Cannot delete file. Either this file is not deletable or something went wrong.");
		}
		return Ok();
	}

	#endregion

	[HttpPost("send-text-email")]
	public async Task<IActionResult> SendTextEmail([FromBody] SendEmailRequest sendEmailRequest)
	{
		await emailService.SendTextEmailAsync(sendEmailRequest.From, sendEmailRequest.To, sendEmailRequest.Subject, sendEmailRequest.Body);
		return Ok();
	}

	[HttpPost("send-html-email")]
	public async Task<IActionResult> SendHtmlEmail([FromBody] SendEmailRequest sendEmailRequest)
	{
		await emailService.SendHTMLEmailAsync(sendEmailRequest.From, sendEmailRequest.To, sendEmailRequest.Subject, sendEmailRequest.Body);
		return Ok();
	}

	[NonAction]
	private async Task<(Session Session, Account Account)> VerifyAccessAsync(params AccountFlags[] aclAny)
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

		var combinedAcl = aclAny.Aggregate((x, y) => x | y) | AccountFlags.Admin;

		if (!account.Flags.HasFlagAny(combinedAcl))
		{
			throw new UnauthorizedAccessException("User has insufficient privileges");
		}

		return (user.Session, account);
	}

	[NonAction]
	private static bool IsSpecialClientID(EpicID id)
	{
		if (id == ClientIdentification.Game.ID || id == ClientIdentification.ServerInstance.ID || id == ClientIdentification.Launcher.ID)
		{
			return true;
		}

		return false;
	}

	[NonAction]
	private static bool IsAccessibleCloudStorageFile(AccountFlags flags, string filename)
	{
		if (flags.HasFlag(AccountFlags.Admin))
		{
			return true;
		}
		if (flags.HasFlag(AccountFlags.ACL_CloudStorageAnnouncements))
		{
			if (filename == "UnrealTournmentMCPAnnouncement.json" || filename.StartsWith("news-"))
			{
				return true;
			}
		}
		if (flags.HasFlag(AccountFlags.ACL_CloudStorageRulesets))
		{
			var allowedFilenames = new[] { "UTMCPPlaylists.json", "UnrealTournamentOnlineSettings.json", "UnrealTournmentMCPGameRulesets.json" };
			if (allowedFilenames.Contains(filename))
			{
				return true;
			}
		}
		if (flags.HasFlag(AccountFlags.ACL_CloudStorageChallenges))
		{
			if (filename == "UnrealTournmentMCPStorage.json")
			{
				return true;
			}
		}

		return false;
	}
}
