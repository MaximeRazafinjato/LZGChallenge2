using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.DTOs.RiotApi;

namespace LZGChallenge2.Api.Services;

public interface IMatchUpdateService
{
    Task<bool> UpdatePlayerMatchesAsync(Player player, int maxMatches = 20);
    Task UpdatePlayerStatsAsync(Player player);
}

public class MatchUpdateService : IMatchUpdateService
{
    private readonly AppDbContext _context;
    private readonly IRiotApiService _riotApiService;
    private readonly ILogger<MatchUpdateService> _logger;
    
    public MatchUpdateService(
        AppDbContext context,
        IRiotApiService riotApiService,
        ILogger<MatchUpdateService> logger)
    {
        _context = context;
        _riotApiService = riotApiService;
        _logger = logger;
    }
    
    public async Task<bool> UpdatePlayerMatchesAsync(Player player, int maxMatches = 20)
    {
        try
        {
            _logger.LogInformation("Updating matches for player {GameName}#{TagLine}", player.GameName, player.TagLine);
            
            // Récupérer les IDs des matches récents (Solo/Duo uniquement)
            var matchIds = await _riotApiService.GetMatchIdsByPuuidAsync(player.Puuid, maxMatches);
            
            if (!matchIds.Any())
            {
                _logger.LogWarning("No matches found for player {GameName}#{TagLine}", player.GameName, player.TagLine);
                return false;
            }
            
            _logger.LogInformation("Found {Count} matches for player {GameName}#{TagLine}", matchIds.Count, player.GameName, player.TagLine);
            
            var existingMatchIds = await _context.Matches
                .Where(m => m.PlayerId == player.Id)
                .Select(m => m.MatchId)
                .ToListAsync();
            
            var newMatchIds = matchIds.Where(id => !existingMatchIds.Contains(id)).ToList();
            
            _logger.LogInformation("Processing {Count} new matches for player {GameName}#{TagLine}", 
                newMatchIds.Count, player.GameName, player.TagLine);
            
            foreach (var matchId in newMatchIds)
            {
                await ProcessMatchAsync(player, matchId);
                
                // Petite pause pour éviter de surcharger l'API
                await Task.Delay(100);
            }
            
            // Mettre à jour les statistiques après avoir ajouté les matches
            await UpdatePlayerStatsAsync(player);
            
            _logger.LogInformation("Successfully updated matches for player {GameName}#{TagLine}", 
                player.GameName, player.TagLine);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matches for player {PlayerId}", player.Id);
            return false;
        }
    }
    
