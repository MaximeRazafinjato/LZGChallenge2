using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.Api.Models;

public class PasswordResetToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("usedAt")]
    public DateTime? UsedAt { get; set; }

    [BsonElement("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [BsonIgnore]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [BsonIgnore]
    public bool IsUsed => UsedAt != null;

    [BsonIgnore]
    public bool IsValid => !IsExpired && !IsUsed;
}