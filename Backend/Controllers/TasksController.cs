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
        private readonly IRedisService _redisService;
       

        public TasksController(MongoDbService mongoService, 
            IRedisService redisService)
        {
            _mongoService = mongoService;
            _redisService = redisService;
            
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            const string cacheKey = "tasks:all";

            // Try from Redis first
            var cachedTasks = await _redisService.GetAsync<List<TaskItem>>(cacheKey);
            if (cachedTasks != null)
            {
                Console.WriteLine(" Returned all tasks from Redis cache.");
                return Ok(cachedTasks);
            }


            // Fallback to MongoDB
            var tasks = await _mongoService.Tasks.Find(_ => true).ToListAsync();


            if (tasks.Count > 0)
                await _redisService.SetAsync(cacheKey, tasks, TimeSpan.FromMinutes(10));

            Console.WriteLine("Returned from MongoDB and cached in Redis.");
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {

            string cacheKey = $"task:{id}";

            var cachedTask = await _redisService.GetAsync<TaskItem>(cacheKey);
            if (cachedTask != null)
            {
                Console.WriteLine(" Returned single task from Redis cache.");
                return Ok(cachedTask);
            }


            var task = await _mongoService.Tasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return NotFound();


            await _redisService.SetAsync(cacheKey, task, TimeSpan.FromMinutes(10));
            Console.WriteLine(" Task cached in Redis.");


            return Ok(task);
        }

        // GET: api/tasks/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(Guid projectId)
        {
            string cacheKey = $"tasks:project:{projectId}";

            var cachedTasks = await _redisService.GetAsync<List<TaskItem>>(cacheKey);
            if (cachedTasks != null)
            {
                Console.WriteLine(" Returned project tasks from Redis cache.");
                return Ok(cachedTasks);
            }




            var tasks = await _mongoService.Tasks.Find(t => t.ProjectId == projectId).ToListAsync();


            if (tasks.Count > 0)
                await _redisService.SetAsync(cacheKey, tasks, TimeSpan.FromMinutes(10));

            Console.WriteLine(" Cached project tasks in Redis.");


            return Ok(tasks);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItem task)
        {
            var project = await _mongoService.Projects.Find(p => p.Id == task.ProjectId).FirstOrDefaultAsync();
            if (project == null) return BadRequest("Project not found");

            await _mongoService.Tasks.InsertOneAsync(task);

            // Invalidate related caches
            await _redisService.RemoveAsync("tasks:all");
            await _redisService.RemoveAsync($"tasks:project:{task.ProjectId}");

            Console.WriteLine("Task created and caches cleared.");

          

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

            // Invalidate caches
            await _redisService.RemoveAsync("tasks:all");
            await _redisService.RemoveAsync($"task:{id}");
            await _redisService.RemoveAsync($"tasks:project:{updatedTask.ProjectId}");

            Console.WriteLine(" Task updated and caches cleared.");

            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var task = await _mongoService.Tasks.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (task == null) return NotFound();

            var result = await _mongoService.Tasks.DeleteOneAsync(t => t.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            
            // Invalidate caches
            await _redisService.RemoveAsync("tasks:all");
            await _redisService.RemoveAsync($"task:{id}");
            await _redisService.RemoveAsync($"tasks:project:{task.ProjectId}");


            Console.WriteLine(" Task deleted and caches cleared.");
            return NoContent();
        }
    }
}
