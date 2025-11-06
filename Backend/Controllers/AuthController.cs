using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbService _mongoService;
        private readonly IConfiguration _config;
        private readonly IRedisService _redisService;

        public AuthController(MongoDbService mongoService, IConfiguration config, IRedisService redisService)
        {
            _mongoService = mongoService;
            _config = config;
            _redisService = redisService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var existingUser = await _mongoService.Users.Find(u => u.Email == model.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest("User already exists");

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Role ?? "User",
                Department = model.Department
            };

            await _mongoService.Users.InsertOneAsync(user);
            return Ok("User registered successfully");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            string cacheKey = $"user:auth:{model.Email}";

            // Check Redis cache first
            var cachedUser = await _redisService.GetAsync<User>(cacheKey);
            if (cachedUser != null)
            {
                Console.WriteLine(" Returned user from Redis cache.");
                if (VerifyPassword(model.Password, cachedUser.PasswordHash))
                {
                    var token = GenerateJwtToken(cachedUser);
                    return Ok(new { token });
                }
                return Unauthorized("Invalid password");
            }

            // If not in cache, check MongoDB
            var user = await _mongoService.Users.Find(u => u.Email == model.Email).FirstOrDefaultAsync();
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var jwtToken = GenerateJwtToken(user);
            return Ok(new {
                token = jwtToken,
                userId = user.Id,
                email = user.Email,
                name=user.Name,
             

            });
        }

        // --- Hashing ---
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        // --- JWT ---
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Name),

                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    
    }
}
