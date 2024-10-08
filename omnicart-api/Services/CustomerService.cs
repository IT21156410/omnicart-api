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

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing the user if found; otherwise, null.</returns>
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates the cart of the specified user with the provided list of cart items.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="updatedCart">The updated list of cart items to set for the user.</param>
        public async Task UpdateUserCartAsync(string userId, List<CartItem> updatedCart)
        {
            var update = Builders<User>.Update.Set(u => u.Cart, updatedCart);
            await _userCollection.UpdateOneAsync(u => u.Id == userId, update);
        }

        /// <summary>
        /// Saves the specified user's cart by updating it in the database.
        /// </summary>
        /// <param name="user">The user whose cart is to be saved, including the updated cart items.</param>
        public async Task SaveCartAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<User>.Update.Set(u => u.Cart, user.Cart);
            await _userCollection.UpdateOneAsync(filter, update);
        }

    }
}
