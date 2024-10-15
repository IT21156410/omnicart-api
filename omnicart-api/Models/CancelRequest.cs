// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Model representing a cancelRequests document in MongoDB cancelRequests collection.
// Tutorial         : https://stackoverflow.com/questions/37776866/json-net-8-0-error-creating-stringenumconverter-on-mono-4-5-mac
//                    https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
// ***********************************************************************

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace omnicart_api.Models
{
    public class CancelRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [BsonElement("orderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; }

        [Required]
        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }

        [BsonElement("reason")]
        public string? Reason { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required]
        public CancelRequestStatus Status { get; set; } = CancelRequestStatus.Pending;

        [BsonElement("requestedDate")]
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    }

    public enum CancelRequestStatus
    {
        [EnumMember(Value = "Pending")]
        Pending, 

        [EnumMember(Value = "Approved")]
        Approved, 

        [EnumMember(Value = "Rejected")]
        Rejected 
    }

    public class CancelRequestDto
    {
        [Required]
        [BsonElement("reason")]
        public string Reason { get; set; }
    }

    public class ProcessCancelDto
    {
        [Required]
        [BsonElement("isApproved")]
        public bool IsApproved { get; set; }

        [Required]
        [BsonElement("request")]
        public CancelRequest Request { get; set; }
    }

}
