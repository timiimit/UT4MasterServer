﻿using Microsoft.AspNetCore.Mvc;
using System.Text;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Database;
using Microsoft.AspNetCore.Authorization;

namespace UT4MasterServer.Controllers.Epic;

[ApiController]
[Route("ut/api/cloudstorage")]
[AuthorizeBearer]
public sealed class CloudStorageController : JsonAPIController
{
	private readonly CloudStorageService cloudStorageService;
	private readonly MatchmakingService matchmakingService;
	private readonly AccountService accountService;

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
		var files = await cloudStorageService.ListFilesAsync(EpicID.FromString(id), false);
		return BuildListResult(files);
	}

	[HttpGet("user/{id}/{filename}"), Produces("application/octet-stream")]
	public async Task<IActionResult> GetUserFile(string id, string filename)
	{
		bool isStatsFile = filename == "stats.json";

		var accountID = EpicID.FromString(id);
		var file = await cloudStorageService.GetFileAsync(accountID, filename);
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
			var playerID = EpicID.Empty;
			var account = await accountService.GetAccountAsync(accountID);
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

		return new FileContentResult(file.RawContent, "application/octet-stream");
	}

	[HttpPut("user/{id}/{filename}")]
	public async Task<IActionResult> UpdateUserFile(string id, string filename)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();
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
		var files = await cloudStorageService.ListFilesAsync(EpicID.Empty, true);
		return BuildListResult(files);
	}

	[AllowAnonymous]
	[HttpGet("system/{filename}"), Produces("application/octet-stream")]
	public async Task<IActionResult> GetSystemFile(string filename)
	{
		return await GetUserFile(EpicID.Empty.ToString(), filename);
	}

	[NonAction]
	private IActionResult BuildListResult(IEnumerable<CloudFile> files)
	{
		var arr = new List<CloudFileResponse>();
		foreach (var file in files)
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
