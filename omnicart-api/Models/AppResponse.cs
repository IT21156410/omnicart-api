// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API response formatting. 
// ***********************************************************************

using MongoDB.Bson.Serialization.Attributes;

namespace omnicart_api.Models
{
    public class AppResponse<T>
    {
        [BsonElement("success")] public bool Success { get; set; }

        [BsonElement("data")] public T? Data { get; set; }

        [BsonElement("message")] public required string Message { get; set; }

        [BsonElement("error")] public string? Error { get; set; }

        [BsonElement("errorCode")] public int? ErrorCode { get; set; }

        [BsonElement("errorData")] public object? ErrorData { get; set; }
    }
}