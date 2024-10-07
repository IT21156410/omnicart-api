// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handling data from MongoDB vendor collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class VendorService
    {
        private readonly IMongoCollection<Vendor> _vendorCollection;

        /// <summary>
        /// Initializes the VendorService with the MongoDB client, database, and vendors collection
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public VendorService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _vendorCollection = mongoDatabase.GetCollection<Vendor>(mongoDbSettings.Value.VendorsCollectionName);
        }

        /// <summary>
        /// Inserts a new vendor document into the vendor collection.
        /// </summary>
        /// <param name="newVendor">New Vendor Object</param>
        /// <returns></returns>
        public async Task CreateVendorAsync(Vendor newVendor) =>
            await _vendorCollection.InsertOneAsync(newVendor);

        /// <summary>
        /// Updates the vendor document.
        /// </summary>
        /// <param name="id">ObjectId of the vendor</param>
        /// <param name="updatedVendor">The updated vendor object</param>
        /// <returns></returns>
        public async Task UpdateVendorAsync(string id, Vendor updatedVendor) =>
            await _vendorCollection.ReplaceOneAsync(vendor => vendor.Id == id, updatedVendor);

        /// <summary>
        /// Gets a vendor by user ID.
        /// </summary>
        /// <param name="userId">User ID of the vendor</param>
        /// <returns>Vendor object</returns>
        public async Task<Vendor> GetVendorByUserIdAsync(string userId) =>
            await _vendorCollection.Find(vendor => vendor.UserId == userId).FirstOrDefaultAsync();

        /// <summary>
        /// Updates vendor ranking information.
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="newComment">The new comment object including rating to update</param>
        /// <returns></returns>
        public async Task UpdateVendorRankingAsync(string vendorId, VendorComment newComment)
        {
            var vendor = await _vendorCollection.Find(v => v.Id == vendorId).FirstOrDefaultAsync();

            if (vendor != null)
            {
                vendor.Comments.Add(newComment);

                // Calculate new average rating and convert to decimal
                if (vendor.Comments.Count > 0)
                {
                    vendor.AverageRating = (decimal)vendor.Comments.Average(c => c.Rating);
                }

                await _vendorCollection.ReplaceOneAsync(v => v.Id == vendorId, vendor);
            }
        }

    }
}
