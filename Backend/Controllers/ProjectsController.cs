using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;

namespace ProjectManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly MongoDbService _mongoService;



        public ProjectsController(MongoDbService mongoService)
        {
            {
                _mongoService = mongoService;
            }
        }

        // Helper property to get current user
        private string? UserEmail => HttpContext.Items["UserEmail"]?.ToString();

        // GET: api/projects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            if (UserEmail == null) return Unauthorized();

            var projects = await _mongoService.Projects.Find(_ => true).ToListAsync();
            return Ok(projects);
        }

        // GET: api/projects/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            if (UserEmail == null) return Unauthorized();

            var project = await _mongoService.Projects.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (project == null) return NotFound();
            return Ok(project);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<IActionResult> CreateProject(Project project)
        {
            if (UserEmail == null) return Unauthorized();

            await _mongoService.Projects.InsertOneAsync(project);
            return Ok(project);
        }

        // PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, Project updatedProject)
        {
            if (UserEmail == null) return Unauthorized();

            var filter = Builders<Project>.Filter.Eq(p => p.Id, id);
            var update = Builders<Project>.Update
                .Set(p => p.Name, updatedProject.Name)
                .Set(p => p.Description, updatedProject.Description)
                .Set(p => p.Members, updatedProject.Members);

            var result = await _mongoService.Projects.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0) return NotFound();

            return NoContent();

        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            if (UserEmail == null) return Unauthorized();

            var result = await _mongoService.Projects.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0) return NotFound();

            return NoContent();
        }
    }
}
