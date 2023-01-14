using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Security.Cryptography;
using System.IO.Pipelines;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public class CloudStorageService
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

	public async Task UpdateSystemfiles()
	{
		var files = Directory.EnumerateFiles("CloudstorageSystemfiles");
		foreach (var file in files)
		{
			var filename = Path.GetFileName(file);
			if (filename == null)
				continue;

			using var stream = File.OpenRead(file);
			var reader = PipeReader.Create(stream);
			await UpdateFileAsync(EpicID.Empty, filename, reader);
		}
	}

	public async Task UpdateFileAsync(EpicID accountID, string filename, PipeReader dataStream)
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
		// TODO: make sure that CloudFile.RawContent remains empty after db retrieval
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
