using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.DTOs;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;
    
    public LeaderboardController(MongoDbContext context, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<LeaderboardEntryWithChampionsDto>>> GetLeaderboard([FromQuery] string? sortBy = "lp")
    {
        // Récupérer les joueurs actifs
        var players = await _context.Players
            .Find(p => p.IsActive)
            .ToListAsync();

        var leaderboard = new List<LeaderboardEntryWithChampionsDto>();
        
        foreach (var p in players)
        {
            var stats = await _context.PlayerStats
                .Find(ps => ps.PlayerId == p.Id)
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                // Récupérer les champions pour ce joueur et trier en mémoire
                var allChampions = await _context.ChampionStats
                    .Find(cs => cs.PlayerId == p.Id)
                    .ToListAsync();

                var topChampions = allChampions
                    .OrderByDescending(cs => cs.GamesPlayed)
                    .ThenByDescending(cs => cs.WinRate)
                    .Take(3)
                    .ToList();

                var topChampionsDto = topChampions.Select(tc => new TopChampionDto
                {
                    ChampionId = tc.ChampionId,
                    ChampionName = tc.ChampionName,
                    GamesPlayed = tc.GamesPlayed,
                    Wins = tc.Wins,
                    Losses = tc.Losses,
                    WinRate = tc.WinRate,
                    KDA = tc.KDA
                }).ToList();

                leaderboard.Add(new LeaderboardEntryWithChampionsDto
                {
                    PlayerId = p.Id,
                    GameName = p.GameName,
                    TagLine = p.TagLine,
                    CurrentTier = stats.CurrentTier,
                    CurrentRank = stats.CurrentRank,
                    CurrentLeaguePoints = stats.CurrentLeaguePoints,
                    WinRate = stats.WinRate,
                    KDA = stats.KDA,
                    TotalGames = stats.TotalGames,
                    TotalWins = stats.TotalWins,
                    TotalLosses = stats.TotalLosses,
                    CurrentWinStreak = stats.CurrentWinStreak,
                    CurrentLoseStreak = stats.CurrentLoseStreak,
                    LongestWinStreak = stats.LongestWinStreak,
                    LongestLoseStreak = stats.LongestLoseStreak,
                    NetLpChange = stats.NetLpChange,
                    AverageKills = stats.AverageKills,
                    AverageDeaths = stats.AverageDeaths,
                    AverageAssists = stats.AverageAssists,
                    AverageCreepScore = stats.AverageCreepScore,
                    AverageVisionScore = stats.AverageVisionScore,
                    AverageDamageDealt = stats.AverageDamageDealt,
                    LastUpdated = stats.LastUpdated,
                    TopChampions = topChampionsDto
                });
            }
        }
        
        // Trier en mémoire
        leaderboard = sortBy?.ToLower() switch
        {
            "winrate" => leaderboard.OrderByDescending(x => x.WinRate).ThenByDescending(x => x.TotalGames).ToList(),
            "kda" => leaderboard.OrderByDescending(x => x.KDA).ThenByDescending(x => x.TotalGames).ToList(),
            "games" => leaderboard.OrderByDescending(x => x.TotalGames).ThenByDescending(x => x.WinRate).ToList(),
            "winstreak" => leaderboard.OrderByDescending(x => x.CurrentWinStreak).ThenByDescending(x => x.WinRate).ToList(),
            _ => leaderboard.OrderByDescending(x => x.CurrentLeaguePoints).ThenByDescending(x => x.WinRate).ToList()
        };
        
        return Ok(leaderboard);
    }
    
    [HttpGet("compact")]
    public async Task<ActionResult<List<CompactLeaderboardEntryDto>>> GetCompactLeaderboard([FromQuery] int limit = 10)
    {
        if (limit <= 0 || limit > 50)
        {
            return BadRequest("La limite doit être entre 1 et 50");
        }

        // Récupérer les joueurs actifs
        var players = await _context.Players
            .Find(p => p.IsActive)
            .ToListAsync();

        var leaderboard = new List<CompactLeaderboardEntryDto>();
        
        foreach (var p in players)
        {
            var stats = await _context.PlayerStats
                .Find(ps => ps.PlayerId == p.Id)
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                leaderboard.Add(new CompactLeaderboardEntryDto
                {
                    GameName = p.GameName,
                    TagLine = p.TagLine,
                    CurrentTier = stats.CurrentTier,
                    CurrentRank = stats.CurrentRank,
                    CurrentLeaguePoints = stats.CurrentLeaguePoints,
                    WinRate = stats.WinRate,
                    TotalGames = stats.TotalGames
                });
            }
        }
        
        // Trier par LP et prendre le nombre demandé
        leaderboard = leaderboard
            .OrderByDescending(x => x.CurrentLeaguePoints)
            .ThenByDescending(x => x.WinRate)
            .Take(limit)
            .ToList();
        
        return Ok(leaderboard);
    }
    
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetLeaderboardStats()
    {
        var totalPlayers = await _context.Players.CountDocumentsAsync(p => p.IsActive);
        
        // Statistiques de base
        var stats = new
        {
            TotalActivePlayers = totalPlayers,
            LastUpdated = DateTime.UtcNow
        };
        
        return Ok(stats);
    }
}