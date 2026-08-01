using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UT4MasterServer.Authentication;
using UT4MasterServer.Common.Enums;
using UT4MasterServer.Common.Helpers;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Settings;
using UT4MasterServer.Services.Scoped;
using UT4MasterServer.Services.Singleton;

namespace UT4MasterServer.Controllers.UT;

/// <summary>
/// Bug-report / log-file ingest endpoint.
/// Uploading is anonymous (rate-limited), retrieval requires admin privileges.
/// </summary>
[ApiController]
[Route("ut/api/logs")]
[AuthorizeBearer]
[Produces("application/json")]
public sealed class LogsController : JsonAPIController
{
	/// <summary>
	/// Hard cap on the whole multipart request body. Slightly above
	/// <see cref="LogReportSettings.MaxFileSizeBytes"/> to leave room for multipart overhead.
	/// </summary>
	private const long MaxRequestSizeBytes = 26 * 1024 * 1024;

	private const int MaxPlayerLength = 64;
	private const int MaxNoteLength = 2048;
	private const int MaxUserAgentLength = 256;

	private static readonly string[] knownContentTypes = new string[]
	{
		"text/plain",
		"application/gzip",
		"application/x-gzip"
	};

	private readonly LogReportService logReportService;
	private readonly LogReportRateLimitService rateLimitService;
	private readonly AccountService accountService;
	private readonly IOptions<ApplicationSettings> applicationSettings;
	private readonly IOptions<LogReportSettings> logReportSettings;

	public LogsController(
		ILogger<LogsController> logger,
		LogReportService logReportService,
		LogReportRateLimitService rateLimitService,
		AccountService accountService,
		IOptions<ApplicationSettings> applicationSettings,
		IOptions<LogReportSettings> logReportSettings) : base(logger)
	{
		this.logReportService = logReportService;
		this.rateLimitService = rateLimitService;
		this.accountService = accountService;
		this.applicationSettings = applicationSettings;
		this.logReportSettings = logReportSettings;
	}

	[AllowAnonymous]
	[HttpPost]
	[RequestSizeLimit(MaxRequestSizeBytes)]
	public async Task<IActionResult> UploadLog()
	{
		LogReportSettings settings = logReportSettings.Value;

		// validate content length before touching the body
		if (Request.ContentLength == null || Request.ContentLength <= 0)
		{
			return BadRequest("Missing request body");
		}
		if (Request.ContentLength > MaxRequestSizeBytes)
		{
			return StatusCode(StatusCodes.Status413PayloadTooLarge, "Upload is too large");
		}

		IPAddress? ip = GetClientIP(applicationSettings);
		if (ip == null)
		{
			return BadRequest("Could not determine client address");
		}

		var ipHash = HashIP(ip);
		var window = TimeSpan.FromMinutes(settings.RateLimitWindowMinutes);
		if (!rateLimitService.TryRecordUpload(ipHash, settings.RateLimitMaxUploads, window))
		{
			logger.LogWarning("Rate-limited bug report upload from client {ClientIPHash}.", ipHash);
			return StatusCode(StatusCodes.Status429TooManyRequests, "Too many uploads, try again later");
		}

		IFormCollection? formCollection = await Request.ReadFormAsync();
		IFormFile? file = formCollection.Files.GetFile("log");
		if (file == null)
		{
			return BadRequest("Missing 'log' file");
		}
		if (file.Length <= 0)
		{
			return BadRequest("Cannot upload empty file");
		}
		if (file.Length > settings.MaxFileSizeBytes)
		{
			return StatusCode(StatusCodes.Status413PayloadTooLarge, "Log file is too large");
		}

		var player = Truncate(formCollection["player"].ToString(), MaxPlayerLength);
		var note = Truncate(formCollection["note"].ToString(), MaxNoteLength);
		var userAgent = Truncate(Request.Headers.UserAgent.ToString(), MaxUserAgentLength) ?? string.Empty;
		var contentType = knownContentTypes.Contains(file.ContentType) ? file.ContentType : "application/octet-stream";

		LogReport report;
		using (Stream? stream = file.OpenReadStream())
		{
			report = await logReportService.CreateReportAsync(stream, file.Length, player, note, ipHash, userAgent, contentType);
		}

		// intentionally return only the report id, never any stored content
		return Ok(new LogReportCreatedResponse(report.ReportID));
	}

	[HttpGet]
	public async Task<IActionResult> ListReports(int skip = 0, int limit = 50)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Maintenance);

		if (skip < 0) skip = 0;
		if (limit < 1 || limit > 100) limit = 100;

		PagedResponse<LogReport>? reports = await logReportService.ListReportsAsync(skip, limit);

		return Ok(new PagedResponse<LogReportResponse>()
		{
			Count = reports.Count,
			Data = reports.Data.Select(x => new LogReportResponse(x)).ToList()
		});
	}

	[HttpGet("{reportId}"), Produces("application/octet-stream")]
	public async Task<IActionResult> GetReport(string reportId)
	{
		await VerifyAccessAsync(AccountFlags.ACL_Maintenance);

		reportId = reportId.Trim().ToUpperInvariant();
		if (!IsValidReportID(reportId))
		{
			return BadRequest("Invalid report id");
		}

		LogReport? report = await logReportService.GetReportAsync(reportId);
		if (report == null)
		{
			return NotFound(new ErrorResponse() { ErrorMessage = "Report not found" });
		}

		Stream stream = await logReportService.OpenLogStreamAsync(report);

		var extension = report.ContentType is "application/gzip" or "application/x-gzip" ? ".log.gz" : ".log";
		return File(stream, report.ContentType, report.ReportID + extension);
	}

	private async Task<(Session Session, Account Account)> VerifyAccessAsync(params AccountFlags[] aclAny)
	{
		if (User.Identity is not EpicUserIdentity user)
		{
			throw new UnauthorizedAccessException("User not logged in");
		}

		Account? account = await accountService.GetAccountAsync(user.Session.AccountID);
		if (account == null)
		{
			throw new UnauthorizedAccessException("User not found");
		}

		AccountFlags combinedAcl = aclAny.Aggregate((x, y) => x | y) | AccountFlags.Admin;

		if (!account.Flags.HasFlagAny(combinedAcl))
		{
			throw new UnauthorizedAccessException("User has insufficient privileges");
		}

		return (user.Session, account);
	}

	private static bool IsValidReportID(string reportID)
	{
		return reportID.Length == 8 && reportID.All(x => x is (>= '0' and <= '9') or (>= 'A' and <= 'Z'));
	}

	private static string HashIP(IPAddress ip)
	{
		var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip.ToString()));
		return Convert.ToHexString(hashedBytes).ToLower();
	}

	private static string? Truncate(string value, int maxLength)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		value = value.Trim();
		return value.Length <= maxLength ? value : value[..maxLength];
	}
}
