using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.Services;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChampionStatsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ISeasonService _seasonService;
    private readonly ILogger<ChampionStatsController> _logger;

    public ChampionStatsController(MongoDbContext context, ISeasonService seasonService, ILogger<ChampionStatsController> logger)
    {
        _context = context;
        _seasonService = seasonService;
        _logger = logger;
    }

    [HttpGet("{playerId}")]
    public async Task<ActionResult<IEnumerable<ChampionStats>>> GetChampionStats(string playerId)
    {
        try
        {
            var championStats = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId)
                .SortByDescending(cs => cs.GamesPlayed)
                .ThenByDescending(cs => cs.WinRate)
                .ToListAsync();

            return Ok(championStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving champion stats for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération des statistiques par champion");
        }
    }

    [HttpGet("{playerId}/top/{limit}")]
    public async Task<ActionResult<IEnumerable<ChampionStats>>> GetTopChampions(string playerId, int limit = 5)
    {
        try
        {
            if (limit <= 0 || limit > 50)
            {
                return BadRequest("La limite doit être entre 1 et 50");
            }

            var topChampions = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId)
                .SortByDescending(cs => cs.GamesPlayed)
                .ThenByDescending(cs => cs.WinRate)
                .Limit(limit)
                .ToListAsync();

            return Ok(topChampions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top champions for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération du top champions");
        }
    }

    [HttpGet("{playerId}/champion/{championId}")]
    public async Task<ActionResult<ChampionStats>> GetChampionStatsById(string playerId, int championId)
    {
        try
        {
            var championStats = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId && cs.ChampionId == championId)
                .FirstOrDefaultAsync();

            if (championStats == null)
            {
                return NotFound("Statistiques non trouvées pour ce champion");
            }

            return Ok(championStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving champion stats for player {PlayerId} and champion {ChampionId}", playerId, championId);
            return StatusCode(500, "Erreur lors de la récupération des statistiques du champion");
        }
    }

    [HttpGet("player/{gameName}/{tagLine}")]
    public async Task<ActionResult<IEnumerable<ChampionStats>>> GetChampionStatsByPlayerName(string gameName, string tagLine)
    {
        try
        {
            var player = await _context.Players
                .Find(p => p.GameName.ToLower() == gameName.ToLower() && 
                          p.TagLine.ToLower() == tagLine.ToLower())
                .FirstOrDefaultAsync();

            if (player == null)
            {
                return NotFound("Joueur introuvable");
            }

            return await GetChampionStats(player.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving champion stats for player {GameName}#{TagLine}", gameName, tagLine);
            return StatusCode(500, "Erreur lors de la récupération des statistiques par champion");
        }
    }
}