// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Model representing a review document in MongoDB reviews collection.
// Tutorial         : https://stackoverflow.com/questions/37776866/json-net-8-0-error-creating-stringenumconverter-on-mono-4-5-mac
//                    https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
// ***********************************************************************

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace omnicart_api.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string VendorId { get; set; } = null!;  // referencing Vendor

        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string CustomerId { get; set; } = null!;  // referencing Customer

        [BsonElement("comment")]
        [Required]
        public string Comment { get; set; } = null!;

        [BsonElement("rating")]
        [Required]
        public int Rating { get; set; }  // Rating from 1 to 5

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReviewCreateDto
    {
        [Required]
        public string VendorId { get; set; } = null!;

        [Required]
        public string CustomerId { get; set; } = null!;

        [Required]
        public string Comment { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
