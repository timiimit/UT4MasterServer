using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
    public class AccountDataService
    {
        private readonly IMongoCollection<string> accountDataCollection;

        public AccountDataService(IOptions<UT4EverDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
            accountDataCollection = mongoDatabase.GetCollection<string>(settings.Value.AccountDataCollectionName);
        }

        public async Task<string> GetProfile(EpicID accountID)
        {
            throw new NotImplementedException();
        }
    }
}
