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

        public TeamChatController(MongoDbService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetChatHistory(Guid projectId)
        {
            var messages = await _mongoService.TeamChatMessages
                .Find(m => m.ProjectId == projectId)
                .SortBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
