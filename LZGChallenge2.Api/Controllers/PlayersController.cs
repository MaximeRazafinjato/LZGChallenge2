using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.DTOs;
using LZGChallenge2.Api.Services;
using LZGChallenge2.Api.Hubs;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRiotApiService _riotApiService;
    private readonly IMatchUpdateService _matchUpdateService;
    private readonly IHubContext<LeaderboardHub> _hubContext;
    private readonly ILogger<PlayersController> _logger;
    
    public PlayersController(
        AppDbContext context,
        IRiotApiService riotApiService,
        IMatchUpdateService matchUpdateService,
        IHubContext<LeaderboardHub> hubContext,
        ILogger<PlayersController> logger)
    {
        _context = context;
        _riotApiService = riotApiService;
        _matchUpdateService = matchUpdateService;
        _hubContext = hubContext;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetPlayers()
    {
        var players = await _context.Players
            .Include(p => p.CurrentStats)
            .Where(p => p.IsActive)
            .Select(p => new PlayerDto
            {
                Id = p.Id,
                RiotId = p.RiotId,
                GameName = p.GameName,
                TagLine = p.TagLine,
                Region = p.Region,
                JoinedAt = p.JoinedAt,
                IsActive = p.IsActive,
                CurrentStats = p.CurrentStats == null ? null : new PlayerStatsDto
                {
                    CurrentTier = p.CurrentStats.CurrentTier,
                    CurrentRank = p.CurrentStats.CurrentRank,
                    CurrentLeaguePoints = p.CurrentStats.CurrentLeaguePoints,
                    TotalGames = p.CurrentStats.TotalGames,
                    TotalWins = p.CurrentStats.TotalWins,
                    TotalLosses = p.CurrentStats.TotalLosses,
                    WinRate = p.CurrentStats.WinRate,
                    AverageKills = p.CurrentStats.AverageKills,
                    AverageDeaths = p.CurrentStats.AverageDeaths,
                    AverageAssists = p.CurrentStats.AverageAssists,
                    KDA = p.CurrentStats.KDA,
                    AverageCreepScore = p.CurrentStats.AverageCreepScore,
                    AverageVisionScore = p.CurrentStats.AverageVisionScore,
                    AverageDamageDealt = p.CurrentStats.AverageDamageDealt,
                    CurrentWinStreak = p.CurrentStats.CurrentWinStreak,
                    CurrentLoseStreak = p.CurrentStats.CurrentLoseStreak,
                    LongestWinStreak = p.CurrentStats.LongestWinStreak,
                    LongestLoseStreak = p.CurrentStats.LongestLoseStreak,
                    NetLpChange = p.CurrentStats.NetLpChange,
                    LastUpdated = p.CurrentStats.LastUpdated
                }
            })
            .ToListAsync();
        
        return Ok(players);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(int id)
    {
        var player = await _context.Players
            .Include(p => p.CurrentStats)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (player == null)
        {
            return NotFound();
        }
        
        var playerDto = new PlayerDto
        {
            Id = player.Id,
            RiotId = player.RiotId,
            GameName = player.GameName,
            TagLine = player.TagLine,
            Region = player.Region,
            JoinedAt = player.JoinedAt,
            IsActive = player.IsActive,
            CurrentStats = player.CurrentStats == null ? null : new PlayerStatsDto
            {
                CurrentTier = player.CurrentStats.CurrentTier,
                CurrentRank = player.CurrentStats.CurrentRank,
                CurrentLeaguePoints = player.CurrentStats.CurrentLeaguePoints,
                TotalGames = player.CurrentStats.TotalGames,
                TotalWins = player.CurrentStats.TotalWins,
                TotalLosses = player.CurrentStats.TotalLosses,
                WinRate = player.CurrentStats.WinRate,
                AverageKills = player.CurrentStats.AverageKills,
                AverageDeaths = player.CurrentStats.AverageDeaths,
                AverageAssists = player.CurrentStats.AverageAssists,
                KDA = player.CurrentStats.KDA,
                AverageCreepScore = player.CurrentStats.AverageCreepScore,
                AverageVisionScore = player.CurrentStats.AverageVisionScore,
                AverageDamageDealt = player.CurrentStats.AverageDamageDealt,
                CurrentWinStreak = player.CurrentStats.CurrentWinStreak,
                CurrentLoseStreak = player.CurrentStats.CurrentLoseStreak,
                LongestWinStreak = player.CurrentStats.LongestWinStreak,
                LongestLoseStreak = player.CurrentStats.LongestLoseStreak,
                NetLpChange = player.CurrentStats.NetLpChange,
                LastUpdated = player.CurrentStats.LastUpdated
            }
        };
        
        return Ok(playerDto);
    }
    
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer(CreatePlayerDto createPlayerDto)
    {
        // Vérifier si le joueur existe déjà
        var existingPlayer = await _context.Players
            .FirstOrDefaultAsync(p => p.GameName == createPlayerDto.GameName && p.TagLine == createPlayerDto.TagLine);
        
        if (existingPlayer != null)
        {
            if (!existingPlayer.IsActive)
            {
                existingPlayer.IsActive = true;
                await _context.SaveChangesAsync();
                return Ok(new PlayerDto
                {
                    Id = existingPlayer.Id,
                    RiotId = existingPlayer.RiotId,
                    GameName = existingPlayer.GameName,
                    TagLine = existingPlayer.TagLine,
                    Region = existingPlayer.Region,
                    JoinedAt = existingPlayer.JoinedAt,
                    IsActive = existingPlayer.IsActive
                });
            }
            
            return BadRequest("Ce joueur est déjà inscrit au challenge.");
        }
        
        // Récupérer les informations du joueur via l'API Riot
        _logger.LogInformation("Attempting to get account for {GameName}#{TagLine}", createPlayerDto.GameName, createPlayerDto.TagLine);
        
        var account = await _riotApiService.GetAccountByRiotIdAsync(createPlayerDto.GameName, createPlayerDto.TagLine);
        if (account == null)
        {
            _logger.LogWarning("Account not found for {GameName}#{TagLine}", createPlayerDto.GameName, createPlayerDto.TagLine);
            return BadRequest("Joueur introuvable. Vérifiez le nom d'invocateur et le tag.");
        }
        
        _logger.LogInformation("Account found for {GameName}#{TagLine}, PUUID: {Puuid}", account.GameName, account.TagLine, account.Puuid);
        
        var summoner = await _riotApiService.GetSummonerByPuuidAsync(account.Puuid);
        if (summoner == null)
        {
            _logger.LogError("Failed to get summoner info for PUUID: {Puuid}. Summoner: {@Summoner}", account.Puuid, summoner);
            return BadRequest("Impossible de récupérer les informations du summoner.");
        }
        
        _logger.LogInformation("Summoner found with level: {Level}", summoner.SummonerLevel);
        
        // Créer le joueur - utiliser le PUUID comme SummonerId car l'API ne retourne plus d'ID séparé
        var player = new Player
        {
            RiotId = $"{account.GameName}#{account.TagLine}",
            GameName = account.GameName,
            TagLine = account.TagLine,
            Puuid = account.Puuid,
            SummonerId = account.Puuid, // Utiliser le PUUID comme SummonerId
            Region = createPlayerDto.Region,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        
        // Récupérer les informations de rang
        var leagueEntries = await _riotApiService.GetLeagueEntriesByPuuidAsync(account.Puuid);
        var soloQEntry = leagueEntries.FirstOrDefault(e => e.QueueType == "RANKED_SOLO_5x5");
        
        // Initialiser les stats avec les informations de rang
        var playerStats = new PlayerStats
        {
            PlayerId = player.Id,
            CurrentTier = soloQEntry?.Tier,
            CurrentRank = soloQEntry?.Rank,
            CurrentLeaguePoints = soloQEntry?.LeaguePoints ?? 0,
            TotalGames = (soloQEntry?.Wins ?? 0) + (soloQEntry?.Losses ?? 0),
            TotalWins = soloQEntry?.Wins ?? 0,
            TotalLosses = soloQEntry?.Losses ?? 0,
            TotalKills = 0,
            TotalDeaths = 0,
            TotalAssists = 0,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.PlayerStats.Add(playerStats);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Player stats initialized with rank: {Tier} {Rank} ({LP} LP)", 
            playerStats.CurrentTier, playerStats.CurrentRank, playerStats.CurrentLeaguePoints);
        
        // Récupérer les matches récents et mettre à jour les statistiques
        _logger.LogInformation("Fetching recent matches for player {GameName}#{TagLine}...", player.GameName, player.TagLine);
        
        try
        {
            var matchesUpdated = await _matchUpdateService.UpdatePlayerMatchesAsync(player, 20);
            if (matchesUpdated)
            {
                _logger.LogInformation("Successfully updated matches and stats for player {GameName}#{TagLine}", player.GameName, player.TagLine);
                
                // Recharger le joueur avec les stats mises à jour pour la notification
                await _context.Entry(player).ReloadAsync();
                await _context.Entry(player).Reference(p => p.CurrentStats).LoadAsync();
            }
            else
            {
                _logger.LogWarning("No matches could be updated for player {GameName}#{TagLine}", player.GameName, player.TagLine);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matches for newly added player {GameName}#{TagLine}", player.GameName, player.TagLine);
            // Continue même si la mise à jour des matches échoue
        }
        
        // Notifier les clients connectés
        await _hubContext.Clients.Group("Leaderboard").SendAsync("PlayerAdded", new PlayerDto
        {
            Id = player.Id,
            RiotId = player.RiotId,
            GameName = player.GameName,
            TagLine = player.TagLine,
            Region = player.Region,
            JoinedAt = player.JoinedAt,
            IsActive = player.IsActive
        });
        
        _logger.LogInformation("Player {GameName}#{TagLine} added to challenge", player.GameName, player.TagLine);
        
        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, new PlayerDto
        {
            Id = player.Id,
            RiotId = player.RiotId,
            GameName = player.GameName,
            TagLine = player.TagLine,
            Region = player.Region,
            JoinedAt = player.JoinedAt,
            IsActive = player.IsActive
        });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> RemovePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }
        
        player.IsActive = false;
        await _context.SaveChangesAsync();
        
        // Notifier les clients connectés
        await _hubContext.Clients.Group("Leaderboard").SendAsync("PlayerRemoved", id);
        
        _logger.LogInformation("Player {GameName}#{TagLine} removed from challenge", player.GameName, player.TagLine);
        
        return NoContent();
    }
    
    [HttpPost("{id}/refresh-rank")]
    public async Task<IActionResult> RefreshPlayerRank(int id)
    {
        var player = await _context.Players
            .Include(p => p.CurrentStats)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            
        if (player == null)
        {
            return NotFound();
        }
        
        try
        {
            // Récupérer les informations de rang actuelles
            var leagueEntries = await _riotApiService.GetLeagueEntriesByPuuidAsync(player.Puuid);
            var soloQEntry = leagueEntries.FirstOrDefault(e => e.QueueType == "RANKED_SOLO_5x5");
            
            if (player.CurrentStats != null)
            {
                // Mettre à jour les informations de rang
                player.CurrentStats.CurrentTier = soloQEntry?.Tier;
                player.CurrentStats.CurrentRank = soloQEntry?.Rank;
                player.CurrentStats.CurrentLeaguePoints = soloQEntry?.LeaguePoints ?? 0;
                player.CurrentStats.TotalGames = (soloQEntry?.Wins ?? 0) + (soloQEntry?.Losses ?? 0);
                player.CurrentStats.TotalWins = soloQEntry?.Wins ?? 0;
                player.CurrentStats.TotalLosses = soloQEntry?.Losses ?? 0;
                player.CurrentStats.LastUpdated = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Refreshed rank for player {GameName}#{TagLine}: {Tier} {Rank} ({LP} LP)", 
                    player.GameName, player.TagLine, player.CurrentStats.CurrentTier, 
                    player.CurrentStats.CurrentRank, player.CurrentStats.CurrentLeaguePoints);
                
                // Notifier les clients connectés
                await _hubContext.Clients.Group("Leaderboard").SendAsync("StatsUpdated", new PlayerDto
                {
                    Id = player.Id,
                    RiotId = player.RiotId,
                    GameName = player.GameName,
                    TagLine = player.TagLine,
                    Region = player.Region,
                    JoinedAt = player.JoinedAt,
                    IsActive = player.IsActive,
                    CurrentStats = new PlayerStatsDto
                    {
                        CurrentTier = player.CurrentStats.CurrentTier,
                        CurrentRank = player.CurrentStats.CurrentRank,
                        CurrentLeaguePoints = player.CurrentStats.CurrentLeaguePoints,
                        TotalGames = player.CurrentStats.TotalGames,
                        TotalWins = player.CurrentStats.TotalWins,
                        TotalLosses = player.CurrentStats.TotalLosses,
                        WinRate = player.CurrentStats.WinRate,
                        AverageKills = player.CurrentStats.AverageKills,
                        AverageDeaths = player.CurrentStats.AverageDeaths,
                        AverageAssists = player.CurrentStats.AverageAssists,
                        KDA = player.CurrentStats.KDA,
                        AverageCreepScore = player.CurrentStats.AverageCreepScore,
                        AverageVisionScore = player.CurrentStats.AverageVisionScore,
                        AverageDamageDealt = player.CurrentStats.AverageDamageDealt,
                        CurrentWinStreak = player.CurrentStats.CurrentWinStreak,
                        CurrentLoseStreak = player.CurrentStats.CurrentLoseStreak,
                        LongestWinStreak = player.CurrentStats.LongestWinStreak,
                        LongestLoseStreak = player.CurrentStats.LongestLoseStreak,
                        NetLpChange = player.CurrentStats.NetLpChange,
                        LastUpdated = player.CurrentStats.LastUpdated
                    }
                });
                
                return Ok();
            }
            
            return BadRequest("Player has no stats to refresh");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing rank for player {PlayerId}", id);
            return StatusCode(500, "Error refreshing player rank");
        }
    }
    
    [HttpPost("{id}/update-matches")]
    public async Task<IActionResult> UpdatePlayerMatches(int id, [FromQuery] int maxMatches = 20)
    {
        var player = await _context.Players
            .Include(p => p.CurrentStats)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            
        if (player == null)
        {
            return NotFound();
        }
        
        try
        {
            _logger.LogInformation("Manually updating matches for player {GameName}#{TagLine}", player.GameName, player.TagLine);
            
            var updated = await _matchUpdateService.UpdatePlayerMatchesAsync(player, maxMatches);
            
            if (updated)
            {
                // Recharger les stats mises à jour
                await _context.Entry(player).Reference(p => p.CurrentStats).LoadAsync();
                
                // Notifier les clients connectés
                await _hubContext.Clients.Group("Leaderboard").SendAsync("StatsUpdated", new PlayerDto
                {
                    Id = player.Id,
                    RiotId = player.RiotId,
                    GameName = player.GameName,
                    TagLine = player.TagLine,
                    Region = player.Region,
                    JoinedAt = player.JoinedAt,
                    IsActive = player.IsActive,
                    CurrentStats = player.CurrentStats == null ? null : new PlayerStatsDto
                    {
                        CurrentTier = player.CurrentStats.CurrentTier,
                        CurrentRank = player.CurrentStats.CurrentRank,
                        CurrentLeaguePoints = player.CurrentStats.CurrentLeaguePoints,
                        TotalGames = player.CurrentStats.TotalGames,
                        TotalWins = player.CurrentStats.TotalWins,
                        TotalLosses = player.CurrentStats.TotalLosses,
                        WinRate = player.CurrentStats.WinRate,
                        AverageKills = player.CurrentStats.AverageKills,
                        AverageDeaths = player.CurrentStats.AverageDeaths,
                        AverageAssists = player.CurrentStats.AverageAssists,
                        KDA = player.CurrentStats.KDA,
                        AverageCreepScore = player.CurrentStats.AverageCreepScore,
                        AverageVisionScore = player.CurrentStats.AverageVisionScore,
                        AverageDamageDealt = player.CurrentStats.AverageDamageDealt,
                        CurrentWinStreak = player.CurrentStats.CurrentWinStreak,
                        CurrentLoseStreak = player.CurrentStats.CurrentLoseStreak,
                        LongestWinStreak = player.CurrentStats.LongestWinStreak,
                        LongestLoseStreak = player.CurrentStats.LongestLoseStreak,
                        NetLpChange = player.CurrentStats.NetLpChange,
                        LastUpdated = player.CurrentStats.LastUpdated
                    }
                });
                
                _logger.LogInformation("Successfully updated matches for player {GameName}#{TagLine}", player.GameName, player.TagLine);
                return Ok(new { message = "Matches updated successfully", updated = true });
            }
            else
            {
                _logger.LogWarning("No new matches found for player {GameName}#{TagLine}", player.GameName, player.TagLine);
                return Ok(new { message = "No new matches found", updated = false });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matches for player {PlayerId}", id);
            return StatusCode(500, "Error updating player matches");
        }
    }
}