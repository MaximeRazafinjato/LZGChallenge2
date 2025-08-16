using System.ComponentModel.DataAnnotations;

namespace LZGChallenge2.Api.Models;

public class Player
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RiotId { get; set; } = null!;
    
    [Required]
    [MaxLength(50)]
    public string GameName { get; set; } = null!;
    
    [Required]
    [MaxLength(10)]
    public string TagLine { get; set; } = null!;
    
    [Required]
    [MaxLength(78)]
    public string Puuid { get; set; } = null!;
    
    [Required]
    [MaxLength(78)]
    public string SummonerId { get; set; } = null!;
    
    [Required]
    [MaxLength(10)]
    public string Region { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public PlayerStats? CurrentStats { get; set; }
    public ICollection<ChampionStats> ChampionStats { get; set; } = new List<ChampionStats>();
    public ICollection<RoleStats> RoleStats { get; set; } = new List<RoleStats>();
}