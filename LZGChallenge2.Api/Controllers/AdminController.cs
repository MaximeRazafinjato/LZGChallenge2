using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Services;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISeasonService _seasonService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(AppDbContext context, ISeasonService seasonService, ILogger<AdminController> logger)
    {
        _context = context;
        _seasonService = seasonService;
        _logger = logger;
    }

    [HttpPost("update-match-seasons")]
    public async Task<IActionResult> UpdateMatchSeasons()
    {
        try
        {
            _logger.LogInformation("Starting update of match seasons...");

            var matchesWithWrongSeason = await _context.Matches
                .Where(m => m.Season == 0)
                .ToListAsync();

            _logger.LogInformation("Found {Count} matches with Season = 0", matchesWithWrongSeason.Count);

            var updatedCount = 0;
            foreach (var match in matchesWithWrongSeason)
            {
                var correctSeason = _seasonService.GetSeasonFromDate(match.GameStartTime);
                if (match.Season != correctSeason)
                {
                    match.Season = correctSeason;
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated {UpdatedCount} matches with correct seasons", updatedCount);
            }
            else
            {
                _logger.LogInformation("No matches needed season updates");
            }

            return Ok(new { 
                message = $"Successfully updated {updatedCount} matches", 
                totalChecked = matchesWithWrongSeason.Count,
                updated = updatedCount 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating match seasons");
            return StatusCode(500, "Erreur lors de la mise à jour des saisons");
        }
    }

    [HttpGet("match-seasons-stats")]
    public async Task<IActionResult> GetMatchSeasonsStats()
    {
        try
        {
            var seasonStats = await _context.Matches
                .GroupBy(m => m.Season)
                .Select(g => new { Season = g.Key, Count = g.Count() })
                .OrderBy(x => x.Season)
                .ToListAsync();

            return Ok(seasonStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting match seasons stats");
            return StatusCode(500, "Erreur lors de la récupération des statistiques");
        }
    }
}