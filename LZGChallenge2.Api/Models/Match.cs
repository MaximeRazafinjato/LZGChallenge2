using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.Api.Models;

public class Match
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("matchId")]
    public string MatchId { get; set; } = null!;
    
    [BsonElement("playerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PlayerId { get; set; } = null!;
    
    [BsonElement("gameStartTime")]
    public DateTime GameStartTime { get; set; }
    
    [BsonElement("gameEndTime")]
    public DateTime GameEndTime { get; set; }
    
    [BsonElement("gameDuration")]
    public long GameDuration { get; set; } // en secondes
    
    [BsonElement("win")]
    public bool Win { get; set; }
    
    [BsonElement("queueId")]
    public int QueueId { get; set; } = 420; // Solo/Duo Queue uniquement
    
    [BsonElement("season")]
    public int Season { get; set; } // Saison League of Legends (ex: 14 pour la saison 2024)
    
    // Stats du joueur dans cette partie
    [BsonElement("kills")]
    public int Kills { get; set; }
    
    [BsonElement("deaths")]
    public int Deaths { get; set; }
    
    [BsonElement("assists")]
    public int Assists { get; set; }
    
    [BsonElement("championId")]
    public int ChampionId { get; set; }
    
    [BsonElement("championName")]
    public string ChampionName { get; set; } = null!;
    
    [BsonElement("position")]
    public string Position { get; set; } = null!; // TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY
    
    [BsonElement("level")]
    public int Level { get; set; }
    
    [BsonElement("totalDamageDealtToChampions")]
    public long TotalDamageDealtToChampions { get; set; }
    
    [BsonElement("totalDamageTaken")]
    public long TotalDamageTaken { get; set; }
    
    [BsonElement("goldEarned")]
    public long GoldEarned { get; set; }
    
    [BsonElement("creepScore")]
    public int CreepScore { get; set; }
    
    [BsonElement("visionScore")]
    public int VisionScore { get; set; }
    
    // Infos sur le rang/LP (si disponible)
    [BsonElement("tier")]
    public string? Tier { get; set; }
    
    [BsonElement("rank")]
    public string? Rank { get; set; }
    
    [BsonElement("leaguePoints")]
    public int? LeaguePoints { get; set; }
    
    [BsonElement("lpChange")]
    public int? LpChange { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties - ignorées pour MongoDB
    [BsonIgnore]
    public Player Player { get; set; } = null!;
}