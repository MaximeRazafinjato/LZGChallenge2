using System.ComponentModel.DataAnnotations;

namespace LZGChallenge2.Api.Models;

public class RoleStats
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Position { get; set; } = null!; // TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY
    
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;
    public double PlayRate { get; set; } // Pourcentage de parties jouées dans ce rôle
    
    // KDA pour ce rôle
    public double TotalKills { get; set; }
    public double TotalDeaths { get; set; }
    public double TotalAssists { get; set; }
    
    public double AverageKills => GamesPlayed > 0 ? TotalKills / GamesPlayed : 0;
    public double AverageDeaths => GamesPlayed > 0 ? TotalDeaths / GamesPlayed : 0;
    public double AverageAssists => GamesPlayed > 0 ? TotalAssists / GamesPlayed : 0;
    public double KDA => TotalDeaths > 0 ? (TotalKills + TotalAssists) / TotalDeaths : TotalKills + TotalAssists;
    
    // Stats spécifiques au rôle
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}