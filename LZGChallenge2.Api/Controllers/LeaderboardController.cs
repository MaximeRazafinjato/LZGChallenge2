using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.DTOs;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;
    
    public LeaderboardController(AppDbContext context, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntryDto>>> GetLeaderboard([FromQuery] string? sortBy = "lp")
    {
        // Récupérer les joueurs actifs avec leurs stats
        var players = await _context.Players
            .Include(p => p.CurrentStats)
            .Where(p => p.IsActive && p.CurrentStats != null)
            .ToListAsync();
        
        // Créer les DTOs en mémoire pour éviter les problèmes LINQ complexes
        var leaderboard = players.Select(p => new LeaderboardEntryDto
        {
            PlayerId = p.Id,
            GameName = p.GameName,
            TagLine = p.TagLine,
            CurrentTier = p.CurrentStats!.CurrentTier,
            CurrentRank = p.CurrentStats.CurrentRank,
            CurrentLeaguePoints = p.CurrentStats.CurrentLeaguePoints,
            WinRate = p.CurrentStats.WinRate,
            KDA = p.CurrentStats.KDA,
            TotalGames = p.CurrentStats.TotalGames,
            NetLpChange = p.CurrentStats.NetLpChange,
            CurrentWinStreak = p.CurrentStats.CurrentWinStreak,
            CurrentLoseStreak = p.CurrentStats.CurrentLoseStreak,
            LastUpdated = p.CurrentStats.LastUpdated
        }).ToList();
        
        // Tri en mémoire basé sur le paramètre
        leaderboard = sortBy?.ToLower() switch
        {
            "winrate" => leaderboard.OrderByDescending(x => x.WinRate).ThenByDescending(x => x.TotalGames).ToList(),
            "kda" => leaderboard.OrderByDescending(x => x.KDA).ThenByDescending(x => x.TotalGames).ToList(),
            "games" => leaderboard.OrderByDescending(x => x.TotalGames).ThenByDescending(x => x.WinRate).ToList(),
            "progress" => leaderboard.OrderByDescending(x => x.NetLpChange).ThenByDescending(x => CalculateRankScore(x.CurrentTier, x.CurrentRank, x.CurrentLeaguePoints)).ToList(),
            "streak" => leaderboard.OrderByDescending(x => x.CurrentWinStreak).ThenByDescending(x => CalculateRankScore(x.CurrentTier, x.CurrentRank, x.CurrentLeaguePoints)).ToList(),
            _ => leaderboard.OrderByDescending(x => CalculateRankScore(x.CurrentTier, x.CurrentRank, x.CurrentLeaguePoints)).ToList() // Default: tri par rang complet
        };
        
        return Ok(leaderboard);
    }
    
    private static int CalculateRankScore(string? tier, string? rank, int leaguePoints)
    {
        // Valeurs de base pour chaque tier
        var tierValues = new Dictionary<string, int>
        {
            { "IRON", 0 },
            { "BRONZE", 400 },
            { "SILVER", 800 },
            { "GOLD", 1200 },
            { "PLATINUM", 1600 },
            { "EMERALD", 2000 },
            { "DIAMOND", 2400 },
            { "MASTER", 2800 },
            { "GRANDMASTER", 3200 },
            { "CHALLENGER", 3600 }
        };
        
        // Si pas de tier, retourner 0
        if (string.IsNullOrEmpty(tier) || !tierValues.ContainsKey(tier.ToUpper()))
        {
            return 0;
        }
        
        var baseScore = tierValues[tier.ToUpper()];
        
        // Ajouter les points pour le rank (IV = 0, III = 100, II = 200, I = 300)
        var rankValue = rank?.ToUpper() switch
        {
            "IV" => 0,
            "III" => 100,
            "II" => 200,
            "I" => 300,
            _ => 0 // Master+ n'ont pas de rank
        };
        
        // Ajouter les LP (max 100 par division)
        var lpValue = Math.Min(leaguePoints, 100);
        
        return baseScore + rankValue + lpValue;
    }
    
    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetLeaderboardSummary()
    {
        var totalPlayers = await _context.Players.CountAsync(p => p.IsActive);
        
        // Récupérer les données en mémoire pour éviter les problèmes LINQ complexes
        var playersWithStats = await _context.Players
            .Include(p => p.CurrentStats)
            .Where(p => p.IsActive && p.CurrentStats != null && p.CurrentStats.TotalGames > 0)
            .ToListAsync();
        
        // Calculer le total de parties de tous les joueurs actifs
        var totalGames = playersWithStats.Sum(p => p.CurrentStats!.TotalGames);
        
        var avgWinRate = playersWithStats.Any() ? playersWithStats.Average(p => p.CurrentStats!.WinRate) : 0;
        
        // Trouver le top player en utilisant le score de rang complet (comme dans le leaderboard)
        var topPlayer = playersWithStats
            .OrderByDescending(p => CalculateRankScore(p.CurrentStats!.CurrentTier, p.CurrentStats.CurrentRank, p.CurrentStats.CurrentLeaguePoints))
            .Select(p => new { 
                p.GameName, 
                p.TagLine, 
                p.CurrentStats!.CurrentTier,
                p.CurrentStats.CurrentRank,
                p.CurrentStats.CurrentLeaguePoints 
            })
            .FirstOrDefault();
        
        var summary = new
        {
            TotalPlayers = totalPlayers,
            TotalGames = totalGames,
            AverageWinRate = Math.Round(avgWinRate, 1),
            TopPlayer = topPlayer
        };
        
        return Ok(summary);
    }
}