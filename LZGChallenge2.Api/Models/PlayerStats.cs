using System.ComponentModel.DataAnnotations;

namespace LZGChallenge2.Api.Models;

public class PlayerStats
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    
    // Rang actuel
    [MaxLength(20)]
    public string? CurrentTier { get; set; }
    
    [MaxLength(5)]
    public string? CurrentRank { get; set; }
    
    public int CurrentLeaguePoints { get; set; }
    
    // Stats générales
    public int TotalGames { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    
    public double WinRate => TotalGames > 0 ? (double)TotalWins / TotalGames * 100 : 0;
    
    // KDA
    public double TotalKills { get; set; }
    public double TotalDeaths { get; set; }
    public double TotalAssists { get; set; }
    
    public double AverageKills => TotalGames > 0 ? TotalKills / TotalGames : 0;
    public double AverageDeaths => TotalGames > 0 ? TotalDeaths / TotalGames : 0;
    public double AverageAssists => TotalGames > 0 ? TotalAssists / TotalGames : 0;
    public double KDA => TotalDeaths > 0 ? (TotalKills + TotalAssists) / TotalDeaths : TotalKills + TotalAssists;
    
    // Autres moyennes
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageGameDuration { get; set; }
    public double AverageDamageDealt { get; set; }
    
    // Séries
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int LongestWinStreak { get; set; }
    public int LongestLoseStreak { get; set; }
    
    // Points de progression (calculé basé sur les gains/pertes de LP)
    public int TotalLpGained { get; set; }
    public int TotalLpLost { get; set; }
    public int NetLpChange => TotalLpGained - TotalLpLost;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}