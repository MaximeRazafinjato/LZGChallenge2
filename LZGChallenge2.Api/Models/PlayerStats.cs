using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LZGChallenge2.Api.Models;

public class PlayerStats
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("playerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PlayerId { get; set; } = null!;
    
    // Rang actuel
    [BsonElement("currentTier")]
    public string? CurrentTier { get; set; }
    
    [BsonElement("currentRank")]
    public string? CurrentRank { get; set; }
    
    [BsonElement("currentLeaguePoints")]
    public int CurrentLeaguePoints { get; set; }
    
    // Stats générales
    [BsonElement("totalGames")]
    public int TotalGames { get; set; }
    
    [BsonElement("totalWins")]
    public int TotalWins { get; set; }
    
    [BsonElement("totalLosses")]
    public int TotalLosses { get; set; }
    
    [BsonIgnore]
    public double WinRate => TotalGames > 0 ? (double)TotalWins / TotalGames * 100 : 0;
    
    // KDA
    [BsonElement("totalKills")]
    public double TotalKills { get; set; }
    
    [BsonElement("totalDeaths")]
    public double TotalDeaths { get; set; }
    
    [BsonElement("totalAssists")]
    public double TotalAssists { get; set; }
    
    [BsonIgnore]
    public double AverageKills => TotalGames > 0 ? TotalKills / TotalGames : 0;
    
    [BsonIgnore]
    public double AverageDeaths => TotalGames > 0 ? TotalDeaths / TotalGames : 0;
    
    [BsonIgnore]
    public double AverageAssists => TotalGames > 0 ? TotalAssists / TotalGames : 0;
    
    [BsonIgnore]
    public double KDA => TotalGames > 0 && TotalDeaths > 0 ? (TotalKills + TotalAssists) / TotalDeaths : 
                         TotalGames > 0 ? TotalKills + TotalAssists : 0;
    
    // Autres moyennes
    [BsonElement("averageCreepScore")]
    public double AverageCreepScore { get; set; }
    
    [BsonElement("averageVisionScore")]
    public double AverageVisionScore { get; set; }
    
    [BsonElement("averageGameDuration")]
    public double AverageGameDuration { get; set; }
    
    [BsonElement("averageDamageDealt")]
    public double AverageDamageDealt { get; set; }
    
    // Séries
    [BsonElement("currentWinStreak")]
    public int CurrentWinStreak { get; set; }
    
    [BsonElement("currentLoseStreak")]
    public int CurrentLoseStreak { get; set; }
    
    [BsonElement("longestWinStreak")]
    public int LongestWinStreak { get; set; }
    
    [BsonElement("longestLoseStreak")]
    public int LongestLoseStreak { get; set; }
    
    // Points de progression (calculé basé sur les gains/pertes de LP)
    [BsonElement("totalLpGained")]
    public int TotalLpGained { get; set; }
    
    [BsonElement("totalLpLost")]
    public int TotalLpLost { get; set; }
    
    [BsonIgnore]
    public int NetLpChange => TotalLpGained - TotalLpLost;
    
    [BsonElement("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [BsonIgnore]
    public Player Player { get; set; } = null!;
}