// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Model representing a product document in MongoDB products collection.
// Tutorial         : https://stackoverflow.com/questions/37776866/json-net-8-0-error-creating-stringenumconverter-on-mono-4-5-mac
//                    https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
// ***********************************************************************

using Amazon.Auth.AccessControlPolicy;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace omnicart_api.Models
{
    public class Product
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        [BsonElement("VendorInfo")]
        public UserDto? VendorInfo { get; set; } = null;

        [BsonElement("categoryId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string? CategoryId { get; set; }

        [BsonElement("name")]
        [Required]
        public string Name { get; set; } = null!;

        [BsonElement("category")]
        public Category? Category { get; set; } = null!;

        [BsonElement("photos")]
        public List<string> Photos { get; set; } = [];

        [BsonElement("condition")]
        [BsonRepresentation(BsonType.String)]  // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Condition Condition { get; set; } = Condition.New;  // Default to 'New'

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]  // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response
        [Required]
        public Status Status { get; set; } = Status.Pending;  // Default to 'Pending'

        [BsonElement("description")]
        [Required]
        public required string Description { get; set; }

        [BsonElement("stock")]
        [Required]
        public int Stock { get; set; } = 0;

        [BsonElement("sku")]
        [Required]
        [StringLength(13, ErrorMessage = "The SKU must be a string with the length less than 13.")]
        public string SKU { get; set; } = null!;

        [BsonElement("price")]
        [Required]
        [BsonRepresentation(BsonType.Double)]
        public double Price { get; set; } = 0.0;

        [BsonElement("discount")]
        [Required]
        [BsonRepresentation(BsonType.Double)]
        public double Discount { get; set; } = 0.0;

        [BsonElement("productWeight")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double ProductWeight { get; set; } = 0.0;

        [BsonElement("width")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double Width { get; set; } = 0.0;

        [BsonElement("height")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double Height { get; set; } = 0.0;

        [BsonElement("length")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double Length { get; set; } = 0.0;

        [BsonElement("shippingFee")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double ShippingFee { get; set; } = 0.0;
    }

    public enum Condition
    {
        New,            // Product is new
        Used,           // Product is used 
    }

    public enum Status
    {
        [EnumMember(Value = "Pending")]
        Pending,        // Product is pending for admin approval

        [EnumMember(Value = "Active")]
        Active,         // Product is available for purchase

        [EnumMember(Value = "Inactive")]
        Inactive,       // Product is not currently available 

        [EnumMember(Value = "Rejected")]
        Rejected        // Product is rejected by admin
    }

    public class UpdateProductStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public Status Status { get; set; }
    }
    public class UpdateProductStockDto
    {
        [Required]
        public int Stock { get; set; }
    }

}
