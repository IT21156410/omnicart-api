// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Model representing a product category document in MongoDB categories collection.
// Tutorial         : 
// ***********************************************************************

using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace omnicart_api.Models;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null!;

    [Required][BsonElement("name")] public string Name { get; set; } = null!;

    [BsonElement("isActive")] public bool IsActive { get; set; } = true;

    [BsonElement("image")] public string? Image { get; set; }
}

public class CategoryDto
{
    [Required][BsonElement("name")] public string Name { get; set; } = null!;

    [BsonElement("isActive")] public bool IsActive { get; set; } = true;

    [BsonElement("image")] public string? Image { get; set; }
}

public class CategoryStatusDto
{
    [BsonElement("isActive")] public bool IsActive { get; set; } = true;
}