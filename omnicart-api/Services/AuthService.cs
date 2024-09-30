
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
using System.Security.Cryptography;

namespace omnicart_api.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly string _jwtSecret;
        private readonly int _jwtLifespan;

        public AuthService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.UsersCollectionName);
            _jwtSecret = "OmnicartAPI@jwtSecret";
            _jwtLifespan = 1440; //minutes
        }

        public async Task<string?> LoginAsync(LoginRequest loginRequest)
        {
            var email = loginRequest.Email;
            var password = loginRequest.Password;

            var loginUser = await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();

            if (loginUser == null || !VerifyPassword(password, loginUser.Password))
            {
                return null;
            }

            return GenerateJwtToken(loginUser);
        }

        public async Task<User?> RegisterAsync(RegisterRequest registerRequest)
        {
            var existingUser = await _userCollection.Find(user => user.Email == registerRequest.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return null;
            }

            var newUser = new User
            {
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                Password = HashPassword(registerRequest.Password),
                Role = registerRequest.Role
            };

            await _userCollection.InsertOneAsync(newUser);

            newUser.Password = null;
            return newUser;
        }

        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            var user = await _userCollection.Find(user => user.Email == email).FirstOrDefaultAsync();

            if (user == null) return false;

            // Generate reset token
            var resetToken = GeneratePasswordResetToken(user);

            var emailService = new EmailService();
            await emailService.SendPasswordResetAsync(user.Email, resetToken);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest resetRequest)
        {
            var resetPswUser = await _userCollection.Find(user => user.Email == resetRequest.Email).FirstOrDefaultAsync();

            if (resetPswUser == null || !VerifyPasswordResetToken(resetPswUser, resetRequest.Token))
            {
                return false;
            }

            resetPswUser.Password = HashPassword(resetRequest.NewPassword);

            await _userCollection.ReplaceOneAsync(user => user.Id == user.Id, resetPswUser);

            return true;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest)
        {
            var changePswUser = await _userCollection.Find(user => user.Email == changePasswordRequest.Email).FirstOrDefaultAsync();

            if (changePswUser == null || !VerifyPassword(changePasswordRequest.CurrentPassword, changePswUser.Password))
            {
                return false;
            }

            changePswUser.Password = HashPassword(changePasswordRequest.NewPassword);

            await _userCollection.ReplaceOneAsync(user => user.Id == changePswUser.Id, changePswUser);

            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == storedHash;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
                // new Claim("role", user.Role) 
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

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[32];
                rng.GetBytes(tokenData);

                var userInfo = user.Id + DateTime.UtcNow.ToString();
                var token = Convert.ToBase64String(tokenData) + Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfo));

                user.PasswordReset.Token = token;
                user.PasswordReset.ExpiryAt = DateTime.UtcNow.AddHours(1); 

                await _userService.UpdateUserAsync(user.Id, user);

                return token;
            }
        }

        private bool VerifyPasswordResetToken(User user, string token)
        {
            return (user.PasswordReset.Token == token) && (user.PasswordReset.ExpiryAt > DateTime.UtcNow);
        }

    }
}
