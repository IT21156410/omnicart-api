// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handling data from MongoDB review collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class ReviewService
    {
        private readonly IMongoCollection<Review> _reviewCollection;

        /// <summary>
        /// Initializes the ReviewService with the MongoDB client, database, and review collection
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public ReviewService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _reviewCollection = mongoDatabase.GetCollection<Review>(mongoDbSettings.Value.ReviewsCollectionName);
        }

        /// <summary>
        /// Inserts a new review document into the review collection.
        /// </summary>
        /// <param name="newReview">New Review Object</param>
        /// <returns></returns>
        public async Task CreateReviewAsync(Review newReview) =>
            await _reviewCollection.InsertOneAsync(newReview);

        /// <summary>
        /// Updates a review document.
        /// </summary>
        /// <param name="id">ObjectId of the review</param>
        /// <param name="updatedReview">The updated review object</param>
        /// <returns></returns>
        public async Task UpdateReviewAsync(string id, Review updatedReview) =>
            await _reviewCollection.ReplaceOneAsync(review => review.Id == id, updatedReview);

        /// <summary>
        /// Gets a review by vendor ID.
        /// </summary>
        /// <param name="vendorId">Vendor ID of the review</param>
        /// <returns>List of reviews for the specified vendor</returns>
        public async Task<List<Review>> GetReviewsByVendorIdAsync(string vendorId) =>
            await _reviewCollection.Find(review => review.VendorId == vendorId).ToListAsync();

        /// <summary>
        /// Gets a review by customer ID.
        /// </summary>
        /// <param name="customerId">Customer ID of the review</param>
        /// <returns>List of reviews by the specified customer</returns>
        public async Task<List<Review>> GetReviewsByCustomerIdAsync(string customerId) =>
            await _reviewCollection.Find(review => review.CustomerId == customerId).ToListAsync();

        /// <summary>
        /// Updates the average rating for a vendor based on the reviews.
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns></returns>
        public async Task UpdateVendorAverageRatingAsync(string vendorId)
        {
            var reviews = await GetReviewsByVendorIdAsync(vendorId);
            if (reviews.Count > 0)
            {
                var averageRating = (decimal)reviews.Average(r => r.Rating);
                // TODO: Update vendor average rating logic here, user -> vendor details
            }
        }

        /// <summary>
        /// Gets a review by its ID.
        /// </summary>
        /// <param name="reviewId">The ID of the review to retrieve</param>
        /// <returns>The Review object if found, otherwise null</returns>
        public async Task<Review?> GetReviewByIdAsync(string reviewId)
        {
            return await _reviewCollection.Find(review => review.Id == reviewId).FirstOrDefaultAsync();
        }

        // Delete an review by ID
        public async Task DeleteReviewAsync(string id)
        {
            await _reviewCollection.DeleteOneAsync(review => review.Id == id);
        }

    }
}
