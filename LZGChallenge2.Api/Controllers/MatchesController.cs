using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.DTOs;
using LZGChallenge2.Api.Services;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ISeasonService _seasonService;
    private readonly ILogger<MatchesController> _logger;
    
    // Constantes pour filtrage strict
    private const int SOLOQ_QUEUE_ID = 420;
    
    // Détection automatique de la saison actuelle basée sur les données disponibles
    private async Task<int> GetCurrentSeasonAsync()
    {
        try
        {
            var latestMatch = await _context.Matches
                .Find(m => m.QueueId == SOLOQ_QUEUE_ID)
                .SortByDescending(m => m.GameStartTime)
                .Limit(1)
                .FirstOrDefaultAsync();
                
            return latestMatch?.Season ?? DateTime.Now.Year;
        }
        catch
        {
            return DateTime.Now.Year; // Fallback sur l'année actuelle
        }
    }

    public MatchesController(MongoDbContext context, ISeasonService seasonService, ILogger<MatchesController> logger)
    {
        _context = context;
        _seasonService = seasonService;
        _logger = logger;
    }

    [HttpGet("season-info")]
    public async Task<ActionResult> GetSeasonInfo()
    {
        try
        {
            var currentSeason = await GetCurrentSeasonAsync();
            
            // Compter les parties SoloQ pour la saison actuelle
            var soloQCount = await _context.Matches
                .Find(m => m.QueueId == SOLOQ_QUEUE_ID && m.Season == currentSeason)
                .CountDocumentsAsync();
                
            return Ok(new
            {
                CurrentSeason = currentSeason,
                SoloQMatchesAvailable = soloQCount,
                QueueId = SOLOQ_QUEUE_ID,
                DisplayText = $"Saison {currentSeason}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season info");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{playerId}")]
    public async Task<ActionResult<List<MatchHistoryDto>>> GetPlayerMatches(
        string playerId,
        [FromQuery] int limit = 50)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                return BadRequest("La limite doit être entre 1 et 100");
            }

            // Détection de la saison actuelle et filtrage strict SoloQ
            var currentSeason = await GetCurrentSeasonAsync();
            var matches = await _context.Matches
                .Find(m => m.PlayerId == playerId && 
                          m.QueueId == SOLOQ_QUEUE_ID && 
                          m.Season == currentSeason)
                .SortByDescending(m => m.GameStartTime)
                .Limit(limit)
                .ToListAsync();

            _logger.LogInformation($"Found {matches.Count} SoloQ matches for season {currentSeason} for player {playerId}");

            var matchDtos = matches.Select(m => new MatchHistoryDto
            {
                MatchId = m.MatchId,
                GameStartTime = m.GameStartTime,
                GameDuration = m.GameDuration,
                Win = m.Win,
                ChampionName = m.ChampionName,
                ChampionId = m.ChampionId,
                Position = m.Position,
                Kills = m.Kills,
                Deaths = m.Deaths,
                Assists = m.Assists,
                CreepScore = m.CreepScore,
                TotalDamageDealtToChampions = m.TotalDamageDealtToChampions,
                GoldEarned = m.GoldEarned,
                VisionScore = m.VisionScore,
                Level = m.Level,
                LpChange = m.LpChange,
                TierAtTime = m.Tier,
                RankAtTime = m.Rank,
                Season = m.Season,
                QueueId = m.QueueId,
                KDA = m.Deaths > 0 ? Math.Round((double)(m.Kills + m.Assists) / m.Deaths, 2) : m.Kills + m.Assists,
                FormattedGameDuration = FormatDuration(m.GameDuration),
                ResultText = m.Win ? "Victoire" : "Défaite",
                KDAText = $"{m.Kills}/{m.Deaths}/{m.Assists}"
            }).ToList();

            return Ok(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving matches for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération de l'historique des parties");
        }
    }

    [HttpGet("{playerId}/recent/{limit}")]
    public async Task<ActionResult<List<MatchHistoryDto>>> GetRecentMatches(string playerId, int limit = 10)
    {
        return await GetPlayerMatches(playerId, Math.Min(limit, 50));
    }

    [HttpGet("{playerId}/filtered")]
    public async Task<ActionResult<List<MatchHistoryDto>>> GetFilteredMatches(
        string playerId,
        [FromQuery] string? period = "all",
        [FromQuery] string? result = "all",
        [FromQuery] string? champion = "all",
        [FromQuery] string? position = "all",
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                return BadRequest("La limite doit être entre 1 et 100");
            }

            // Base: Toujours SoloQ + Saison actuelle détectée
            var currentSeason = await GetCurrentSeasonAsync();
            var filter = Builders<Match>.Filter.And(
                Builders<Match>.Filter.Eq(m => m.PlayerId, playerId),
                Builders<Match>.Filter.Eq(m => m.QueueId, SOLOQ_QUEUE_ID),
                Builders<Match>.Filter.Eq(m => m.Season, currentSeason)
            );

            // Filtre par période
            if (!string.IsNullOrEmpty(period) && period != "all")
            {
                DateTime startDate = period switch
                {
                    "7days" => DateTime.UtcNow.AddDays(-7),
                    "30days" => DateTime.UtcNow.AddDays(-30),
                    "season" => new DateTime(currentSeason, 1, 1), // Début saison actuelle
                    _ => DateTime.MinValue
                };

                if (startDate > DateTime.MinValue)
                {
                    filter = Builders<Match>.Filter.And(filter,
                        Builders<Match>.Filter.Gte(m => m.GameStartTime, startDate));
                }
            }

            // Filtre par résultat
            if (!string.IsNullOrEmpty(result) && result != "all")
            {
                bool winFilter = result == "wins";
                filter = Builders<Match>.Filter.And(filter,
                    Builders<Match>.Filter.Eq(m => m.Win, winFilter));
            }

            // Filtre par champion
            if (!string.IsNullOrEmpty(champion) && champion != "all")
            {
                filter = Builders<Match>.Filter.And(filter,
                    Builders<Match>.Filter.Eq(m => m.ChampionName, champion));
            }

            // Filtre par position
            if (!string.IsNullOrEmpty(position) && position != "all")
            {
                filter = Builders<Match>.Filter.And(filter,
                    Builders<Match>.Filter.Eq(m => m.Position, position.ToUpper()));
            }

            var matches = await _context.Matches
                .Find(filter)
                .SortByDescending(m => m.GameStartTime)
                .Skip(offset)
                .Limit(limit)
                .ToListAsync();

            _logger.LogInformation($"Found {matches.Count} filtered SoloQ matches for season {currentSeason} for player {playerId}");

            var matchDtos = matches.Select(m => new MatchHistoryDto
            {
                MatchId = m.MatchId,
                GameStartTime = m.GameStartTime,
                GameDuration = m.GameDuration,
                Win = m.Win,
                ChampionName = m.ChampionName,
                ChampionId = m.ChampionId,
                Position = m.Position,
                Kills = m.Kills,
                Deaths = m.Deaths,
                Assists = m.Assists,
                CreepScore = m.CreepScore,
                TotalDamageDealtToChampions = m.TotalDamageDealtToChampions,
                GoldEarned = m.GoldEarned,
                VisionScore = m.VisionScore,
                Level = m.Level,
                LpChange = m.LpChange,
                TierAtTime = m.Tier,
                RankAtTime = m.Rank,
                Season = m.Season,
                QueueId = m.QueueId,
                KDA = m.Deaths > 0 ? Math.Round((double)(m.Kills + m.Assists) / m.Deaths, 2) : m.Kills + m.Assists,
                FormattedGameDuration = FormatDuration(m.GameDuration),
                ResultText = m.Win ? "Victoire" : "Défaite",
                KDAText = $"{m.Kills}/{m.Deaths}/{m.Assists}"
            }).ToList();

            return Ok(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filtered matches for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération des parties filtrées");
        }
    }

    [HttpGet("{playerId}/stats")]
    public async Task<ActionResult<MatchStatsDto>> GetPlayerMatchStats(string playerId)
    {
        try
        {
            // Stats uniquement pour SoloQ + Saison actuelle
            var currentSeason = await GetCurrentSeasonAsync();
            var matches = await _context.Matches
                .Find(m => m.PlayerId == playerId && 
                          m.QueueId == SOLOQ_QUEUE_ID && 
                          m.Season == currentSeason)
                .SortBy(m => m.GameStartTime)
                .ToListAsync();

            if (!matches.Any())
            {
                return Ok(new MatchStatsDto
                {
                    TotalGames = 0,
                    Wins = 0,
                    Losses = 0,
                    WinRate = 0,
                    AverageKDA = 0,
                    TotalLpChange = 0
                });
            }

            var firstMatch = matches.First();
            var lastMatch = matches.Last();
            var wins = matches.Count(m => m.Win);
            var losses = matches.Count(m => !m.Win);

            var stats = new MatchStatsDto
            {
                TotalGames = matches.Count,
                Wins = wins,
                Losses = losses,
                WinRate = matches.Count > 0 ? Math.Round((double)wins / matches.Count * 100, 1) : 0,
                AverageKDA = matches.Count > 0 ? Math.Round(matches.Average(m => 
                    m.Deaths > 0 ? (double)(m.Kills + m.Assists) / m.Deaths : m.Kills + m.Assists), 2) : 0,
                TotalLpChange = matches.Where(m => m.LpChange.HasValue).Sum(m => m.LpChange!.Value),
                StartRank = firstMatch.Tier != null ? $"{firstMatch.Tier} {firstMatch.Rank}" : null,
                CurrentRank = lastMatch.Tier != null ? $"{lastMatch.Tier} {lastMatch.Rank}" : null,
                FirstGameDate = firstMatch.GameStartTime,
                LastGameDate = lastMatch.GameStartTime
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving match stats for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération des statistiques");
        }
    }

    private static string FormatDuration(long durationInSeconds)
    {
        var minutes = durationInSeconds / 60;
        var seconds = durationInSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }
}