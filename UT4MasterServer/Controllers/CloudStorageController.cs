using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UT4MasterServer.Authorization;
using UT4MasterServer.Models;
using UT4MasterServer.Services;

namespace UT4MasterServer.Controllers;

[ApiController]
[Route("ut/api/cloudstorage")]
[AuthorizeBearer]
[Produces("application/octet-stream")]
public class CloudstorageController : JsonAPIController
{
	private readonly CloudstorageService cloudstorageService;

	public CloudstorageController(CloudstorageService cloudstorageService)
	{
		this.cloudstorageService = cloudstorageService;
	}

	[HttpGet("user/{id}")]
	public async Task<IActionResult> ListUserfiles(string id)
	{
		// list all files this user has in storage - any user can see files from another user

		/* [

		{"uniqueFilename":"user_progression_1",
		"filename":"user_progression_1",
		"hash":"32a17bdf348e653a5cc7f94c3afb404301502d43",
		"hash256":"7dcfaac101dbba0337e1b51bf3c088e591742d5f1c299f10cc0c9da01eab5fe8",
		"length":21,
		"contentType":"text/plain",
		"uploaded":"2020-05-24T07:10:43.198Z",
		"storageType":"S3",
		"accountId":"64bf8c6d81004e88823d577abe157373"
		},
		]


		*/

		var eid = EpicID.FromString(id);

		var files = await cloudstorageService.ListFilesAsync(eid);

		var arr = new JArray();
		foreach (var file in files)
		{
			var obj = new JObject();
			obj.Add("uniqueFilename", file.Filename);
			obj.Add("filename", file.Filename);
			obj.Add("hash", file.Hash);
			obj.Add("hash256", file.Hash256);
			obj.Add("length", file.Length);
			obj.Add("contentType", "text/plain"); // this seems to be constant
			obj.Add("uploaded", file.UploadedAt.ToStringISO());
			obj.Add("storageType", "S3");
			obj.Add("accountId", id);
			if (eid.IsEmpty)
			{
				obj.Add("doNotCache", false);
			}
			arr.Add(obj);
		}

		return Json(arr);
	}

	[HttpGet("user/{id}/{filename}")]
	public async Task<IActionResult> GetUserfile(string id, string filename)
	{
		// get the user file from cloudstorage - any user can see files from another user

		var file = await cloudstorageService.GetFileAsync(EpicID.FromString(id), filename);
		if (file == null)
		{
			return Json(new ErrorResponse()
			{
				ErrorCode = "errors.com.epicgames.cloudstorage.file_not_found",
				ErrorMessage = $"Sorry, we couldn't find a file {filename} for account {id}",
				MessageVars = new[] { filename, id },
				NumericErrorCode = 12007,
				OriginatingService = "utservice",
				Intent = "prod10"
			}, StatusCodes.Status404NotFound);
		}

		return new FileContentResult(file.RawContent, "application/octet-stream");
	}

	[HttpPut("user/{id}/{filename}")]
	public async Task<IActionResult> UpdateUserfile(string id, string filename)
	{
		if (User.Identity is not EpicUserIdentity user)
			return Unauthorized();

		if (user.Session.AccountID != EpicID.FromString(id))
			return Unauthorized(); // users can modify only their own files

		await cloudstorageService.UpdateFileAsync(user.Session.AccountID, filename, Request.BodyReader);
		return Ok();
	}

	[HttpGet("system")]
	public Task<IActionResult> ListSystemfiles()
	{
		return ListUserfiles(EpicID.Empty.ToString());
	}

	[HttpGet("system/{filename}")]
	public async Task<IActionResult> GetSystemfile(string filename)
	{
		return await GetUserfile(EpicID.Empty.ToString(), filename);
	}
}
