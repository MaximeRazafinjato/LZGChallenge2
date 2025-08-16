using System.ComponentModel.DataAnnotations;

namespace LZGChallenge2.Api.Models;

public class ChampionStats
{
    public int Id { get; set; }
    
    public int PlayerId { get; set; }
    
    public int ChampionId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ChampionName { get; set; } = null!;
    
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    
    public double WinRate => GamesPlayed > 0 ? (double)Wins / GamesPlayed * 100 : 0;
    
    // KDA pour ce champion
    public double TotalKills { get; set; }
    public double TotalDeaths { get; set; }
    public double TotalAssists { get; set; }
    
    public double AverageKills => GamesPlayed > 0 ? TotalKills / GamesPlayed : 0;
    public double AverageDeaths => GamesPlayed > 0 ? TotalDeaths / GamesPlayed : 0;
    public double AverageAssists => GamesPlayed > 0 ? TotalAssists / GamesPlayed : 0;
    public double KDA => TotalDeaths > 0 ? (TotalKills + TotalAssists) / TotalDeaths : TotalKills + TotalAssists;
    
    // Autres stats moyennes pour ce champion
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    public double AverageGameDuration { get; set; }
    
    public DateTime LastPlayedAt { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}