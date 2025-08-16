using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleStatsController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<RoleStatsController> _logger;

    public RoleStatsController(MongoDbContext context, ILogger<RoleStatsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("{playerId}")]
    public async Task<ActionResult<IEnumerable<RoleStats>>> GetRoleStats(string playerId)
    {
        try
        {
            var roleStats = await _context.RoleStats
                .Find(rs => rs.PlayerId == playerId)
                .SortByDescending(rs => rs.PlayRate)
                .ThenByDescending(rs => rs.GamesPlayed)
                .ToListAsync();

            return Ok(roleStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role stats for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération des statistiques par rôle");
        }
    }

    [HttpGet("{playerId}/main")]
    public async Task<ActionResult<RoleStats>> GetMainRole(string playerId)
    {
        try
        {
            var mainRole = await _context.RoleStats
                .Find(rs => rs.PlayerId == playerId)
                .SortByDescending(rs => rs.PlayRate)
                .ThenByDescending(rs => rs.GamesPlayed)
                .FirstOrDefaultAsync();

            if (mainRole == null)
            {
                return NotFound("Aucun rôle trouvé pour ce joueur");
            }

            return Ok(mainRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving main role for player {PlayerId}", playerId);
            return StatusCode(500, "Erreur lors de la récupération du rôle principal");
        }
    }

    [HttpGet("{playerId}/role/{position}")]
    public async Task<ActionResult<RoleStats>> GetRoleStatsById(string playerId, string position)
    {
        try
        {
            var roleStats = await _context.RoleStats
                .Find(rs => rs.PlayerId == playerId && rs.Position.ToLower() == position.ToLower())
                .FirstOrDefaultAsync();

            if (roleStats == null)
            {
                return NotFound("Statistiques non trouvées pour ce rôle");
            }

            return Ok(roleStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role stats for player {PlayerId} and position {Position}", playerId, position);
            return StatusCode(500, "Erreur lors de la récupération des statistiques du rôle");
        }
    }

    [HttpGet("player/{gameName}/{tagLine}")]
    public async Task<ActionResult<IEnumerable<RoleStats>>> GetRoleStatsByPlayerName(string gameName, string tagLine)
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

            return await GetRoleStats(player.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role stats for player {GameName}#{TagLine}", gameName, tagLine);
            return StatusCode(500, "Erreur lors de la récupération des statistiques par rôle");
        }
    }

    [HttpGet("leaderboard/{position}")]
    public async Task<ActionResult<IEnumerable<object>>> GetRoleLeaderboard(string position, int limit = 10)
    {
        try
        {
            if (limit <= 0 || limit > 50)
            {
                return BadRequest("La limite doit être entre 1 et 50");
            }

            var roleStats = await _context.RoleStats
                .Find(rs => rs.Position.ToLower() == position.ToLower() && rs.GamesPlayed >= 3)
                .SortByDescending(rs => rs.WinRate)
                .ThenByDescending(rs => rs.GamesPlayed)
                .Limit(limit)
                .ToListAsync();

            var leaderboard = new List<object>();
            foreach (var rs in roleStats)
            {
                var player = await _context.Players.Find(p => p.Id == rs.PlayerId).FirstOrDefaultAsync();
                if (player != null)
                {
                    leaderboard.Add(new
                    {
                        GameName = player.GameName,
                        TagLine = player.TagLine,
                        rs.Position,
                        rs.GamesPlayed,
                        rs.WinRate,
                        rs.KDA,
                        rs.AverageCreepScore,
                        rs.AverageVisionScore
                    });
                }
            }

            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role leaderboard for position {Position}", position);
            return StatusCode(500, "Erreur lors de la récupération du classement par rôle");
        }
    }
}