    private async Task ProcessMatchAsync(Player player, string matchId)
    {
        try
        {
            var matchDto = await _riotApiService.GetMatchByIdAsync(matchId);
            if (matchDto == null)
            {
                _logger.LogWarning("Could not retrieve match {MatchId}", matchId);
                return;
            }
            
            // Vérifier que c'est bien une partie Solo/Duo (queueId 420)
            if (matchDto.Info.QueueId != 420)
            {
                _logger.LogDebug("Skipping match {MatchId} - not Solo/Duo queue (queueId: {QueueId})", 
                    matchId, matchDto.Info.QueueId);
                return;
            }
            
            // Trouver le participant correspondant au joueur
            var participant = matchDto.Info.Participants.FirstOrDefault(p => p.Puuid == player.Puuid);
            if (participant == null)
            {
                _logger.LogWarning("Player {GameName}#{TagLine} not found in match {MatchId}", 
                    player.GameName, player.TagLine, matchId);
                return;
            }
            
            // Créer l'entité Match
            var match = new Match
            {
                MatchId = matchId,
                PlayerId = player.Id,
                GameStartTime = DateTimeOffset.FromUnixTimeMilliseconds(matchDto.Info.GameStartTimestamp).DateTime,
                GameEndTime = DateTimeOffset.FromUnixTimeMilliseconds(matchDto.Info.GameEndTimestamp).DateTime,
                GameDuration = matchDto.Info.GameDuration,
                Win = participant.Win,
                QueueId = matchDto.Info.QueueId,
                Kills = participant.Kills,
                Deaths = participant.Deaths,
                Assists = participant.Assists,
                ChampionId = participant.ChampionId,
                ChampionName = participant.ChampionName,
                Position = participant.TeamPosition,
                Level = participant.ChampLevel,
                TotalDamageDealtToChampions = participant.TotalDamageDealtToChampions,
                TotalDamageTaken = participant.TotalDamageTaken,
                GoldEarned = participant.GoldEarned,
                CreepScore = participant.TotalMinionsKilled + participant.NeutralMinionsKilled,
                VisionScore = participant.VisionScore,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
            
            _logger.LogDebug("Added match {MatchId} for player {GameName}#{TagLine} - {Result}", 
                matchId, player.GameName, player.TagLine, participant.Win ? "Win" : "Loss");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing match {MatchId} for player {PlayerId}", matchId, player.Id);
        }
    }
    
    public async Task UpdatePlayerStatsAsync(Player player)
    {
        try
        {
            var matches = await _context.Matches
                .Where(m => m.PlayerId == player.Id)
                .OrderByDescending(m => m.GameStartTime)
                .ToListAsync();
            
            if (!matches.Any())
            {
                _logger.LogWarning("No matches found for player {GameName}#{TagLine} to calculate stats", 
                    player.GameName, player.TagLine);
                return;
            }
            
            var stats = await _context.PlayerStats.FirstOrDefaultAsync(ps => ps.PlayerId == player.Id);
            if (stats == null)
            {
                _logger.LogWarning("No stats record found for player {GameName}#{TagLine}", 
                    player.GameName, player.TagLine);
                return;
            }
            
            // Calculer les statistiques basées sur les matches
            var totalMatches = matches.Count;
            var wins = matches.Count(m => m.Win);
            var losses = totalMatches - wins;
            
            var totalKills = matches.Sum(m => m.Kills);
            var totalDeaths = matches.Sum(m => m.Deaths);
            var totalAssists = matches.Sum(m => m.Assists);
            
            var avgKills = totalMatches > 0 ? (double)totalKills / totalMatches : 0;
            var avgDeaths = totalMatches > 0 ? (double)totalDeaths / totalMatches : 0;
            var avgAssists = totalMatches > 0 ? (double)totalAssists / totalMatches : 0;
            var kda = totalDeaths > 0 ? (double)(totalKills + totalAssists) / totalDeaths : totalKills + totalAssists;
            
            var avgCs = totalMatches > 0 ? matches.Average(m => m.CreepScore) : 0;
            var avgVisionScore = totalMatches > 0 ? matches.Average(m => m.VisionScore) : 0;
            var avgDamage = totalMatches > 0 ? matches.Average(m => m.TotalDamageDealtToChampions) : 0;
            
            // Calculer les séries de victoires/défaites
            var currentStreak = CalculateCurrentStreak(matches);
            var (longestWinStreak, longestLoseStreak) = CalculateLongestStreaks(matches);
            
            // Mettre à jour les stats (garder les infos de rang existantes)
            stats.TotalGames = totalMatches;
            stats.TotalWins = wins;
            stats.TotalLosses = losses;
            stats.TotalKills = totalKills;
            stats.TotalDeaths = totalDeaths;
            stats.TotalAssists = totalAssists;
            // AverageKills, AverageDeaths, AverageAssists et KDA sont des propriétés calculées
            stats.AverageCreepScore = avgCs;
            stats.AverageVisionScore = avgVisionScore;
            stats.AverageDamageDealt = avgDamage;
            stats.CurrentWinStreak = currentStreak.Item1;
            stats.CurrentLoseStreak = currentStreak.Item2;
            stats.LongestWinStreak = longestWinStreak;
            stats.LongestLoseStreak = longestLoseStreak;
            stats.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated stats for player {GameName}#{TagLine}: {Wins}W/{Losses}L, KDA: {KDA:F2}", 
                player.GameName, player.TagLine, wins, losses, kda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stats for player {PlayerId}", player.Id);
        }
    }
    
    private static (int winStreak, int loseStreak) CalculateCurrentStreak(List<Match> matches)
    {
        if (!matches.Any()) return (0, 0);
        
        var orderedMatches = matches.OrderByDescending(m => m.GameStartTime).ToList();
        var currentWinStreak = 0;
        var currentLoseStreak = 0;
        
        // Compter la série actuelle
        foreach (var match in orderedMatches)
        {
            if (match.Win)
            {
                if (currentLoseStreak > 0) break; // Fin de la série de défaites
                currentWinStreak++;
            }
            else
            {
                if (currentWinStreak > 0) break; // Fin de la série de victoires
                currentLoseStreak++;
            }
        }
        
        return (currentWinStreak, currentLoseStreak);
    }
    
    private static (int longestWin, int longestLose) CalculateLongestStreaks(List<Match> matches)
    {
        if (!matches.Any()) return (0, 0);
        
        var orderedMatches = matches.OrderBy(m => m.GameStartTime).ToList();
        
        int longestWinStreak = 0;
        int longestLoseStreak = 0;
        int currentWinStreak = 0;
        int currentLoseStreak = 0;
        
        foreach (var match in orderedMatches)
        {
            if (match.Win)
            {
                currentWinStreak++;
                currentLoseStreak = 0;
                longestWinStreak = Math.Max(longestWinStreak, currentWinStreak);
            }
            else
            {
                currentLoseStreak++;
                currentWinStreak = 0;
                longestLoseStreak = Math.Max(longestLoseStreak, currentLoseStreak);
            }
        }
        
        return (longestWinStreak, longestLoseStreak);
    }
}