// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handling data from MongoDB related to customer.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class CustomerService
    {
        private readonly IMongoCollection<User> _userCollection;

        public CustomerService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
        }

        // Get user by ID
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        // Update the user's cart
        public async Task UpdateUserCartAsync(string userId, List<CartItem> updatedCart)
        {
            var update = Builders<User>.Update.Set(u => u.Cart, updatedCart);
            await _userCollection.UpdateOneAsync(u => u.Id == userId, update);
        }

        // Save the user's cart
        public async Task SaveCartAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<User>.Update.Set(u => u.Cart, user.Cart);
            await _userCollection.UpdateOneAsync(filter, update);
        }

    }
}
