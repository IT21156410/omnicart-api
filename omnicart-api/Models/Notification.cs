// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Model representing a user document in MongoDB notification collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using omnicart_api.Requests;

namespace omnicart_api.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; } = null!;

    [BsonElement("title")] public required string Title { get; set; }

    [BsonElement("message")] public required string Message { get; set; }

    [BsonElement("roles")]
    [BsonRepresentation(BsonType.String)] // Store the enum as a string in the database
    [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response
    // public Role[]? Roles { get; set; } = [];
    public Role? Roles { get; set; } = null;

    [BsonElement("isRead")] public bool IsRead { get; set; } = false;

    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationRequest
{
    [BsonElement("userId")] public string? UserId { get; set; } = null;

    [BsonElement("title")] public required string Title { get; set; }

    [BsonElement("message")] public required string Message { get; set; }

    [BsonElement("roles")]
    [BsonRepresentation(BsonType.String)] // Store the enum as a string in the database
    [JsonConverter(typeof(JsonStringEnumConverter))]
    // public Role[]? Roles { get; set; } = []; // 
    public Role? Roles { get; set; } = null;
}