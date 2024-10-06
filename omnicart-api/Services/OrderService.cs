// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handling data from MongoDB order collection for order management.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public OrderService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _orderCollection = mongoDatabase.GetCollection<Order>(mongoDbSettings.Value.OrdersCollectionName);
        }

        // Create a new order
        public async Task CreateOrderAsync(Order newOrder)
        {
            await _orderCollection.InsertOneAsync(newOrder);
        }

        // Get an order by ID
        public async Task<Order?> GetOrderByIdAsync(string id)
        {
            return await _orderCollection.Find(order => order.Id == id).FirstOrDefaultAsync();
        }

        // Get orders by Vendor ID
        public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId)
        {
            //var filter = Builders<Order>.Filter.ElemMatch(order => order.Items, item => item.VendorId == vendorId);
            //return await _orderCollection.Find(filter).ToListAsync();

            var pipeline = new[]
            {
                // Match orders with the specified VendorId
                new BsonDocument("$match", new BsonDocument
                {
                    { "userId", ObjectId.Parse(vendorId) }
                }),

                // Lookup user information
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userId" },
                    { "foreignField", "_id" },
                    { "as", "UserInfo" }
                }),

                // Unwind the UserInfo array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$UserInfo" },
                    { "preserveNullAndEmptyArrays", true }
                }),

                // Project necessary fields
                new BsonDocument("$project", new BsonDocument
                {
                    // Include order fields
                    { "_id", 1 },
                    { "userId", 1 },
                    { "orderNumber", 1 },
                    { "orderDate", 1 },
                    { "orderStatus", 1 },
                    { "paymentStatus", 1 },
                    { "totalAmount", 1 },
                    { "shippingAddress", 1 },
                    { "items", 1 },
                    { "shippingFee", 1 },
                    { "note", 1 },

                    // Include user information
                    { "UserInfo._id", 1 },
                    { "UserInfo.name", 1 },
                    { "UserInfo.email", 1 }
                })
            };

            var result = await _orderCollection.Aggregate<Order>(pipeline).ToListAsync();
            return result;

        }

        // Get all orders
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var pipeline = new[]
            {
                // Lookup to get user information from "users" collection
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "users" },
                    { "localField", "userId" },
                    { "foreignField", "_id" },
                    { "as", "UserInfo" }
                }),

                // Unwind the UserInfo array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$UserInfo" },
                    { "preserveNullAndEmptyArrays", true }
                }),

                // Project only the necessary fields (from both orders and users)
                new BsonDocument("$project", new BsonDocument
                {
                    // Include order fields
                    { "_id", 1 },
                    { "userId", 1 },
                    { "orderNumber", 1 },
                    { "orderDate", 1 },
                    { "orderStatus", 1 },
                    { "paymentStatus", 1 },
                    { "totalAmount", 1 },
                    { "shippingAddress", 1 },
                    { "items", 1 }, 
                    { "shippingFee", 1 },
                    { "note", 1 },

                    // Include user information
                    { "UserInfo._id", 1 },
                    { "UserInfo.name", 1 },
                    { "UserInfo.email", 1 }
                })
            };

            var result = await _orderCollection.Aggregate<Order>(pipeline).ToListAsync();
            return result;
        }

        // Update an order's status (e.g., Processing, Shipped, Delivered)
        public async Task<UpdateResult> UpdateOrderStatusAsync(string id, OrderStatus newStatus)
        {
            var filter = Builders<Order>.Filter.Eq(order => order.Id, id);
            var update = Builders<Order>.Update.Set(order => order.OrderStatus, newStatus);
            return await _orderCollection.UpdateOneAsync(filter, update);
        }

        // Update an order's payment status (e.g., Paid, Failed)
        public async Task<UpdateResult> UpdatePaymentStatusAsync(string id, PaymentStatus newStatus)
        {
            var filter = Builders<Order>.Filter.Eq(order => order.Id, id);
            var update = Builders<Order>.Update.Set(order => order.PaymentStatus, newStatus);
            return await _orderCollection.UpdateOneAsync(filter, update);
        }

        // Delete an order by ID
        public async Task DeleteOrderAsync(string id)
        {
            await _orderCollection.DeleteOneAsync(order => order.Id == id);
        }
    }
}
