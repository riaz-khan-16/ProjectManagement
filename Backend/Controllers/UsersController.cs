using Microsoft.AspNetCore.Mvc;     // provides mvc framework components
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services; 
using MongoDB.Driver;

namespace ProjectManagementAPI.Controllers
{
    [ApiController]                               //marks the class as a Web API controller
    [Route("api/[controller]")]                  // Defines the base route
    public class UsersController : ControllerBase   
    {
        private readonly MongoDbService _mongoService;  // hold a reference to your MongoDB service
        private readonly IRedisService _redisService; // Redis service

        public UsersController(MongoDbService mongoService, IRedisService redisService)   // constructor.
        {
            _mongoService = mongoService;
            _redisService = redisService;

        }
        
        // GET: /api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()  // for getting all users
        {

            const string cacheKey = "users:all";

            // Try to get from Redis first
            var cachedUsers = await _redisService.GetAsync<List<object>>(cacheKey);
            if (cachedUsers != null)
            {
                
                    Console.WriteLine("Returned from Redis cache.");
                    return Ok(cachedUsers);
                

            }


            // If not in cache, query MongoDB
            var users = await _mongoService.Users    // Accesses the Users collection from your MongoDB
                .Find(_ => true)                     // Finds all documents
                .Project(u => new                    // Selects specific fields you want from each document
                {
                    u.Id,
                    u.Email,
                    u.Role,
                    u.Department,
                    u.Name,

                })
                .ToListAsync();                   // converts the results to a list

            if (users.Count == 0)
                return NotFound("User data not found in database.");


            // Store in Redis cache for 10 minutes
            await _redisService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(10));
            Console.WriteLine("Returned from MongoDB and cached in Redis.");



            return Ok(users);
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {

            string cacheKey = $"user:{id}";

            // Try Redis first
            var cachedUser = await _redisService.GetAsync<object>(cacheKey);
            if (cachedUser != null){
                    Console.WriteLine("Returned from Redis cache.");
                    return Ok(cachedUser);
                }
            else
            {
                Console.WriteLine("User fetched from MongoDB");
            }





            var user = await _mongoService.Users
                .Find(u => u.Id == id)
                .Project(u => new
                {
                    u.Id,
                    u.Email,
                    u.Role,
                    u.Department
                })
                .FirstOrDefaultAsync();
            

            if (user == null)
                return NotFound(new { message = "User not found" });


            // Cache user profile for 10 minutes
            await _redisService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(10));
            Console.WriteLine("Returned from MongoDB and cached in Redis.");


            return Ok(user);
        }

        
    }
}
