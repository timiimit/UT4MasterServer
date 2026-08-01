using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Security.Cryptography;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Models.DTO.Responses;
using UT4MasterServer.Models.Settings;

namespace UT4MasterServer.Services.Scoped;

/// <summary>
/// Stores and retrieves bug-report log files. Log bytes are kept in a GridFS
/// bucket while a small metadata document per report is kept in the
/// "logreports" collection for cheap listing.
/// </summary>
public sealed class LogReportService
{
	/// <summary>
	/// Crockford base32 alphabet (no I, L, O, U) used for human-friendly report IDs.
	/// </summary>
	private const string ReportIDAlphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
	private const int ReportIDLength = 8;
	private const int MaxReportIDGenerationAttempts = 10;

	private readonly ILogger<LogReportService> logger;
	private readonly IMongoCollection<LogReport> logReportCollection;
	private readonly GridFSBucket bucket;

	public LogReportService(
		ILogger<LogReportService> logger,
		DatabaseContext dbContext,
		IOptions<LogReportSettings> settings)
	{
		this.logger = logger;
		logReportCollection = dbContext.Database.GetCollection<LogReport>("logreports");
		bucket = new GridFSBucket(dbContext.Database, new GridFSBucketOptions()
		{
			BucketName = settings.Value.BucketName
		});
	}

	public async Task CreateIndexesAsync()
	{
		var indexes = new[]
		{
			new CreateIndexModel<LogReport>(
				Builders<LogReport>.IndexKeys.Ascending(x => x.ReportID),
				new CreateIndexOptions() { Unique = true }),
			new CreateIndexModel<LogReport>(
				Builders<LogReport>.IndexKeys.Descending(x => x.CreatedAt))
		};
		await logReportCollection.Indexes.CreateManyAsync(indexes);
	}

	public async Task<LogReport> CreateReportAsync(
		Stream logStream, long length, string? player, string? note,
		string clientIPHash, string userAgent, string contentType)
	{
		var reportID = await GenerateUniqueReportIDAsync();

		ObjectId fileID = await bucket.UploadFromStreamAsync(reportID, logStream, new GridFSUploadOptions()
		{
			Metadata = new BsonDocument()
			{
				{ "ReportID", reportID },
				{ "ContentType", contentType }
			}
		});

		var report = new LogReport()
		{
			ReportID = reportID,
			FileID = fileID,
			Player = player,
			Note = note,
			ClientIPHash = clientIPHash,
			UserAgent = userAgent,
			CreatedAt = DateTime.UtcNow,
			Length = length,
			ContentType = contentType
		};

		await logReportCollection.InsertOneAsync(report);

		logger.LogInformation("Stored bug report {ReportID} ({Length} bytes).", reportID, length);

		return report;
	}

	public async Task<PagedResponse<LogReport>> ListReportsAsync(int skip, int limit)
	{
		FilterDefinition<LogReport>? filter = Builders<LogReport>.Filter.Empty;

		var count = await logReportCollection.CountDocumentsAsync(filter);
		List<LogReport>? data = await logReportCollection
			.Find(filter)
			.SortByDescending(x => x.CreatedAt)
			.Skip(skip)
			.Limit(limit)
			.ToListAsync();

		return new PagedResponse<LogReport>()
		{
			Count = count,
			Data = data
		};
	}

	public async Task<LogReport?> GetReportAsync(string reportID)
	{
		IAsyncCursor<LogReport>? cursor = await logReportCollection.FindAsync(
			Builders<LogReport>.Filter.Eq(x => x.ReportID, reportID));
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<Stream> OpenLogStreamAsync(LogReport report)
	{
		return await bucket.OpenDownloadStreamAsync(report.FileID);
	}

	private async Task<string> GenerateUniqueReportIDAsync()
	{
		for (var attempt = 0; attempt < MaxReportIDGenerationAttempts; attempt++)
		{
			var reportID = GenerateReportID();

			// the unique index on ReportID is the final guarantee, this check
			// just avoids an insert failure in the common case
			var existing = await logReportCollection.CountDocumentsAsync(
				Builders<LogReport>.Filter.Eq(x => x.ReportID, reportID));
			if (existing == 0)
			{
				return reportID;
			}
		}

		throw new InvalidOperationException("Failed to generate a unique report ID");
	}

	private static string GenerateReportID()
	{
		// 256 % 32 == 0, so mapping bytes onto the alphabet introduces no modulo bias
		var bytes = RandomNumberGenerator.GetBytes(ReportIDLength);
		var chars = new char[ReportIDLength];
		for (var i = 0; i < ReportIDLength; i++)
		{
			chars[i] = ReportIDAlphabet[bytes[i] % ReportIDAlphabet.Length];
		}
		return new string(chars);
	}
}
