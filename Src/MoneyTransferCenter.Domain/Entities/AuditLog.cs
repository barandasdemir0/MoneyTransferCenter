using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MoneyTransferCenter.Domain.Entities;

public sealed class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)] // Veritabanında string olarak saklar ama C# kodunda Guid olarak kullanırız.
    public Guid UserId { get; set; }


    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;


    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;


    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;


    [BsonElement("entityId")]
    public string EntityId { get; set; } = string.Empty;


    [BsonElement("oldValue")]
    public string? OldValue { get; set; }


    [BsonElement("newValue")]
    public string? NewValue { get; set; }


    [BsonElement("details")]
    public string? Details { get; set; }


    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }


    [BsonElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
