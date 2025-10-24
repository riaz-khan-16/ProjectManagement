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

        public UsersController(MongoDbService mongoService)   // constructor.
        {
            _mongoService = mongoService;  
        }
        
        // GET: /api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()  // for getting all users
        {
            var users = await _mongoService.Users    // Accesses the Users collection from your MongoDB
                .Find(_ => true)                     // Finds all documents
                .Project(u => new                    // Selects specific fields you want from each document
                {
                    u.Id,
                    u.Email,
                    u.Role,
                    u.Department
                })
                .ToListAsync();                   // converts the results to a list

            if (users.Count == 0)
                return NotFound("User data not found in database.");

            return Ok(users);
        }

        // GET: /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(Guid id)
        {
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

            return Ok(user);
        }

        
    }
}
