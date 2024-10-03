// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handling data from MongoDB users collection for authentication.
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// Tutorial         : https://medium.com/@siva.veeravarapu/jwt-token-authentication-in-c-a-beginners-guide-with-code-snippets-7545f4c7c597
// ***********************************************************************

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using omnicart_api.Models;
using omnicart_api.Services;
using System.Security.Cryptography;
using System.Text;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json.Linq;

namespace omnicart_api.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly string _jwtSecret;
        private readonly int _jwtLifespan;
        private readonly UserService _userService;

        /// <summary>
        /// Initializes the AuthService with MongoDB client, database, and users collection.
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public AuthService(IOptions<MongoDbSettings> mongoDbSettings, UserService userService)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
            _jwtSecret = "OmnicartAPI@jwtSecretIT211699080PRASHANTHAKGMSLIITEADASSIGMENT01C#DOTNETRESRAPI";
            _jwtLifespan = 1440; //minutes
            _userService = userService;
        }

        /// <summary>
        /// Authenticates the user by verifying email and password.
        /// </summary>
        /// <param name="loginRequest">The login credentials</param>
        /// <returns>JWT Token if successful, null if failed</returns>
        public async Task<AuthResponse?> LoginAsync(LoginRequest loginRequest)
        {
            var email = loginRequest.Email;
            var password = loginRequest.Password;

            var loginUser = await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();

            if (loginUser == null || !VerifyPassword(password, loginUser.Password))
            {
                return null;
            }

            var token = await GenerateJwtTokenAsync(loginUser);
            await Generate2FAVerifyTokenAsync(loginUser);

            return new AuthResponse(new UserDto(loginUser), token);
        }

        /// <summary>
        /// Registers a new user by creating a new user account with the provided credentials.
        /// </summary>
        /// <param name="registerRequest">The registration details of the new user</param>
        /// <returns>The created user's information if successful, null if the registration fails</returns>
        public async Task<AuthResponse?> RegisterAsync(RegisterRequest registerRequest)
        {
            var existingUser = await _userCollection.Find(user => user.Email == registerRequest.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return null;
            }

            var createUser = new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                Password = HashPassword(registerRequest.Password),
                Role = registerRequest.Role
            };

            await _userCollection.InsertOneAsync(createUser);

            User newUser = await _userCollection.Find(user => user.Email == registerRequest.Email).FirstOrDefaultAsync();

            var token = await GenerateJwtTokenAsync(newUser);
            await Generate2FAVerifyTokenAsync(newUser);

            newUser.Password = "";

            return new AuthResponse(new UserDto(newUser), token);
        }

        /// <summary>
        /// Sends a password reset email to the user with a reset token.
        /// </summary>
        /// <param name="email">User's email</param>
        /// <returns>True if email was sent, false if user not found</returns>
        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            try
            {
                var user = await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();

                if (user == null) return false;

                var resetToken = await GeneratePasswordResetTokenAsync(user);

                //var emailService = new EmailService();
                //await emailService.SendPasswordResetAsync(user.Email, resetToken);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate password reset: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="resetRequest">Reset password details including token and new password</param>
        /// <returns>True if successful, false if token is invalid or user not found</returns>
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest resetRequest)
        {
            var resetPswUser = await _userCollection.Find(user => user.Email == resetRequest.Email).FirstOrDefaultAsync();

            if (resetPswUser == null || resetPswUser.PasswordReset == null)
            {
                return false;
            }

            if (!(resetPswUser.PasswordReset.Token == resetRequest.Token) && (resetPswUser.PasswordReset.ExpiryAt > DateTime.UtcNow))
            {
                return false;
            }

            resetPswUser.Password = HashPassword(resetRequest.NewPassword);
            resetPswUser.PasswordReset.IsReseted = true;
            resetPswUser.PasswordReset.ExpiryAt = DateTime.UtcNow;

            await _userCollection.ReplaceOneAsync(user => user.Id == resetPswUser.Id, resetPswUser);

            return true;
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="changePasswordRequest">Password change request containing old and new password</param>
        /// <returns>True if password changed successfully, false if old password is incorrect</returns>
        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest)
        {
            //var changePswUser = await _userCollection.Find(user => user.Email == changePasswordRequest.Email).FirstOrDefaultAsync();

            //if (changePswUser == null || !VerifyPassword(changePasswordRequest.CurrentPassword, changePswUser.Password))
            //{
            //    return false;
            //}

            //changePswUser.Password = HashPassword(changePasswordRequest.NewPassword);

            //await _userCollection.ReplaceOneAsync(user => user.Id == changePswUser.Id, changePswUser);

            return true;
        }

        public async Task<bool> VerifyTwoFactorAsync(VerifyTwoFactorRequest verifyTwoFactorRequest)
        {
            var user2AF = await _userCollection.Find(user => user.Email == verifyTwoFactorRequest.Email).FirstOrDefaultAsync();

            if (user2AF == null || user2AF.TwoFAVerify == null)
            {
                return false;
            }

            if (user2AF.TwoFAVerify.Code != verifyTwoFactorRequest.Code)
            {
                return false;
            }

            if (DateTime.UtcNow > user2AF.TwoFAVerify.ExpiryAt)
            {
                return false;
            }

            user2AF.TwoFAVerify.IsVerified = true;
            user2AF.TwoFAVerify.ExpiryAt = DateTime.UtcNow;

            await _userCollection.ReplaceOneAsync(user => user.Id == user2AF.Id, user2AF);

            return true;
        }



        // Utility methods for hashing, token generation, and verification
        // 
        /// <summary>
        /// Hashes the given password using SHA256.
        /// </summary>
        /// <param name="password">The plain-text password to be hashed</param>
        /// <returns>The hashed password as a Base64-encoded string</returns>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Verifies if the input password matches the stored hashed password.
        /// </summary>
        /// <param name="password">The plain-text password entered by the user</param>
        /// <param name="storedHash">The previously stored hashed password</param>
        /// <returns>True if the password matches the stored hash, otherwise false</returns>
        private static bool VerifyPassword(string password, string storedHash)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == storedHash;
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user asynchronously.
        /// </summary>
        /// <param name="user">The user object for whom the token is being generated</param>
        /// <returns>A JWT token as a string</returns>
        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // set unique identifier
                    // new Claim("role", user.Role) // Custom set user role
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "OmnicartAPI",
                    audience: "Omnicart_WEB_APP",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(_jwtLifespan),
                    signingCredentials: credentials
                );

                // Wrap in Task.FromResult for async compatibility
                return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                //return string.Empty;
                throw new Exception($"Failed to generate JWT token: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a password reset token for the given user and stores it in the database with an expiration time.
        /// </summary>
        /// <param name="user">The user object for whom the token is being generated</param>
        /// <returns>The generated token as a string</returns>
        private async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenData = new byte[32];
            rng.GetBytes(tokenData);

            var userInfo = user.Id + DateTime.UtcNow.ToString();
            var combinedData = Convert.ToBase64String(tokenData) + Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfo));

            // Make the token URL-safe
            var urlFriendlyToken = combinedData
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            user.PasswordReset = new PasswordReset
            {
                Token = urlFriendlyToken,
                ExpiryAt = DateTime.UtcNow.AddHours(1),
            };

            await _userService.UpdateUserAsync(user.Id!, user);

            return urlFriendlyToken;
        }

        /// <summary>
        /// Generates a two-factor authentication (2FA) verification token for the given user and stores it in the database with an expiration time.
        /// </summary>
        /// <param name="user">The user object for whom the token is being generated</param>
        /// <returns>The generated token as a string</returns>
        public async Task<string> Generate2FAVerifyTokenAsync(User user)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] randomNumber = new byte[4];
            rng.GetBytes(randomNumber);

            // Convert the random number to a uint, then ensure it's in the 6-digit range
            uint code = BitConverter.ToUInt32(randomNumber, 0) % 900000 + 100000;

            user.TwoFAVerify = new TwoFAVerify
            {
                Code = code.ToString(),
                ExpiryAt = DateTime.UtcNow.AddMinutes(5), // token valid for 5 minutes
                IsVerified = false
            };

            await _userService.UpdateUserAsync(user.Id!, user);

            return code.ToString();
        }

    }
}
