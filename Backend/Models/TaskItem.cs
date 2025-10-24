using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProjectManagementAPI.Models;
using System;

public class TaskItem
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string Assignee { get; set; } = string.Empty;
    public DateTime DueDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonRepresentation(BsonType.String)] // Store foreign key as string
    public Guid ProjectId { get; set; }

    [BsonIgnore]
    public Project? Project { get; set; }
}
