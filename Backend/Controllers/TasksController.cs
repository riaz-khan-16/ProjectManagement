using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using MongoDB.Driver;

namespace ProjectManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly MongoDbService _mongoService;

        public TasksController(MongoDbService mongoService)
        {
            _mongoService = mongoService;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _mongoService.Tasks.Find(_ => true).ToListAsync();
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var task = await _mongoService.Tasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return NotFound();
            return Ok(task);
        }

        // GET: api/tasks/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(Guid projectId)
        {
            var tasks = await _mongoService.Tasks.Find(t => t.ProjectId == projectId).ToListAsync();
            return Ok(tasks);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItem task)
        {
            var project = await _mongoService.Projects.Find(p => p.Id == task.ProjectId).FirstOrDefaultAsync();
            if (project == null) return BadRequest("Project not found");

            await _mongoService.Tasks.InsertOneAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, TaskItem updatedTask)
        {
            var filter = Builders<TaskItem>.Filter.Eq(t => t.Id, id);
            var update = Builders<TaskItem>.Update
                .Set(t => t.Title, updatedTask.Title)
                .Set(t => t.Description, updatedTask.Description)
                .Set(t => t.Status, updatedTask.Status)
                .Set(t => t.Assignee, updatedTask.Assignee)
                .Set(t => t.DueDate, updatedTask.DueDate);

            var result = await _mongoService.Tasks.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0) return NotFound();

            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var result = await _mongoService.Tasks.DeleteOneAsync(t => t.Id == id);
            if (result.DeletedCount == 0) return NotFound();

            return NoContent();
        }
    }
}
