// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB products collection. 
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

        public ProductService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _productCollection = mongoDatabase.GetCollection<Product>(mongoDbSettings.Value.ProductsCollectionName);
        }

        // Create a new product
        public async Task CreateProductAsync(Product newProduct)
        {
            await _productCollection.InsertOneAsync(newProduct);
        }

        // Get a product by ID
        public async Task<Product?> GetProductByIdAsync(string id)
        {
            return await _productCollection.Find(product => product.Id == id).FirstOrDefaultAsync();
        }

        // Get a product by User ID
        public async Task<List<Product>> GetProductByUserIdAsync(string UserId)
        {
            return await _productCollection.Find(product => product.UserId == UserId).ToListAsync();
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
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$VendorInfo" },
                    { "preserveNullAndEmptyArrays", true }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    // Include all product fields by specifying each one or simply by using "1"
                    { "_id", 1 },
                    { "userId", 1 },
                    { "name", 1 },
                    { "category", 1 },
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
                    { "VendorInfo.role", 1 }
                })
            };

            var result = await _productCollection.Aggregate<Product>(pipeline).ToListAsync();
            return result;
        }

        // Update an existing product
        public async Task UpdateProductAsync(string id, Product updatedProduct)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update
                .Set(p => p.Name, updatedProduct.Name)
                .Set(p => p.Category, updatedProduct.Category)
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
        public async Task UpdateStockAsync(string id, int newStock)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            var update = Builders<Product>.Update.Set(p => p.Stock, newStock);
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
