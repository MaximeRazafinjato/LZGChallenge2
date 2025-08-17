using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.Api.Models;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }

    [BsonElement("replacedByToken")]
    public string? ReplacedByToken { get; set; }

    [BsonElement("reasonRevoked")]
    public string? ReasonRevoked { get; set; }

    [BsonElement("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [BsonElement("userAgent")]
    public string UserAgent { get; set; } = string.Empty;

    [BsonIgnore]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [BsonIgnore]
    public bool IsRevoked => RevokedAt != null;

    [BsonIgnore]
    public bool IsActive => !IsRevoked && !IsExpired;
}