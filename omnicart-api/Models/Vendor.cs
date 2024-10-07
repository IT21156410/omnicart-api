// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Model representing a vendor document in MongoDB vendors collection.
// Tutorial         : https://stackoverflow.com/questions/37776866/json-net-8-0-error-creating-stringenumconverter-on-mono-4-5-mac
//                    https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
// ***********************************************************************

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace omnicart_api.Models
{
    public class Vendor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string UserId { get; set; } = null!;  // referencing User

        [BsonElement("businessName")]
        [Required]
        public string BusinessName { get; set; } = null!;

        [BsonElement("averageRating")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal AverageRating { get; set; } = 0.0M;

        [BsonElement("comments")]
        public List<VendorComment> Comments { get; set; } = new List<VendorComment>();  // List of customer comments and ratings
    }

    public class VendorComment
    {
        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; } = null!;  // Customer's UserId

        [BsonElement("comment")]
        public string Comment { get; set; } = null!;

        [BsonElement("rating")]
        public int Rating { get; set; }  // Rating from 1 to 5

        [BsonElement("createdAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
