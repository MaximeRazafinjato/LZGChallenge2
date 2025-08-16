using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.Services;
using LZGChallenge2.Api.DTOs;

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
            var allChampionStats = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId)
                .ToListAsync();

            var championStats = allChampionStats
                .OrderByDescending(cs => cs.GamesPlayed)
                .ThenByDescending(cs => cs.WinRate)
                .ToList();

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

            var allChampions = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId)
                .ToListAsync();

            var topChampions = allChampions
                .OrderByDescending(cs => cs.GamesPlayed)
                .ThenByDescending(cs => cs.WinRate)
                .Take(limit)
                .ToList();

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

    [HttpGet("{playerId}/filtered")]
    public async Task<ActionResult<IEnumerable<ChampionStatsFilteredDto>>> GetFilteredChampionStats(
        string playerId,
        [FromQuery] string? sortBy = "games",
        [FromQuery] string? role = "all",
        [FromQuery] string? search = "",
        [FromQuery] int limit = 50)
    {
        try
        {
            if (limit <= 0 || limit > 100)
            {
                return BadRequest("La limite doit être entre 1 et 100");
            }

            // Récupérer tous les champions du joueur
            var championStats = await _context.ChampionStats
                .Find(cs => cs.PlayerId == playerId)
                .ToListAsync();

            // Récupérer les statistiques par rôle pour déterminer le rôle principal de chaque champion
            var roleStats = await _context.RoleStats
                .Find(rs => rs.PlayerId == playerId)
                .ToListAsync();

            // Créer un dictionnaire pour mapper les champions à leur rôle principal
            var championRoleMap = new Dictionary<int, string>();
            
            // Pour chaque champion, essayer de déterminer son rôle principal
            foreach (var champ in championStats)
            {
                // Logique simplifiée : on peut améliorer cela en croisant avec les données de match
                var mostPlayedRole = roleStats
                    .Where(rs => rs.GamesPlayed > 0)
                    .OrderByDescending(rs => rs.GamesPlayed)
                    .FirstOrDefault();
                
                championRoleMap[champ.ChampionId] = mostPlayedRole?.Position ?? "UNKNOWN";
            }

            // Appliquer les filtres
            var filteredStats = championStats.AsQueryable();

            // Filtre par nom de champion
            if (!string.IsNullOrEmpty(search))
            {
                filteredStats = filteredStats.Where(cs => 
                    cs.ChampionName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Convertir en DTO avec le rôle
            var championDtos = filteredStats.Select(cs => new ChampionStatsFilteredDto
            {
                ChampionId = cs.ChampionId,
                ChampionName = cs.ChampionName,
                GamesPlayed = cs.GamesPlayed,
                Wins = cs.Wins,
                Losses = cs.Losses,
                WinRate = cs.WinRate,
                KDA = cs.KDA,
                AverageKills = cs.AverageKills,
                AverageDeaths = cs.AverageDeaths,
                AverageAssists = cs.AverageAssists,
                AverageCreepScore = cs.AverageCreepScore,
                AverageVisionScore = cs.AverageVisionScore,
                AverageDamageDealt = cs.AverageDamageDealt,
                PrimaryRole = championRoleMap.GetValueOrDefault(cs.ChampionId, "UNKNOWN")
            }).AsQueryable();

            // Filtre par rôle
            if (!string.IsNullOrEmpty(role) && role.ToLower() != "all")
            {
                var normalizedRole = role.ToUpper() switch
                {
                    "TOP" => "TOP",
                    "JUNGLE" => "JUNGLE",
                    "MID" or "MIDDLE" => "MIDDLE",
                    "ADC" or "BOTTOM" => "BOTTOM",
                    "SUPPORT" or "UTILITY" => "UTILITY",
                    _ => role.ToUpper()
                };

                championDtos = championDtos.Where(cd => cd.PrimaryRole == normalizedRole);
            }

            // Tri
            championDtos = sortBy?.ToLower() switch
            {
                "winrate" => championDtos.OrderByDescending(cd => cd.WinRate).ThenByDescending(cd => cd.GamesPlayed),
                "kda" => championDtos.OrderByDescending(cd => cd.KDA).ThenByDescending(cd => cd.GamesPlayed),
                "alphabetical" => championDtos.OrderBy(cd => cd.ChampionName),
                _ => championDtos.OrderByDescending(cd => cd.GamesPlayed).ThenByDescending(cd => cd.WinRate)
            };

            // Appliquer la limite
            var result = championDtos.Take(limit).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving filtered champion stats for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération des statistiques filtrées");
        }
    }
}