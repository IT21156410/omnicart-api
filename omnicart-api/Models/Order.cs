// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Model representing a order document in MongoDB orders collection.
// Tutorial         : https://stackoverflow.com/questions/37776866/json-net-8-0-error-creating-stringenumconverter-on-mono-4-5-mac
//                    https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
// ***********************************************************************

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace omnicart_api.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; } = null;

        [BsonElement("VendorInfo")]
        public UserDto? VendorInfo { get; set; } = null;

        [BsonElement("orderNumber")]
        [Required]
        public string OrderNumber { get; set; } = null!;

        [BsonElement("orderDate")]
        [Required]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime OrderDate { get; set; }

        [BsonElement("totalAmount")]
        [Required]
        [BsonRepresentation(BsonType.Double)]
        public double TotalAmount { get; set; } = 0.0;

        [BsonElement("shippingAddress")]
        [Required]
        public string ShippingAddress { get; set; } = null!;

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]  // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))]  // Serialize enum as string in JSON response
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;  // Default to 'Pending'

        [BsonElement("items")]
        [Required]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        [BsonElement("shippingFee")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double ShippingFee { get; set; } = 0.0;

        [BsonElement("paymentStatus")]
        [BsonRepresentation(BsonType.String)]  // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))]  // Serialize enum as string in JSON response
        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;  // Default to 'Pending'

        [BsonElement("note")]
        public string? Note { get; set; }  // Optional note
    }

    public class OrderItem
    {
        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string ProductId { get; set; } = null!;

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string VendorId { get; set; } = null!;

        [BsonElement("quantity")]
        [Required]
        public int Quantity { get; set; } = 1;

        [BsonElement("unitPrice")]
        [BsonRepresentation(BsonType.Double)]
        [Required]
        public double UnitPrice { get; set; } = 0.0;

        [BsonElement("totalPrice")]
        [BsonRepresentation(BsonType.Double)]
        public double TotalPrice { get { return UnitPrice * Quantity; } }  // Calculated value

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;  // Default to 'Pending'
    }

    public enum OrderStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,  // Order placed but not yet processed

        [EnumMember(Value = "Processing")]
        Processing,  // Order is being processed

        [EnumMember(Value = "Shipped")]
        Shipped,  // Order shipped but not yet delivered

        [EnumMember(Value = "PartiallyDelivered")]
        PartiallyDelivered,  // Order delivered by multiple Vendors

        [EnumMember(Value = "Delivered")]
        Delivered,  // Order delivered to the customer

        [EnumMember(Value = "Cancelled")]
        Cancelled  // Order canceled
    }

    public enum PaymentStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,  // Payment is pending

        [EnumMember(Value = "Paid")]
        Paid,  // Payment received

        [EnumMember(Value = "Failed")]
        Failed  // Payment failed
    }

    public class UpdateOrderStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public OrderStatus Status { get; set; }
    }

    public class CancelOrderDto
    {
        [Required]
        public string Note { get; set; }
    }

    public class UpdateOrderItemStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public OrderStatus Status { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public PaymentStatus PaymentStatus { get; set; }
    }

    public class UpdateOrderDto
    {
        [Required]
        [BsonElement("shippingAddress")]
        public string? ShippingAddress { get; set; }

        [BsonElement("items")]
        public List<UpdateOrderItemDto>? Items { get; set; }

        [BsonElement("note")]
        public string? Note { get; set; }
    }
    public class UpdateOrderItemDto
    {
        [Required]
        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; }

        [Required]
        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [Required]
        [BsonElement("unitPrice")]
        public double UnitPrice { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)] // Enum stored as a string
        public OrderStatus Status { get; set; }
    }


    public class PurchaseRequest
    {
        public string? ShippingAddress { get; set; }
    }

}
