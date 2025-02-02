﻿// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB categories collection. 
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services;

public class CategoryService
{
    private readonly IMongoCollection<Category> _categoryCollection;

    // assign mongodb settings and get the collection
    public CategoryService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _categoryCollection = mongoDatabase.GetCollection<Category>(mongoDbSettings.Value.CategoriesCollectionName);
    }

    // Get All categories
    public async Task<List<Category>> GetCategoriesAsync() =>
        await _categoryCollection.Find(_ => true).ToListAsync();

    // Get only active categories where isActive = true
    public async Task<List<Category>> GetActiveCategoriesAsync() =>
        await _categoryCollection.Find(category => category.IsActive == true).ToListAsync();

    /// <summary>
    /// Get category for given id
    /// </summary>
    /// <param name="id">ObjectId of the category</param>
    /// <returns>Category?</returns>
    public async Task<Category?> GetCategoryByIdAsync(string id) =>
        await _categoryCollection.Find(category => category.Id == id).FirstOrDefaultAsync();

    /// <summary>
    /// Inserts a new category document
    /// </summary>
    /// <param name="newCategory">New Category Object</param>
    /// <returns></returns>
    public async Task CreateCategoryAsync(Category newCategory) =>
        await _categoryCollection.InsertOneAsync(newCategory);

    /// <summary>
    /// Update the category
    /// </summary>
    /// <param name="id">ObjectId of the category</param>
    /// <param name="updatedCategory">The updated category object</param>
    /// <returns></returns>
    public async Task UpdateCategoryAsync(string id, Category updatedCategory) =>
        await _categoryCollection.ReplaceOneAsync(category => category.Id == id, updatedCategory);

    /// <summary>
    /// Delete the category 
    /// </summary>
    /// <param name="id">ObjectId of the category</param>
    /// <returns></returns>
    public async Task DeleteCategoryAsync(string id) =>
        await _categoryCollection.DeleteOneAsync(category => category.Id == id);

    // Activate/Deactivate a category
    public async Task<UpdateResult> SetCategoryStatusAsync(string id, bool status)
    {
        var filter = Builders<Category>.Filter.Eq(p => p.Id, id);
        var update = Builders<Category>.Update.Set(p => p.IsActive, status);
        return await _categoryCollection.UpdateOneAsync(filter, update);
    }
}