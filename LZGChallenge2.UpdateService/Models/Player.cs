using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.UpdateService.Models;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("riotId")]
    public string RiotId { get; set; } = null!;
    
    [BsonElement("gameName")]
    public string GameName { get; set; } = null!;
    
    [BsonElement("tagLine")]
    public string TagLine { get; set; } = null!;
    
    [BsonElement("puuid")]
    public string Puuid { get; set; } = null!;
    
    [BsonElement("summonerId")]
    public string SummonerId { get; set; } = null!;
    
    [BsonElement("region")]
    public string Region { get; set; } = null!;
    
    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}