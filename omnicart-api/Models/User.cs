// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Model representing a user document in MongoDB users collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

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
        public required string Name { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        public required string Password { get; set; }

        [BsonElement("role")]
        public required string Role { get; set; }

        [BsonElement("passwordReset")]
        public PasswordReset? PasswordReset { get; set; }

        [BsonElement("twoFAVerify")]
        public TwoFAVerify? TwoFAVerify { get; set; }
    }

    public class PasswordReset
    {
        [BsonElement("token")]
        public required string Token { get; set; }

        [BsonElement("expiryAt")]
        public required DateTime ExpiryAt { get; set; }

        [BsonElement("isReseted")]
        public bool IsReseted { get; set; }
    }

    public class TwoFAVerify
    {
        [BsonElement("code")]
        public required string Code { get; set; }

        [BsonElement("expiryAt")]
        public required DateTime ExpiryAt { get; set; }

        [BsonElement("isVerified")]
        public bool IsVerified { get; set; }
    }

    public class UserDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        public UserDto(User user)
        {
            Id = user.Id!;
            Name = user.Name;
            Email = user.Email;
            Role = user.Role;
        }
    }


    public class UpdateUserDto
    {
        public required string Name { get; set; } 
        public required string Email { get; set; }  
        public string? Role { get; set; }  
 
    }
}
