using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
        }

        public async Task<List<User>> GetUsersAsync() =>
            await _userCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetUserByIdAsync(string id) =>
            await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

        public async Task CreateUserAsync(User newUser) =>
            await _userCollection.InsertOneAsync(newUser);

        public async Task UpdateUserAsync(string id, User updatedUser) =>
            await _userCollection.ReplaceOneAsync(user => user.Id == id, updatedUser);

        public async Task DeleteUserAsync(string id) =>
            await _userCollection.DeleteOneAsync(user => user.Id == id);
    }
}
