using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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
    private readonly MongoDbContext _context;
    private readonly IRiotApiService _riotApiService;
    private readonly IMatchUpdateService _matchUpdateService;
    private readonly IHubContext<LeaderboardHub> _hubContext;
    private readonly ILogger<PlayersController> _logger;
    
    public PlayersController(
        MongoDbContext context,
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
            .Find(p => p.IsActive)
            .ToListAsync();

        var playerDtos = new List<PlayerDto>();
        foreach (var p in players)
        {
            var stats = await _context.PlayerStats
                .Find(ps => ps.PlayerId == p.Id)
                .FirstOrDefaultAsync();

            playerDtos.Add(new PlayerDto
            {
                Id = p.Id,
                RiotId = p.RiotId,
                GameName = p.GameName,
                TagLine = p.TagLine,
                Region = p.Region,
                JoinedAt = p.JoinedAt,
                IsActive = p.IsActive,
                CurrentStats = stats == null ? null : new PlayerStatsDto
                {
                    CurrentTier = stats.CurrentTier,
                    CurrentRank = stats.CurrentRank,
                    CurrentLeaguePoints = stats.CurrentLeaguePoints,
                    TotalGames = stats.TotalGames,
                    TotalWins = stats.TotalWins,
                    TotalLosses = stats.TotalLosses,
                    WinRate = stats.WinRate,
                    AverageKills = stats.AverageKills,
                    AverageDeaths = stats.AverageDeaths,
                    AverageAssists = stats.AverageAssists,
                    KDA = stats.KDA,
                    AverageCreepScore = stats.AverageCreepScore,
                    AverageVisionScore = stats.AverageVisionScore,
                    AverageDamageDealt = stats.AverageDamageDealt,
                    CurrentWinStreak = stats.CurrentWinStreak,
                    CurrentLoseStreak = stats.CurrentLoseStreak,
                    LongestWinStreak = stats.LongestWinStreak,
                    LongestLoseStreak = stats.LongestLoseStreak,
                    NetLpChange = stats.NetLpChange,
                    LastUpdated = stats.LastUpdated
                }
            });
        }
        
        return Ok(playerDtos);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(string id)
    {
        var player = await _context.Players
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync();
        
        if (player == null)
        {
            return NotFound();
        }

        var stats = await _context.PlayerStats
            .Find(ps => ps.PlayerId == player.Id)
            .FirstOrDefaultAsync();

        var playerDto = new PlayerDto
        {
            Id = player.Id,
            RiotId = player.RiotId,
            GameName = player.GameName,
            TagLine = player.TagLine,
            Region = player.Region,
            JoinedAt = player.JoinedAt,
            IsActive = player.IsActive,
            CurrentStats = stats == null ? null : new PlayerStatsDto
            {
                CurrentTier = stats.CurrentTier,
                CurrentRank = stats.CurrentRank,
                CurrentLeaguePoints = stats.CurrentLeaguePoints,
                TotalGames = stats.TotalGames,
                TotalWins = stats.TotalWins,
                TotalLosses = stats.TotalLosses,
                WinRate = stats.WinRate,
                AverageKills = stats.AverageKills,
                AverageDeaths = stats.AverageDeaths,
                AverageAssists = stats.AverageAssists,
                KDA = stats.KDA,
                AverageCreepScore = stats.AverageCreepScore,
                AverageVisionScore = stats.AverageVisionScore,
                AverageDamageDealt = stats.AverageDamageDealt,
                CurrentWinStreak = stats.CurrentWinStreak,
                CurrentLoseStreak = stats.CurrentLoseStreak,
                LongestWinStreak = stats.LongestWinStreak,
                LongestLoseStreak = stats.LongestLoseStreak,
                NetLpChange = stats.NetLpChange,
                LastUpdated = stats.LastUpdated
            }
        };
        
        return Ok(playerDto);
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer([FromBody] CreatePlayerDto createPlayerDto)
    {
        try
        {
            // Vérifier si le joueur existe déjà
            var existingPlayer = await _context.Players
                .Find(p => p.GameName.ToLower() == createPlayerDto.GameName.ToLower() && 
                          p.TagLine.ToLower() == createPlayerDto.TagLine.ToLower())
                .FirstOrDefaultAsync();

            if (existingPlayer != null)
            {
                return Conflict("Un joueur avec ce GameName et TagLine existe déjà");
            }

            // Récupérer les informations du joueur depuis Riot API
            var riotPlayer = await _riotApiService.GetAccountByRiotIdAsync(createPlayerDto.GameName, createPlayerDto.TagLine);
            if (riotPlayer == null)
            {
                return BadRequest("Joueur introuvable sur Riot Games");
            }

            var summonerInfo = await _riotApiService.GetSummonerByPuuidAsync(riotPlayer.Puuid);
            if (summonerInfo == null)
            {
                return BadRequest("Informations du summoner introuvables");
            }

            var leagueEntries = await _riotApiService.GetLeagueEntriesByPuuidAsync(riotPlayer.Puuid);
            var soloQueueEntry = leagueEntries.FirstOrDefault(entry => entry.QueueType == "RANKED_SOLO_5x5");

            // Créer le joueur
            var player = new Player
            {
                RiotId = $"{riotPlayer.GameName}#{riotPlayer.TagLine}",
                GameName = riotPlayer.GameName,
                TagLine = riotPlayer.TagLine,
                Puuid = riotPlayer.Puuid,
                SummonerId = summonerInfo.Id,
                Region = "EUW1",
                IsActive = true,
                JoinedAt = DateTime.UtcNow
            };

            await _context.Players.InsertOneAsync(player);

            // Créer les stats initiales
            var playerStats = new PlayerStats
            {
                PlayerId = player.Id,
                CurrentTier = soloQueueEntry?.Tier,
                CurrentRank = soloQueueEntry?.Rank,
                CurrentLeaguePoints = soloQueueEntry?.LeaguePoints ?? 0,
                TotalGames = soloQueueEntry?.Wins + soloQueueEntry?.Losses ?? 0,
                TotalWins = soloQueueEntry?.Wins ?? 0,
                TotalLosses = soloQueueEntry?.Losses ?? 0,
                LastUpdated = DateTime.UtcNow
            };

            await _context.PlayerStats.InsertOneAsync(playerStats);

            _logger.LogInformation("Player created: {GameName}#{TagLine}", player.GameName, player.TagLine);

            // Lancer automatiquement la mise à jour des matches et statistiques détaillées
            try
            {
                _logger.LogInformation("Starting automatic data update for new player {GameName}#{TagLine}", player.GameName, player.TagLine);
                await _matchUpdateService.UpdatePlayerMatchesAsync(player);
                _logger.LogInformation("Successfully updated data for new player {GameName}#{TagLine}", player.GameName, player.TagLine);
                
                // Notifier les clients connectés
                await _hubContext.Clients.All.SendAsync("PlayerAdded", player);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Error updating data for new player {GameName}#{TagLine}, but player was created successfully", player.GameName, player.TagLine);
                // On ne fait pas échouer la création du joueur si la mise à jour échoue
            }

            return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating player {GameName}#{TagLine}", createPlayerDto.GameName, createPlayerDto.TagLine);
            return StatusCode(500, "Erreur lors de la création du joueur");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(string id, [FromBody] UpdatePlayerDto updatePlayerDto)
    {
        try
        {
            var player = await _context.Players
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (player == null)
            {
                return NotFound();
            }

            player.IsActive = updatePlayerDto.IsActive;

            await _context.Players.ReplaceOneAsync(p => p.Id == id, player);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player {PlayerId}", id);
            return StatusCode(500, "Erreur lors de la mise à jour du joueur");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(string id)
    {
        try
        {
            var player = await _context.Players
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (player == null)
            {
                return NotFound();
            }

            await _context.Players.DeleteOneAsync(p => p.Id == id);
            await _context.PlayerStats.DeleteManyAsync(ps => ps.PlayerId == id);
            await _context.ChampionStats.DeleteManyAsync(cs => cs.PlayerId == id);
            await _context.RoleStats.DeleteManyAsync(rs => rs.PlayerId == id);
            await _context.Matches.DeleteManyAsync(m => m.PlayerId == id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting player {PlayerId}", id);
            return StatusCode(500, "Erreur lors de la suppression du joueur");
        }
    }

    [HttpPost("{id}/refresh")]
    public async Task<IActionResult> RefreshPlayer(string id)
    {
        try
        {
            var player = await _context.Players
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (player == null)
            {
                return NotFound();
            }

            // Mettre à jour les informations de base du joueur
            var summonerInfo = await _riotApiService.GetSummonerByPuuidAsync(player.Puuid);
            if (summonerInfo == null)
            {
                return BadRequest("Informations du summoner introuvables");
            }

            var leagueEntries = await _riotApiService.GetLeagueEntriesByPuuidAsync(player.Puuid);
            var soloQueueEntry = leagueEntries.FirstOrDefault(entry => entry.QueueType == "RANKED_SOLO_5x5");
            
            // Mettre à jour les stats de base depuis l'API League
            var stats = await _context.PlayerStats
                .Find(ps => ps.PlayerId == player.Id)
                .FirstOrDefaultAsync();

            if (stats != null)
            {
                stats.CurrentTier = soloQueueEntry?.Tier;
                stats.CurrentRank = soloQueueEntry?.Rank;
                stats.CurrentLeaguePoints = soloQueueEntry?.LeaguePoints ?? 0;
                stats.TotalGames = soloQueueEntry?.Wins + soloQueueEntry?.Losses ?? 0;
                stats.TotalWins = soloQueueEntry?.Wins ?? 0;
                stats.TotalLosses = soloQueueEntry?.Losses ?? 0;
                stats.LastUpdated = DateTime.UtcNow;

                await _context.PlayerStats.ReplaceOneAsync(ps => ps.PlayerId == player.Id, stats);
            }

            // Mettre à jour les matches et stats détaillées
            await _matchUpdateService.UpdatePlayerMatchesAsync(player);

            await _hubContext.Clients.All.SendAsync("PlayerUpdated", player.Id);

            return Ok("Joueur mis à jour avec succès");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing player {PlayerId}", id);
            return StatusCode(500, "Erreur lors de la mise à jour du joueur");
        }
    }

    [HttpPost("refresh-all")]
    public async Task<IActionResult> RefreshAllPlayers()
    {
        try
        {
            var players = await _context.Players
                .Find(p => p.IsActive)
                .ToListAsync();

            var refreshedCount = 0;
            foreach (var player in players)
            {
                try
                {
                    await _matchUpdateService.UpdatePlayerMatchesAsync(player);
                    refreshedCount++;
                    _logger.LogInformation("Refreshed player {GameName}#{TagLine}", player.GameName, player.TagLine);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing player {GameName}#{TagLine}", player.GameName, player.TagLine);
                }
            }

            await _hubContext.Clients.All.SendAsync("AllPlayersUpdated");

            return Ok($"Mis à jour {refreshedCount}/{players.Count} joueurs");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing all players");
            return StatusCode(500, "Erreur lors de la mise à jour des joueurs");
        }
    }
}