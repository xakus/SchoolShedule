using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Data;
using UserService.Models;

namespace UserService.Services
{
    /// <summary>
    /// Сервис для регистрации, логина, генерации токена и работы с Redis
    /// </summary>
    public class AuthService
    {
        private readonly UserDbContext _db;
        private readonly IConfiguration _config;
        private readonly IDatabase _redis;

        public AuthService(UserDbContext db, IConfiguration config, IConnectionMultiplexer redis)
        {
            _db = db;
            _config = config;
            _redis = redis.GetDatabase();
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        public async Task<(bool Success, string? Error)> RegisterAsync(string email, string password)
        {
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return (false, "Пользователь с таким email уже существует");
            var passwordHash = HashPassword(password);
            var user = new User { Email = email, PasswordHash = passwordHash };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        /// <summary>
        /// Авторизация пользователя
        /// </summary>
        public async Task<(bool Success, string? Error, string? Token)> LoginAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return (false, "Неверный email или пароль", null);
            var token = GenerateJwtToken(user);
            // Кешируем в Redis: ключ = токен, значение = JSON-объект пользователя
            var userData = new { user.Id, user.Email, user.CreatedAt };
            string userJson = System.Text.Json.JsonSerializer.Serialize(userData);
            await _redis.StringSetAsync(token, userJson, TimeSpan.FromHours(24));
            return (true, null, token);
        }

        /// <summary>
        /// Хеширование пароля с использованием PBKDF2
        /// </summary>
        private string HashPassword(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256);
            var salt = deriveBytes.Salt;
            var key = deriveBytes.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        /// <summary>
        /// Проверка пароля
        /// </summary>
        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var keyToCheck = deriveBytes.GetBytes(32);
            return keyToCheck.SequenceEqual(key);
        }

        /// <summary>
        /// Генерация JWT токена для пользователя
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(int.Parse(jwtSection["ExpiresInHours"] ?? "24")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
