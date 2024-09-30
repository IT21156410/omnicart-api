﻿
// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB users collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    // https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        /// <summary>
        /// Initializes the UserService with the MongoDB client, database, and users collection
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public UserService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
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
        /// Inserts a new user document
        /// </summary>
        /// <param name="newUser">New User Object</param>
        /// <returns></returns>
        public async Task CreateUserAsync(User newUser) =>
            await _userCollection.InsertOneAsync(newUser);

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