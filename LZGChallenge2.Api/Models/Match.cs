using System.ComponentModel.DataAnnotations;

namespace LZGChallenge2.Api.Models;

public class Match
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(15)]
    public string MatchId { get; set; } = null!;
    
    public int PlayerId { get; set; }
    
    public DateTime GameStartTime { get; set; }
    
    public DateTime GameEndTime { get; set; }
    
    public long GameDuration { get; set; } // en secondes
    
    public bool Win { get; set; }
    
    public int QueueId { get; set; } = 420; // Solo/Duo Queue uniquement
    
    // Stats du joueur dans cette partie
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    
    public int ChampionId { get; set; }
    [MaxLength(50)]
    public string ChampionName { get; set; } = null!;
    
    [MaxLength(20)]
    public string Position { get; set; } = null!; // TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY
    
    public int Level { get; set; }
    
    public long TotalDamageDealtToChampions { get; set; }
    public long TotalDamageTaken { get; set; }
    public long GoldEarned { get; set; }
    public int CreepScore { get; set; }
    public int VisionScore { get; set; }
    
    // Infos sur le rang/LP (si disponible)
    public string? Tier { get; set; }
    public string? Rank { get; set; }
    public int? LeaguePoints { get; set; }
    public int? LpChange { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}