// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB products collection. 
// Tutorial         : https://www.mongodb.com/docs/drivers/csharp/upcoming/fundamentals/aggregation/
//                    https://stackoverflow.com/questions/29569289/using-and-in-the-pipeline-for-mongodb-aggregate-function-driver-in-c-sharp
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _productCollection;

        private readonly NotificationService _notificationService;

        public ProductService(IOptions<MongoDbSettings> mongoDbSettings, NotificationService notificationService)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _productCollection = mongoDatabase.GetCollection<Product>(mongoDbSettings.Value.ProductsCollectionName);

            _notificationService = notificationService;
        }

        // Create a new product
        public async Task CreateProductAsync(Product newProduct)
        {
            await _productCollection.InsertOneAsync(newProduct);
        }

        // Get a product by ID
        public async Task<Product?> GetProductByIdAsync(string id)
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "categories" },
                    { "localField", "categoryId" },
                    { "foreignField", "_id" },
                    { "as", "category" }
                }),

                new BsonDocument("$match", new BsonDocument
                {
                    { "_id", ObjectId.Parse(id) }
                }),


                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$category" },
                    { "preserveNullAndEmptyArrays", true }
                }),
            };
            return await _productCollection.Aggregate<Product>(pipeline).FirstOrDefaultAsync();
        }

        // Get a product by User ID
        public async Task<List<Product>> GetProductByForeignIdAsync(string userId, string foreignMatchProperty = "userId", bool filterOutOfStock = false)
        {
            var pipeline = new List<BsonDocument>
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "categories" },
                    { "localField", "categoryId" },
                    { "foreignField", "_id" },
                    { "as", "category" }
                }),

                new BsonDocument("$match", new BsonDocument
                {
                    { foreignMatchProperty, ObjectId.Parse(userId) }
                }),


                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$category" },
                    { "preserveNullAndEmptyArrays", true }
                }),
            };

            if (filterOutOfStock)
            {
                var outOfStock = new BsonDocument("$match", new BsonDocument
                {
                    { "stock", new BsonDocument("$lte", 0) } // Stock <= 0
                });

                pipeline.Add(outOfStock);
            }

            return await _productCollection.Aggregate<Product>(pipeline).ToListAsync();
        }

        // Get all products
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var pipeline = new[]
            {
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userId" },
                    { "foreignField", "_id" },
                    { "as", "VendorInfo" }
                }),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "categories" },
                    { "localField", "categoryId" },
                    { "foreignField", "_id" },
                    { "as", "category" }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$VendorInfo" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$category" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    // Include all product fields by specifying each one or simply by using "1"
                    { "_id", 1 },
                    { "userId", 1 },
                    { "name", 1 },
                    { "categoryId", 1 },
                    { "photos", 1 },
                    { "condition", 1 },
                    { "status", 1 },
                    { "description", 1 },
                    { "stock", 1 },
                    { "sku", 1 },
                    { "price", 1 },
                    { "discount", 1 },
                    { "productWeight", 1 },
                    { "width", 1 },
                    { "height", 1 },
                    { "length", 1 },
                    { "shippingFee", 1 },

                    { "VendorInfo._id", 1 },
                    { "VendorInfo.name", 1 },
                    { "VendorInfo.email", 1 },
                    { "VendorInfo.role", 1 },

                    { "category._id", 1 },
                    { "category.name", 1 },
                    { "category.isActive", 1 },
                    { "category.image", 1 },
                })
            };

            var result = await _productCollection.Aggregate<Product>(pipeline).ToListAsync();
            return result;
        }

        // Search and filter products based on the provided criteria
        public async Task<List<Product>> SearchProductsAsync(
            string? name = null,
            string? category = null,
            double? minPrice = null,
            double? maxPrice = null,
            string? vendor = null,
            int? minRating = null,
            int? maxRating = null,
            string? sortBy = null,
            string sortDirection = "asc")
        {
            // Build a filter for the search query
            var filter = Builders<Product>.Filter.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                filter &= Builders<Product>.Filter.Regex("name", new BsonRegularExpression(name, "i")); // Case-insensitive name search
            }

            if (!string.IsNullOrEmpty(category))
            {
                filter &= Builders<Product>.Filter.Eq("categoryId", ObjectId.Parse(category)); // Match by category ID
            }

            if (minPrice.HasValue)
            {
                filter &= Builders<Product>.Filter.Gte("price", minPrice);
            }

            if (maxPrice.HasValue)
            {
                filter &= Builders<Product>.Filter.Lte("price", maxPrice);
            }

            if (!string.IsNullOrEmpty(vendor))
            {
                filter &= Builders<Product>.Filter.Eq("userId", ObjectId.Parse(vendor)); // Match by vendor
            }

            if (minRating.HasValue)
            {
                filter &= Builders<Product>.Filter.Gte("ratings", minRating.Value);
            }

            if (maxRating.HasValue)
            {
                filter &= Builders<Product>.Filter.Lte("ratings", maxRating.Value);
            }

            // Sorting logic
            var sortDefinition = Builders<Product>.Sort.Ascending("name"); // default sort

            if (!string.IsNullOrEmpty(sortBy))
            {
                // Sort by vendor-specific ratings or price
                if (sortBy.ToLower() == "price" || sortBy.ToLower() == "ratings")
                    sortDefinition = sortDirection.ToLower() == "desc" ? Builders<Product>.Sort.Descending(sortBy) : Builders<Product>.Sort.Ascending(sortBy);
                else
                    sortDefinition = Builders<Product>.Sort.Ascending("name");
            }

            // Fetch the filtered and sorted products
            return await _productCollection
                .Find(filter)
                .Sort(sortDefinition)
                .ToListAsync();
        }

        // Update an existing product
        public async Task UpdateProductAsync(string id, Product updatedProduct)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update
                .Set(p => p.Name, updatedProduct.Name)
                .Set(p => p.CategoryId, updatedProduct.CategoryId)
                .Set(p => p.Photos, updatedProduct.Photos)
                .Set(p => p.Condition, updatedProduct.Condition)
                .Set(p => p.Status, updatedProduct.Status)
                .Set(p => p.Stock, updatedProduct.Stock)
                .Set(p => p.SKU, updatedProduct.SKU)
                .Set(p => p.Price, updatedProduct.Price)
                .Set(p => p.Discount, updatedProduct.Discount)
                .Set(p => p.ProductWeight, updatedProduct.ProductWeight)
                .Set(p => p.Width, updatedProduct.Width)
                .Set(p => p.Height, updatedProduct.Height)
                .Set(p => p.Length, updatedProduct.Length)
                .Set(p => p.ShippingFee, updatedProduct.ShippingFee);

            await _productCollection.UpdateOneAsync(filter, update);
        }

        // Delete a product by ID
        public async Task DeleteProductAsync(string id)
        {
            await _productCollection.DeleteOneAsync(product => product.Id == id);
        }

        // Activate/Deactivate a product
        public async Task<UpdateResult> SetProductStatusAsync(string id, Status newStatus)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update.Set(p => p.Status, newStatus);
            return await _productCollection.UpdateOneAsync(filter, update);
        }

        // Manage stock: Add/Remove stock
        public async Task UpdateStockAsync(Product product, int newStock)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            var update = Builders<Product>.Update.Set(p => p.Stock, newStock);


            // Send notification if stock is low
            if (newStock <= 10) // Define your stock alert threshold
            {
                var notification = new NotificationRequest
                {
                    UserId = product.UserId, // Product belongs vendor userId
                    Title = "Low Stock Alert",
                    Message = $"Stock for {product.Name} is low. Only {product.Stock} items left.",
                    Roles = null
                };
                await _notificationService.CreateNotificationAsync(notification);
            }

            await _productCollection.UpdateOneAsync(filter, update);
        }

        // Get low stock products
        public async Task<List<Product>> GetLowStockProductsAsync(int threshold)
        {
            var filter = Builders<Product>.Filter.Lt(p => p.Stock, threshold);
            return await _productCollection.Find(filter).ToListAsync();
        }
    }
}