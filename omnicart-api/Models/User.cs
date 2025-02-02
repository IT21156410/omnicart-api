﻿// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Model representing a user document in MongoDB users collection.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;


namespace omnicart_api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")] public required string Name { get; set; }

        [BsonElement("email")] public required string Email { get; set; }

        [BsonElement("password")] public required string Password { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)] // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response
        [Required]
        public required Role Role { get; set; } = Role.customer;

        [BsonElement("passwordReset")] public PasswordReset? PasswordReset { get; set; }

        [BsonElement("twoFAVerify")] public TwoFAVerify? TwoFAVerify { get; set; }

        [BsonElement("isActive")] public bool IsActive { get; set; } = false;

        [BsonElement("cart")] public List<CartItem> Cart { get; set; } = new List<CartItem>();

        [BsonElement("shippingAddress")] public string? ShippingAddress { get; set; } = String.Empty;
    }

    public class PasswordReset
    {
        [BsonElement("token")] public required string Token { get; set; }

        [BsonElement("expiryAt")] public required DateTime ExpiryAt { get; set; }

        [BsonElement("isReseted")] public bool IsReseted { get; set; }
    }

    public class TwoFAVerify
    {
        [BsonElement("code")] public required string Code { get; set; }

        [BsonElement("expiryAt")] public required DateTime ExpiryAt { get; set; }

        [BsonElement("isVerified")] public bool IsVerified { get; set; }
    }

    public class CartItem
    {
        private int _quantity;
        private double _unitPrice;

        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = null!;

        [BsonElement("vendorId")]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string VendorId { get; set; } = null!;

        [BsonElement("quantity")]
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                TotalPrice = _quantity * _unitPrice; // Automatically update TotalPrice when Quantity changes
            }
        }

        [BsonElement("unitPrice")]
        public double UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                TotalPrice = _quantity * _unitPrice; // Automatically update TotalPrice when UnitPrice changes
            }
        }

        [BsonElement("totalPrice")] public double TotalPrice { get; private set; } // Automatically calculated TotalPrice
    }


    public class CartItemDto
    {
        [Required] public string ProductId { get; set; } = null!;

        public string? VendorId { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be a positive value.")]
        public double UnitPrice { get; set; }
    }

    public class UserDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")] public string Name { get; set; }

        [BsonElement("email")] public string Email { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)] // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response
        public Role Role { get; set; } = Role.customer;

        public UserDto(User user)
        {
            Id = user.Id!;
            Name = user.Name;
            Email = user.Email;
            Role = user.Role;
        }
    }

    public class CreateUserDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required] public string Name { get; set; } = null!;

        [Required] public string Email { get; set; } = null!;

        [Required] public string Password { get; set; } = null!;

        [Required] public Role Role { get; set; } = Role.customer;
    }

    public class UpdateUserDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)] // Store the enum as a string in the database
        [JsonConverter(typeof(JsonStringEnumConverter))] // Serialize enum as string in JSON response
        [Required]
        public required Role Role { get; set; } = Role.customer;
    }

    public class UpdateProfileUserDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
 
    }

    public class UpdateUserStatusDto
    {
        [Required] [BsonElement("isActive")] public bool IsActive { get; set; } = false;
    }

    public enum Role
    {
        [EnumMember(Value = "admin")] admin,

        [EnumMember(Value = "vendor")] vendor,

        [EnumMember(Value = "csr")] csr,

        [EnumMember(Value = "customer")] customer,
    }
}