using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamChatController : ControllerBase
    {
        private readonly MongoDbService _mongoService;
        private readonly IRedisService _redisService;

        public TeamChatController(MongoDbService mongoService, IRedisService redisService)
        {
            _mongoService = mongoService;
            _redisService = redisService;
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetChatHistory(Guid projectId)
        {
            // at first trying from cache
            string cacheKey = $"teamchat:{projectId}";  // cache key
            var cachedMessages = await _redisService.GetAsync<List<TeamChatMessage>>(cacheKey);
            if (cachedMessages != null)
            {
                return Ok(cachedMessages);
            }

            // If not in Redis, fetching from MongoDB
            var messages = await _mongoService.TeamChatMessages
                .Find(m => m.ProjectId == projectId)
                .Sort(Builders<TeamChatMessage>.Sort.Descending(m => m.Timestamp))
                .Limit(5)
                .ToListAsync();
                
            messages.Reverse();

            // Caching them in Redis for 10 minutes
            await _redisService.SetAsync(cacheKey, messages, TimeSpan.FromMinutes(1));

            return Ok(messages);

        }
    }
}
