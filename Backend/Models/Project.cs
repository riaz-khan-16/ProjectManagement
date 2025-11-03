using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Store Guid as string
        public Guid Id { get; set; } = Guid.NewGuid();  // global unique id auto generated

        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new List<string>();

        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        // List of user IDs who are members of this project
        [BsonElement("userIds")]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
