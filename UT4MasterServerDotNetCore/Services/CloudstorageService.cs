using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;
using System.Text;
using System.Security.Cryptography;

namespace UT4MasterServer.Services;

public class CloudstorageService
{
	// some general information about cloudstorage files
	//
	// systemfiles: <game_install_location>\UnrealTournament\PersistentDownloadDir\EMS
	// system files are always downloaded when game boots
	//
	// userfiles: <documents>\UnrealTournament\Saved\Cloud\<accountID>\<filename>
	// userfiles are only there while game is running and file had to have been needed within the game

	private readonly IMongoCollection<CloudFile> cloudstorageCollection;

	public CloudstorageService(IOptions<UT4EverDatabaseSettings> settings)
	{
		var mongoClient = new MongoClient(settings.Value.ConnectionString);
		var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
		cloudstorageCollection = mongoDatabase.GetCollection<CloudFile>(settings.Value.CloudstorageCollectionName);
	}

	public async Task UpdateFileAsync(EpicID accountID, string filename, Stream dataStream)
	{
		// for now we only care about specific files
		if (!IsCommonUserfileFilename(filename))
			return;

		byte[] buffer = new byte[1024 * 1024]; // 1MB = our max
		int bytesRead = await dataStream.ReadAsync(buffer);
		if (bytesRead < buffer.Length)
		{
			// if there have been fewer bytes read than buffer's length

			// TODO: verify that Array.Resize works as expected and removes excess bytes from array
			Array.Resize(ref buffer, bytesRead);

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
			await cloudstorageCollection.ReplaceOneAsync(
				x => x.AccountID == accountID &&
				x.Filename == filename, file, options
			);
		}
	}

	public async Task<CloudFile?> GetFileAsync(EpicID accountID, string filename)
	{
		var cursor = await cloudstorageCollection.FindAsync(
			x => x.AccountID == accountID &&
			x.Filename == filename
		);
		if (!await cursor.AnyAsync())
			return null;
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<List<CloudFile>> ListFilesAsync(EpicID accountID)
	{
		// TODO: make sure that CloudFile.Bytes remains empty after db retrieval
		FindOptions<CloudFile> options = new FindOptions<CloudFile>()
		{
			Projection = Builders<CloudFile>.Projection.Exclude(x => x.RawContent)
		};
		var cursor = await cloudstorageCollection.FindAsync(x => x.AccountID == accountID, options);
		return await cursor.ToListAsync();
	}

	public async Task DeleteFileAsync(EpicID accountID, string filename)
	{
		await cloudstorageCollection.DeleteOneAsync(
			x => x.AccountID == accountID &&
			x.Filename == filename
		);
	}



	private static bool IsCommonUserfileFilename(string filename)
	{
		// old players might also have "user_profile_1", but it is not used for anything
		return filename == "user_profile_2" || filename == "user_progression_1";
	}

	private static string CalcFileHash(byte[] data)
	{
		// TODO: figure out actual algo used, this one is pure guess
		var hashedBytes = SHA1.HashData(data);
		var hash = Convert.ToHexString(hashedBytes).ToLower();
		return hash;
	}

	private static string CalcFileHash256(byte[] data)
	{
		// TODO: figure out actual algo used, this one is pure guess
		var hashedBytes = SHA256.HashData(data);
		var hash = Convert.ToHexString(hashedBytes).ToLower();
		return hash;
	}
}
