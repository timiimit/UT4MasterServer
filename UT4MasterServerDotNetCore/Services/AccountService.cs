using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using UT4MasterServer.Models;

namespace UT4MasterServer.Services
{
    public class AccountService
    {
        private readonly IMongoCollection<Account> _usersCollection;

        public AccountService(IOptions<UT4EverDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<Account>(settings.Value.AccountCollectionName);
        }

        public async Task<List<Account>> GetAsync()
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }
        public async Task<Account?> GetAsync(string id)
        {
            return await _usersCollection.Find(account => account.id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Account>> GetAsync(List<string> ids)
        {
            var filter = Builders<Account>.Filter.In("id", ids);
            var result = await _usersCollection.FindAsync(filter);
            return await result.ToListAsync();
        }

        // TODO: Implement these when needed
        //public async Task CreateAsync(Account newAccount) => await _usersCollection.InsertOneAsync(newAccount);
        //public async Task UpdateAsync(string id, Account updatedAccount) => await _usersCollection.ReplaceOneAsync(user => user.Id == id, updatedAccount);
        //public async Task RemoveAsync(string id) => await _usersCollection.DeleteOneAsync(user => user.Id == id);   
    }
}
