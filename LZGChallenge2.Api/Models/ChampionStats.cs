using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.Api.Models;

public class ChampionStats
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("playerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PlayerId { get; set; } = null!;
    
    [BsonElement("championId")]
    public int ChampionId { get; set; }
    
    [BsonElement("championName")]
    public string ChampionName { get; set; } = null!;
    
    [BsonElement("gamesPlayed")]
    public int GamesPlayed { get; set; }
    
    [BsonElement("wins")]
    public int Wins { get; set; }
    
    [BsonElement("losses")]
    public int Losses { get; set; }
    
    [BsonIgnore]
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;
    
    // KDA pour ce champion
    [BsonElement("totalKills")]
    public double TotalKills { get; set; }
    
    [BsonElement("totalDeaths")]
    public double TotalDeaths { get; set; }
    
    [BsonElement("totalAssists")]
    public double TotalAssists { get; set; }
    
    [BsonIgnore]
    public double AverageKills => GamesPlayed > 0 ? TotalKills / GamesPlayed : 0;
    
    [BsonIgnore]
    public double AverageDeaths => GamesPlayed > 0 ? TotalDeaths / GamesPlayed : 0;
    
    [BsonIgnore]
    public double AverageAssists => GamesPlayed > 0 ? TotalAssists / GamesPlayed : 0;
    
    [BsonIgnore]
    public double KDA => TotalDeaths > 0 ? (TotalKills + TotalAssists) / TotalDeaths : TotalKills + TotalAssists;
    
    // Autres stats moyennes pour ce champion
    [BsonElement("averageCreepScore")]
    public double AverageCreepScore { get; set; }
    
    [BsonElement("averageVisionScore")]
    public double AverageVisionScore { get; set; }
    
    [BsonElement("averageDamageDealt")]
    public double AverageDamageDealt { get; set; }
    
    [BsonElement("averageGameDuration")]
    public double AverageGameDuration { get; set; }
    
    [BsonElement("lastPlayedAt")]
    public DateTime LastPlayedAt { get; set; }
    
    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [BsonIgnore]
    public Player Player { get; set; } = null!;
}