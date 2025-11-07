using MongoDB.Driver;
using ProjectManagementAPI.Models;       //  models namespace
using ProjectManagementAPI.Settings;     // MongoDBSettings namespace
using Microsoft.Extensions.Options;

namespace ProjectManagementAPI.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Project> Projects => _database.GetCollection<Project>("Projects");
        public IMongoCollection<TaskItem> Tasks => _database.GetCollection<TaskItem>("Tasks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<TeamChatMessage> TeamChatMessages => _database.GetCollection<TeamChatMessage>("TeamChatMessages");
    }
}
