using MongoDB.Driver;
using System.Security.Cryptography;
using UT4MasterServer.Common;
using UT4MasterServer.Models.Database;
using UT4MasterServer.Common.Helpers;

namespace UT4MasterServer.Services.Scoped;

public sealed class CloudStorageService
{
	// some general information about cloudstorage files
	//
	// systemfiles: <game_install_location>\UnrealTournament\PersistentDownloadDir\EMS
	// system files are always downloaded when game boots
	//
	// userfiles: <documents>\UnrealTournament\Saved\Cloud\<accountID>\<filename>
	// userfiles are only there while game is running and file had to have been needed within the game

	private readonly IMongoCollection<CloudFile> cloudStorageCollection;

	public CloudStorageService(DatabaseContext dbContext)
	{
		cloudStorageCollection = dbContext.Database.GetCollection<CloudFile>("cloudstorage");
	}

	public async Task EnsureSystemFilesExistAsync()
	{
		// get a list of already stored system files
		var stored = await ListFilesAsync(EpicID.Empty);

		// get a list of default system files
		var files = Directory.EnumerateFiles("CloudstorageSystemfiles");

		// ensure that all default files exist in db
		foreach (var file in files)
		{
			// get just the filename part of file path
			var filename = Path.GetFileName(file);
			if (filename == null)
				continue;

			if (stored.Where(x => x.Filename == filename).Any())
			{
				// file already in db
				continue;
			}

			// file is not in db, save it
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
				return;
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

	public async Task<List<CloudFile>> ListFilesAsync(EpicID accountID)
	{
		FindOptions<CloudFile> options = new FindOptions<CloudFile>()
		{
			Projection = Builders<CloudFile>.Projection.Exclude(x => x.RawContent)
		};
		var cursor = await cloudStorageCollection.FindAsync(GetFilter(accountID), options);
		return await cursor.ToListAsync();
	}

	public async Task DeleteFileAsync(EpicID accountID, string filename)
	{
		await cloudStorageCollection.DeleteOneAsync(GetFilter(accountID, filename));
	}

	public async Task<int?> RemoveFilesByAccountAsync(EpicID accountID)
	{
		var result = await cloudStorageCollection.DeleteManyAsync(GetFilter(accountID));
		if (!result.IsAcknowledged)
			return null;

		return (int)result.DeletedCount;
	}



	private static bool IsCommonUserFileFilename(string filename)
	{
		// old players might also have "user_profile_1", but it is not used for anything
		return filename == "user_profile_2" || filename == "user_progression_1" || filename == "stats.json";
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
