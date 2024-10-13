// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handling data from MongoDB notifications collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using omnicart_api.Models;

namespace omnicart_api.Services;

public class NotificationService
{
    private readonly IMongoCollection<Notification> _notificationCollection;

    public NotificationService(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _notificationCollection = mongoDatabase.GetCollection<Notification>(mongoDbSettings.Value.NotificationsCollectionName);
    }

    // Create a new notification
    public async Task CreateNotificationAsync(NotificationRequest data)
    {
        var notification = new Notification
        {
            UserId = data.UserId,
            Title = data.Title,
            Message = data.Message,
            // Roles = string.IsNullOrWhiteSpace(data.UserId) ? [] : data.Roles,
            Roles = !string.IsNullOrWhiteSpace(data.UserId) ? null : data.Roles,
            IsRead = false,
        };
        // Console.WriteLine(notification.Roles);

        await _notificationCollection.InsertOneAsync(notification);
    }

    // Get all notifications for a user
    public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
    {
        return await _notificationCollection.Find(n => n.UserId == userId).ToListAsync();
    }

    // Get notifications for a specific user roles
    public async Task<List<Notification>> GetNotificationsForUserAsync(string userRoles)
    {
        var filter = Builders<Notification>.Filter.Eq("roles", userRoles);

        return await _notificationCollection.Find(filter).ToListAsync();
    }

    // Get notifications for a specific user or their roles
    public async Task<List<Notification>> GetNotificationsForUserOrRoleAsync(string userId, string? userRole)
    {
        // Match notifications that are either for the specific userId or for the roles the user belongs to
        var filter = Builders<Notification>.Filter.Or(
            Builders<Notification>.Filter.Eq(n => n.UserId, userId),
            Builders<Notification>.Filter.Eq("roles", userRole)
        );
        var sortDefinition = Builders<Notification>.Sort.Descending("createdAt");
        return await _notificationCollection.Find(filter).Sort(sortDefinition).ToListAsync();
    }

    // Mark a notification as read
    public async Task MarkNotificationAsReadAsync(string notificationId)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notificationCollection.UpdateOneAsync(filter, update);
    }

    public async Task<long> GetUnreadNotificationCountAsync(string? userId, string? userRole)
    {
        // Build the filter for unread notifications
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter.Eq(n => n.IsRead, false),
            Builders<Notification>.Filter.Or(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq("roles", userRole)
            )
        );

        // Return the count of unread notifications
        return await _notificationCollection.CountDocumentsAsync(filter);
    }
}