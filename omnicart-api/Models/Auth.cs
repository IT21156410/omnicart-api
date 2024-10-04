// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Auth related classes as types/models.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace omnicart_api.Models
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]  // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response

        public Role Role { get; set; } = Role.customer;
        public string? AdminToken { get; set; }
    }

    public class AuthResponse
    {
        public UserDto User { get; set; }
        public string Token { get; set; }

        public AuthResponse(UserDto user, string token)
        {
            User = user;
            Token = token;
        }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class VerifyTwoFactorRequest
    {
        public required string Email { get; set; }

        public required string Code { get; set; }
    }

    public class SendTwoFactorRequest
    {
        public required string Email { get; set; }
    }

}


