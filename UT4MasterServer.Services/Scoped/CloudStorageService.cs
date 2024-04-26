using MongoDB.Driver;
using System.Security.Cryptography;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace UT4MasterServer.Services.Scoped;

/// <summary>
/// Some general information about cloudstorage files<br/><br/>
///
/// systemfiles: <code>{game_install_location}\UnrealTournament\PersistentDownloadDir\EMS</code>
/// system files are always downloaded when game boots<br/><br/>
///
/// userfiles: <code>{documents}\UnrealTournament\Saved\Cloud\{accountID}\{filename}</code>
/// userfiles are only there while game is running and file had to have been needed within the game
/// </summary>
public sealed class CloudStorageService
{
	private readonly IMongoCollection<CloudFile> cloudStorageCollection;
	private readonly ILogger<CloudStorageService> logger;

	private static readonly string[] commonSystemFileFilenames = new string[]
	{
		// yes, 3 of these are misspelled by epic
		"UnrealTournamentOnlineSettings.json",
		"UnrealTournmentMCPAnnouncement.json",
		"UnrealTournmentMCPGameRulesets.json",
		"UnrealTournmentMCPStorage.json",
		"UTMCPPlaylists.json"
	};
	private static readonly string[] commonUserFileFilenames = new string[]
	{
		// old players might also have "user_profile_1", but it is not used for anything anymore
		"user_profile_2",
		"user_progression_1",
		"stats.json"
	};

	public CloudStorageService(DatabaseContext dbContext, ILogger<CloudStorageService> logger)
	{
		cloudStorageCollection = dbContext.Database.GetCollection<CloudFile>("cloudstorage");
		this.logger = logger;
	}

	public async Task EnsureSystemFilesExistAsync()
	{
		// get a list of already stored system files
		var stored = await ListFilesAsync(EpicID.Empty, true);

		// ensure that all default files exist in db
		foreach (var filename in commonSystemFileFilenames)
		{
			var file = Path.Combine("CloudStorageSystemFiles", filename);

			if (stored.Any(x => x.Filename == filename))
			{
				// file already in db
				continue;
			}

			if (!File.Exists(file))
			{
				continue;
			}

			// file is not in db, save it
			logger.LogInformation("Adding cloud storage system file to database: {filename}", filename);
			using var stream = File.OpenRead(file);
			await UpdateFileAsync(EpicID.Empty, filename, stream);
		}
	}

	public async Task UpdateFileAsync(EpicID accountID, string filename, Stream dataStream)
	{
		if (!accountID.IsEmpty)
		{
			// for now we only care about specific files
			if (!IsCommonUserFileFilename(filename))
			{
				return;
			}
		}

		var buffer = await dataStream.ReadAsBytesAsync(1024 * 1024);

		var file = new CloudFile()
		{
			AccountID = accountID,
			Filename = filename,
			Hash = CalcFileHash(buffer),
			Hash256 = CalcFileHash256(buffer),
			UploadedAt = DateTime.UtcNow,
			RawContent = buffer,
			Length = buffer.Length
		};

		// upsert = update or insert if doesn't exist
		var options = new ReplaceOptions { IsUpsert = true };

		await cloudStorageCollection.ReplaceOneAsync(GetFilter(accountID, filename), file, options);
	}

	public async Task<CloudFile?> GetFileAsync(EpicID accountID, string filename)
	{
		var cursor = await cloudStorageCollection.FindAsync(GetFilter(accountID, filename));
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<List<CloudFile>> ListFilesAsync(EpicID accountID, bool hideCustomFiles)
	{
		FindOptions<CloudFile> options = new FindOptions<CloudFile>()
		{
			Projection = Builders<CloudFile>.Projection.Exclude(x => x.RawContent)
		};

		var filter = GetFilter(accountID);
		if (hideCustomFiles)
		{
			if (accountID.IsEmpty)
			{
				filter &= Builders<CloudFile>.Filter.In(x => x.Filename, commonSystemFileFilenames);
			}
			else
			{
				filter &= Builders<CloudFile>.Filter.In(x => x.Filename, commonUserFileFilenames);
			}
		}
		var cursor = await cloudStorageCollection.FindAsync(filter, options);
		return await cursor.ToListAsync();
	}

	public async Task<bool?> DeleteFileAsync(EpicID accountID, string filename)
	{
		if (accountID.IsEmpty && IsCommonSystemFileFilename(filename))
		{
			return false;
		}

		var result = await cloudStorageCollection.DeleteOneAsync(GetFilter(accountID, filename));
		if (!result.IsAcknowledged)
		{
			return null;
		}

		return result.DeletedCount > 0;
	}

	public async Task<int?> RemoveAllByAccountAsync(EpicID accountID)
	{
		var result = await cloudStorageCollection.DeleteManyAsync(GetFilter(accountID));
		if (!result.IsAcknowledged)
		{
			return null;
		}

		return (int)result.DeletedCount;
	}

	private static bool IsCommonUserFileFilename(string filename)
	{
		return commonUserFileFilenames.Contains(filename);
	}

	private static bool IsCommonSystemFileFilename(string filename)
	{
		return commonSystemFileFilenames.Contains(filename);
	}

	private static string CalcFileHash(byte[] data)
	{
		var hashedBytes = SHA1.HashData(data);
		var hash = Convert.ToHexString(hashedBytes).ToLower();
		return hash;
	}

	private static string CalcFileHash256(byte[] data)
	{
		var hashedBytes = SHA256.HashData(data);
		var hash = Convert.ToHexString(hashedBytes).ToLower();
		return hash;
	}

	private static FilterDefinition<CloudFile> GetFilter(EpicID accountID, string filename)
	{
		if (accountID.IsEmpty)
		{
			return
				Builders<CloudFile>.Filter.Exists(x => x.AccountID, false) &
				Builders<CloudFile>.Filter.Eq(x => x.Filename, filename);
		}

		return
			Builders<CloudFile>.Filter.Eq(x => x.AccountID, accountID) &
			Builders<CloudFile>.Filter.Eq(x => x.Filename, filename);
	}

	private static FilterDefinition<CloudFile> GetFilter(EpicID accountID)
	{
		if (accountID.IsEmpty)
		{
			return
				Builders<CloudFile>.Filter.Exists(x => x.AccountID, false);
		}

		return
			Builders<CloudFile>.Filter.Eq(x => x.AccountID, accountID);
	}
}
