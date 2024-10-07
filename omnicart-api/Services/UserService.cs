
// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB users collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    // https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly VendorService _vendorService;

        /// <summary>
        /// Initializes the UserService with the MongoDB client, database, and users collection
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public UserService(IOptions<MongoDbSettings> mongoDbSettings, VendorService vendorService)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
            _vendorService = vendorService;
        }

        /// <summary>
        /// Retrieves all users
        /// </summary>
        /// <returns>List<User></returns>
        public async Task<List<User>> GetUsersAsync() =>
            await _userCollection.Find(_ => true).ToListAsync();

        /// <summary>
        /// Get user for given id
        /// </summary>
        /// <param name="id">ObjectId of the user</param>
        /// <returns>User?</returns>
        public async Task<User?> GetUserByIdAsync(string id) =>
            await _userCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

        /// <summary>
        /// Get user for a given email
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <returns>User?</returns>
        public async Task<User?> FindByEmailAsync(string email) =>
            await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();

        /// <summary>
        /// Inserts a new user document and creates a vendor if the role is vendor.
        /// </summary>
        /// <param name="newUser">New User Object</param>
        /// <returns></returns>
        public async Task CreateUserAsync(CreateUserDto newUser)
        {
            var user = new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = newUser.Name,
                Email = newUser.Email,
                Password = AuthService.HashPassword(newUser.Password),
                Role = newUser.Role
            };

            await _userCollection.InsertOneAsync(user);

            // If the role is vendor, create a new vendor record
            if (user.Role == Role.vendor)
            {
                var vendor = new Vendor
                {
                    UserId = user.Id,
                    BusinessName = string.IsNullOrWhiteSpace(newUser.BusinessName) ? "Unnamed Vendor" : newUser.BusinessName,
                    AverageRating = 0.0M,
                    Comments = new List<VendorComment>()
                };

                await _vendorService.CreateVendorAsync(vendor);
            }
        }

        /// <summary>
        /// Update the user
        /// </summary>
        /// <param name="id">ObjectId of the user</param>
        /// <param name="updatedUser">The updated user object</param>
        /// <returns></returns>
        public async Task UpdateUserAsync(string id, User updatedUser) =>
            await _userCollection.ReplaceOneAsync(user => user.Id == id, updatedUser);

        /// <summary>
        /// Delete the user 
        /// </summary>
        /// <param name="id">ObjectId of the user</param>
        /// <returns></returns>
        public async Task DeleteUserAsync(string id) =>
            await _userCollection.DeleteOneAsync(user => user.Id == id);
    }
}
