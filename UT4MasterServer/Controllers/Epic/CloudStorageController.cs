using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Services.Scoped;

namespace UT4MasterServer.Controllers.Epic;

[ApiController]
[Route("ut/api/cloudstorage")]
[AuthorizeBearer]
public sealed class CloudStorageController : JsonAPIController
{
	private readonly CloudStorageService cloudStorageService;
	private readonly MatchmakingService matchmakingService;
	private readonly AccountService accountService;

	private static readonly FileExtensionContentTypeProvider contentTypeProvider = new();

	public CloudStorageController(ILogger<CloudStorageController> logger,
		CloudStorageService cloudStorageService,
		MatchmakingService matchmakingService,
		AccountService accountService) : base(logger)
	{
		this.cloudStorageService = cloudStorageService;
		this.matchmakingService = matchmakingService;
		this.accountService = accountService;
	}

	[HttpGet("user/{id}")]
	public async Task<IActionResult> ListUserFiles(string id)
	{
		List<CloudFile> files = await cloudStorageService.ListFilesAsync(EpicID.FromString(id), false);
		return BuildListResult(files);
	}

	[HttpGet("user/{id}/{filename}")]
	public async Task<IActionResult> GetUserFile(string id, string filename)
	{
		return await GetFile(id, filename);
	}

	[NonAction]
	public async Task<IActionResult> GetFile(string id, string filename)
	{
		var isStatsFile = filename == "stats.json";

		var accountID = EpicID.FromString(id);
		CloudFile? file = await cloudStorageService.GetFileAsync(accountID, filename);
		if (file == null)
		{
			if (!isStatsFile)
			{
				return NotFound(new ErrorResponse()
				{
					ErrorCode = "errors.com.epicgames.cloudstorage.file_not_found",
					ErrorMessage = $"Sorry, we couldn't find a file {filename} for account {id}",
					MessageVars = new[] { filename, id },
					NumericErrorCode = 12007,
					OriginatingService = "utservice",
					Intent = "prod10"
				});
			}

			// Send a fake response in order to fix #109 (which is a game bug)
			var playerName = "New Player";
			EpicID playerID = EpicID.Empty;
			Account? account = await accountService.GetAccountAsync(accountID);
			if (account != null)
			{
				playerName = account.Username;
				playerID = account.ID;
			}

			file = new CloudFile() { RawContent = Encoding.UTF8.GetBytes($"{{\"PlayerName\":\"{playerName}\",StatsID:\"{playerID}\",Version:0}}") };
		}

		if (isStatsFile)
		{
			// HACK: Fix game bug where stats.json is expected to always have nul character at the end
			//       Bug is at UnrealTournament\Source\UnrealTournament\Private\Slate\Panels\SUTStatsViewerPanel.cpp:415
			var tmp = new byte[file.RawContent.Length + 1];
			Array.Copy(file.RawContent, tmp, file.RawContent.Length);
			tmp[tmp.Length - 1] = 0;

			file.RawContent = tmp;
		}

		if (!contentTypeProvider.TryGetContentType(filename, out var contentType))
		{
			contentType = "application/octet-stream";
		}

		return File(file.RawContent, contentType);
	}

	[HttpPut("user/{id}/{filename}")]
	public async Task<IActionResult> UpdateUserFile(string id, string filename)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			return Unauthorized();
		}

		var accountID = EpicID.FromString(id);
		if (user.Session.AccountID != accountID)
		{
			// cannot modify other's files

			// unless you are a game server with this player and are modifying this player's stats file
			var isServerWithPlayer = await matchmakingService.DoesClientOwnGameServerWithPlayerAsync(user.Session.ClientID, accountID);
			if (!isServerWithPlayer || filename != "stats.json")
			{
				return Unauthorized();
			}
		}

		await cloudStorageService.UpdateFileAsync(accountID, filename, Request.Body);
		return Ok();
	}

	[HttpGet("system")]
	public async Task<IActionResult> ListSystemFiles()
	{
		List<CloudFile> files = await cloudStorageService.ListFilesAsync(EpicID.Empty, true);
		return BuildListResult(files);
	}

	[AllowAnonymous]
	[HttpGet("system/{filename}")]
	public async Task<IActionResult> GetSystemFile(string filename)
	{
		return await GetFile(EpicID.Empty.ToString(), filename);
	}

	[NonAction]
	private IActionResult BuildListResult(IEnumerable<CloudFile> files)
	{
		var arr = new List<CloudFileResponse>();
		foreach (CloudFile file in files)
		{
			var fileResponse = new CloudFileResponse(file);
			if (file.AccountID.IsEmpty)
			{
				fileResponse.DoNotCache = false;
			}

			arr.Add(fileResponse);
		}

		return Ok(arr);
	}
}
