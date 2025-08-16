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
    
    private static int CalculateRankScore(string? tier, string? rank, int leaguePoints)
    {
        // Valeurs de base pour chaque tier
        var tierValues = new Dictionary<string, int>
        {
            ["IRON"] = 0,
            ["BRONZE"] = 400,
            ["SILVER"] = 800,
            ["GOLD"] = 1200,
            ["PLATINUM"] = 1600,
            ["EMERALD"] = 2000,
            ["DIAMOND"] = 2400,
            ["MASTER"] = 2800,
            ["GRANDMASTER"] = 3200,
            ["CHALLENGER"] = 3600
        };

        // Si pas de tier, retourner 0
        if (string.IsNullOrEmpty(tier) || !tierValues.ContainsKey(tier.ToUpper()))
        {
            return 0;
        }

        var baseScore = tierValues[tier.ToUpper()];

        // Ajouter les points pour le rank (IV = 0, III = 100, II = 200, I = 300)
        var rankValues = new Dictionary<string, int>
        {
            ["IV"] = 0,
            ["III"] = 100,
            ["II"] = 200,
            ["I"] = 300
        };
        
        var rankValue = !string.IsNullOrEmpty(rank) && rankValues.ContainsKey(rank.ToUpper()) 
            ? rankValues[rank.ToUpper()] 
            : 0;

        // Ajouter les LP (max 100 par division)
        var lpValue = Math.Min(leaguePoints, 100);

        return baseScore + rankValue + lpValue;
    }
    
    private async Task<RealPlayerStats?> CalculateRealPlayerStats(string playerId)
    {
        try
        {
            // Utiliser la même logique que MatchesController pour détecter la saison
            var currentSeason = await GetCurrentSeasonAsync();
            
            var matches = await _context.Matches
                .Find(m => m.PlayerId == playerId && 
                          m.QueueId == 420 && // SoloQ uniquement
                          m.Season == currentSeason)
                .ToListAsync();

            if (!matches.Any())
                return null;

            var totalKills = matches.Sum(m => m.Kills);
            var totalDeaths = matches.Sum(m => m.Deaths);
            var totalAssists = matches.Sum(m => m.Assists);
            var totalCreepScore = matches.Sum(m => m.CreepScore);
            var totalVisionScore = matches.Sum(m => m.VisionScore);
            var totalDamage = matches.Sum(m => m.TotalDamageDealtToChampions);
            var totalLpChange = matches.Where(m => m.LpChange.HasValue).Sum(m => m.LpChange!.Value);

            var gameCount = matches.Count;

            return new RealPlayerStats
            {
                AverageKDA = totalDeaths > 0 ? Math.Round((double)(totalKills + totalAssists) / totalDeaths, 2) : totalKills + totalAssists,
                AverageKills = gameCount > 0 ? Math.Round((double)totalKills / gameCount, 1) : 0,
                AverageDeaths = gameCount > 0 ? Math.Round((double)totalDeaths / gameCount, 1) : 0,
                AverageAssists = gameCount > 0 ? Math.Round((double)totalAssists / gameCount, 1) : 0,
                AverageCreepScore = gameCount > 0 ? Math.Round((double)totalCreepScore / gameCount, 1) : 0,
                AverageVisionScore = gameCount > 0 ? Math.Round((double)totalVisionScore / gameCount, 1) : 0,
                AverageDamageDealt = gameCount > 0 ? Math.Round((double)totalDamage / gameCount, 0) : 0,
                TotalLpChange = totalLpChange
            };
        }
        catch
        {
            return null;
        }
    }
    
    private async Task<int> GetCurrentSeasonAsync()
    {
        try
        {
            var latestMatch = await _context.Matches
                .Find(m => m.QueueId == 420)
                .SortByDescending(m => m.GameStartTime)
                .Limit(1)
                .FirstOrDefaultAsync();
                
            return latestMatch?.Season ?? DateTime.Now.Year;
        }
        catch
        {
            return DateTime.Now.Year;
        }
    }
    
    public class RealPlayerStats
    {
        public double AverageKDA { get; set; }
        public double AverageKills { get; set; }
        public double AverageDeaths { get; set; }
        public double AverageAssists { get; set; }
        public double AverageCreepScore { get; set; }
        public double AverageVisionScore { get; set; }
        public double AverageDamageDealt { get; set; }
        public int TotalLpChange { get; set; }
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
                // Calculer les vraies statistiques à partir des parties
                var realStats = await CalculateRealPlayerStats(p.Id);
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
                    KDA = realStats?.AverageKDA ?? 0,
                    TotalGames = stats.TotalGames,
                    TotalWins = stats.TotalWins,
                    TotalLosses = stats.TotalLosses,
                    CurrentWinStreak = stats.CurrentWinStreak,
                    CurrentLoseStreak = stats.CurrentLoseStreak,
                    LongestWinStreak = stats.LongestWinStreak,
                    LongestLoseStreak = stats.LongestLoseStreak,
                    NetLpChange = realStats?.TotalLpChange ?? stats.NetLpChange,
                    AverageKills = realStats?.AverageKills ?? 0,
                    AverageDeaths = realStats?.AverageDeaths ?? 0,
                    AverageAssists = realStats?.AverageAssists ?? 0,
                    AverageCreepScore = realStats?.AverageCreepScore ?? 0,
                    AverageVisionScore = realStats?.AverageVisionScore ?? 0,
                    AverageDamageDealt = realStats?.AverageDamageDealt ?? 0,
                    LastUpdated = stats.LastUpdated,
                    TopChampions = topChampionsDto
                });
            }
        }
        
        // Trier en mémoire avec système de score de rang correct
        leaderboard = sortBy?.ToLower() switch
        {
            "winrate" => leaderboard.OrderByDescending(x => x.WinRate).ThenByDescending(x => x.TotalGames).ToList(),
            "kda" => leaderboard.OrderByDescending(x => x.KDA).ThenByDescending(x => x.TotalGames).ToList(),
            "games" => leaderboard.OrderByDescending(x => x.TotalGames).ThenByDescending(x => x.WinRate).ToList(),
            "winstreak" => leaderboard.OrderByDescending(x => x.CurrentWinStreak).ThenByDescending(x => x.WinRate).ToList(),
            _ => leaderboard.OrderByDescending(x => CalculateRankScore(x.CurrentTier, x.CurrentRank, x.CurrentLeaguePoints))
                          .ThenByDescending(x => x.WinRate)
                          .ToList()
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
        
        // Trier par score de rang correct et prendre le nombre demandé
        leaderboard = leaderboard
            .OrderByDescending(x => CalculateRankScore(x.CurrentTier, x.CurrentRank, x.CurrentLeaguePoints))
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