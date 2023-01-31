using Microsoft.AspNetCore.DataProtection;
using MongoDB.Driver;
using System.Net.Sockets;
using UT4MasterServer.Authentication;
using UT4MasterServer.Models;
using UT4MasterServer.Other;

namespace UT4MasterServer.Services;

public sealed class ClientService
{
	private readonly IMongoCollection<Client> collection;

	public ClientService(DatabaseContext dbContext)
	{
		collection = dbContext.Database.GetCollection<Client>("clients");
	}

	public async Task UpdateDefaultClientsAsync()
	{
		await UpdateAsync(new Client(
			ClientIdentification.Launcher.ID,
			ClientIdentification.Launcher.Secret,
			nameof(ClientIdentification.Launcher) + " (our website)"
		));
		await UpdateAsync(new Client(
			ClientIdentification.Game.ID,
			ClientIdentification.Game.Secret,
			nameof(ClientIdentification.Game)
		));
		await UpdateAsync(new Client(
			ClientIdentification.ServerInstance.ID,
			ClientIdentification.ServerInstance.Secret,
			nameof(ClientIdentification.ServerInstance)
		));
	}

	public async Task<bool?> UpdateAsync(Client client)
	{
		var options = new ReplaceOptions() { IsUpsert = true };
		var result = await collection.ReplaceOneAsync(x => x.ID == client.ID, client, options);
		if (!result.IsAcknowledged)
			return null;

		return result.ModifiedCount == 1;
	}

	public async Task<Client?> GetAsync(EpicID id)
	{
		var options = new FindOptions<Client>() { Limit = 1 };
		var cursor = await collection.FindAsync(x => x.ID == id, options);
		return await cursor.SingleOrDefaultAsync();
	}

	public async Task<List<Client>> ListAsync()
	{
		var cursor = await collection.FindAsync(x => true);
		return await cursor.ToListAsync();
	}

	public async Task<bool?> RemoveAsync(EpicID id)
	{
		var result = await collection.DeleteOneAsync(x => x.ID == id);
		if (!result.IsAcknowledged)
			return null;

		return result.DeletedCount > 0;
	}
}
