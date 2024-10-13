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
        private readonly IMongoCollection<CancelRequest> _cancelRequestsCollection;

        private readonly NotificationService _notificationService;

        public OrderService(IOptions<MongoDbSettings> mongoDbSettings, NotificationService notificationService)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _orderCollection = mongoDatabase.GetCollection<Order>(mongoDbSettings.Value.OrdersCollectionName);
            _cancelRequestsCollection = mongoDatabase.GetCollection<CancelRequest>(mongoDbSettings.Value.CancelRequestCollectionName);

            _notificationService = notificationService;
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

        // Get user's order by ID 
        public async Task<Order?> GetUserOrderByIdAsync(string userId, string orderId)
        {
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(order => order.UserId, userId),
                Builders<Order>.Filter.Eq(order => order.Id, orderId)
            );
            return await _orderCollection.Find(filter).FirstOrDefaultAsync();
        }

        // Get orders by Vendor ID
        public async Task<List<Order>> GetOrdersByVendorIdAsync(string vendorId)
        {
            // Filter orders where at least one item has the matching vendorId
            var filter = Builders<Order>.Filter.ElemMatch(order => order.Items, item => item.VendorId == vendorId);

            var orders = await _orderCollection.Find(filter).ToListAsync();
            return orders;
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
                    { "as", "VendorInfo" }
                }),

                // Unwind the UserInfo array
                new BsonDocument("$unwind", new BsonDocument
                {
                    { "path", "$VendorInfo" },
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
                    { "status", 1 },
                    { "paymentStatus", 1 },
                    { "totalAmount", 1 },
                    { "shippingAddress", 1 },
                    { "items", 1 },
                    { "shippingFee", 1 },
                    { "note", 1 },

                    // Include user information
                    { "VendorInfo._id", 1 },
                    { "VendorInfo.name", 1 },
                    { "VendorInfo.email", 1 }
                })
            };

            var result = await _orderCollection.Aggregate<Order>(pipeline).ToListAsync();
            return result;
        }

        // Update order
        public async Task<UpdateResult> UpdateOrderAsync(Order order)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, order.Id);
            var update = Builders<Order>.Update
                .Set(o => o.Status, order.Status)
                .Set(o => o.Items, order.Items)
                .Set(o => o.Note, order.Note);

            return await _orderCollection.UpdateOneAsync(filter, update);
        }

        // Update an order's status (e.g., Processing, Shipped, Delivered)
        public async Task<UpdateResult> UpdateOrderStatusAsync(Order existingOrder, OrderStatus newStatus, string? note)
        {
            var filter = Builders<Order>.Filter.Eq(order => order.Id, existingOrder.Id);
            var update = Builders<Order>.Update.Set(order => order.Status, newStatus);
            if (!string.IsNullOrEmpty(note))
            {
                update = update.Set(order => order.Note, note);
            }

            if (newStatus == OrderStatus.Cancelled)
            {
                var notification = new NotificationRequest
                {
                    UserId = existingOrder.UserId,
                    Title = "Order canceled",
                    Message = note ?? $"Your order #{existingOrder.Id} has been cancelled.",
                    Roles = null,
                };
                await _notificationService.CreateNotificationAsync(notification);
            }

            return await _orderCollection.UpdateOneAsync(filter, update);
        }

        // Update an order's payment status (e.g., Paid, Failed)
        public async Task<UpdateResult> UpdatePaymentStatusAsync(string id, PaymentStatus newStatus)
        {
            var filter = Builders<Order>.Filter.Eq(order => order.Id, id);
            var update = Builders<Order>.Update.Set(order => order.PaymentStatus, newStatus);
            return await _orderCollection.UpdateOneAsync(filter, update);
        }

        // Order histry by user
        public async Task<List<Order>> GetOrderHistoryByUserIdAsync(string userId)
        {
            var filter = Builders<Order>.Filter.Eq(order => order.UserId, userId);
            return await _orderCollection.Find(filter).ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByProductIdAsync(string productId, List<OrderStatus> statuses)
        {
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.ElemMatch(o => o.Items, i => i.ProductId == productId), // Check if the product is in the order items
                Builders<Order>.Filter.In(o => o.Status, statuses) // Check if the order is in one of the given statuses
            );

            return await _orderCollection.Find(filter).ToListAsync();
        }


        // Delete an order by ID
        public async Task DeleteOrderAsync(string id)
        {
            await _orderCollection.DeleteOneAsync(order => order.Id == id);
        }

        // Order cancellation request
        public async Task CreateRequestAsync(CancelRequest request)
        {
            var notification = new NotificationRequest
            {
                UserId = null,
                Title = "New order cancellation requested",
                Message = $"Order #{request.OrderId} has been requested to be cancelled. Please review the cancellation request",
                Roles = Role.csr,
            };
            await _notificationService.CreateNotificationAsync(notification);


            await _cancelRequestsCollection.InsertOneAsync(request);
        }

        // Get a cancellation request by ID
        public async Task<CancelRequest?> GetRequestByIdAsync(string requestId)
        {
            return await _cancelRequestsCollection.Find(request => request.Id == requestId).FirstOrDefaultAsync();
        }

        // Update cancellation request status
        public async Task UpdateRequestAsync(CancelRequest request)
        {
            var filter = Builders<CancelRequest>.Filter.Eq(x => x.Id, request.Id);
            var update = Builders<CancelRequest>.Update
                .Set(x => x.Status, request.Status)
                .Set(x => x.RequestedDate, request.RequestedDate);

            await _cancelRequestsCollection.UpdateOneAsync(filter, update);
        }

        // Get all cancellation requests
        public async Task<List<CancelRequest>> GetAllCancellationRequestsAsync()
        {
            return await _cancelRequestsCollection.Find(request => true).ToListAsync();
        }


        // Generate a Unique Order Number
        public string GenerateOrderNumber()
        {
            return $"ORD-{Guid.NewGuid().ToString().Substring(0, 6)}";
        }
    }
}