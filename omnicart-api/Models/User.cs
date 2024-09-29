using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace omnicart_api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("email")]
        public string email { get; set; }

        public string password { get; set; }

        [BsonElement("role")]
        public string role { get; set; }
    }
}
