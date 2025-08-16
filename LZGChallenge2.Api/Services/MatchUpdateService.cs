using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.DTOs.RiotApi;

namespace LZGChallenge2.Api.Services;

public interface IMatchUpdateService
{
    Task<bool> UpdatePlayerMatchesAsync(Player player, int maxMatches = 20);
    Task UpdatePlayerStatsAsync(Player player);
    Task UpdateChampionStatsAsync(Player player);
    Task UpdateRoleStatsAsync(Player player);
}

public class MatchUpdateService : IMatchUpdateService
{
    private readonly AppDbContext _context;
    private readonly IRiotApiService _riotApiService;
    private readonly ISeasonService _seasonService;
    private readonly ILogger<MatchUpdateService> _logger;
    
    public MatchUpdateService(
        AppDbContext context,
        IRiotApiService riotApiService,
        ISeasonService seasonService,
        ILogger<MatchUpdateService> logger)
    {
        _context = context;
        _riotApiService = riotApiService;
        _seasonService = seasonService;
        _logger = logger;
    }
    
    public async Task<bool> UpdatePlayerMatchesAsync(Player player, int maxMatches = 20)
    {
        try
        {
            _logger.LogInformation("Updating matches for player {GameName}#{TagLine}", player.GameName, player.TagLine);
            
            // Récupérer les IDs des matches de la saison (Solo/Duo uniquement)
            var allMatchIds = new List<string>();
            var batchSize = 100; // Limite de l'API Riot
            var start = 0;
            
            while (allMatchIds.Count < maxMatches)
            {
                var remaining = Math.Min(batchSize, maxMatches - allMatchIds.Count);
                var batchMatchIds = await _riotApiService.GetMatchIdsByPuuidAsync(player.Puuid, remaining, start);
                
                if (!batchMatchIds.Any())
                {
                    // Plus de matches disponibles
                    break;
                }
                
                allMatchIds.AddRange(batchMatchIds);
                start += batchMatchIds.Count;
                
                // Si on a reçu moins de matches que demandé, on a atteint la fin
                if (batchMatchIds.Count < remaining)
                {
                    break;
                }
            }
            
            var matchIds = allMatchIds;
            
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
            
            _logger.LogInformation("Found {TotalMatches} total matches, {NewMatches} new matches for player {GameName}#{TagLine}", 
                matchIds.Count, newMatchIds.Count, player.GameName, player.TagLine);
            
            // Traiter seulement les nouveaux matches pour éviter les appels API inutiles
            if (newMatchIds.Any())
            {
                foreach (var matchId in newMatchIds)
                {
                    await ProcessMatchAsync(player, matchId);
                    
                    // Petite pause pour éviter de surcharger l'API
                    await Task.Delay(100);
                }
                _logger.LogInformation("Processed {Count} new matches for player {GameName}#{TagLine}", 
                    newMatchIds.Count, player.GameName, player.TagLine);
            }
            else
            {
                _logger.LogInformation("No new matches to process for player {GameName}#{TagLine}", 
                    player.GameName, player.TagLine);
            }
            
            // Mettre à jour les statistiques après avoir ajouté les matches
            // Toujours recalculer même s'il n'y a pas de nouveaux matches
            await UpdatePlayerStatsAsync(player);
            await UpdateChampionStatsAsync(player);
            await UpdateRoleStatsAsync(player);
            
            _logger.LogInformation("Successfully updated stats for player {GameName}#{TagLine} - Total matches found: {TotalMatches}", 
                player.GameName, player.TagLine, matchIds.Count);
            
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
            var gameStartTime = DateTimeOffset.FromUnixTimeMilliseconds(matchDto.Info.GameStartTimestamp).DateTime;
            var match = new Match
            {
                MatchId = matchId,
                PlayerId = player.Id,
                GameStartTime = gameStartTime,
                GameEndTime = DateTimeOffset.FromUnixTimeMilliseconds(matchDto.Info.GameEndTimestamp).DateTime,
                GameDuration = matchDto.Info.GameDuration,
                Win = participant.Win,
                QueueId = matchDto.Info.QueueId,
                Season = _seasonService.GetSeasonFromDate(gameStartTime),
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
            var currentSeason = _seasonService.GetCurrentSeason();
            var matches = await _context.Matches
                .Where(m => m.PlayerId == player.Id && m.QueueId == 420 && m.Season == currentSeason)
                .OrderByDescending(m => m.GameStartTime)
                .ToListAsync();
            
            var stats = await _context.PlayerStats.FirstOrDefaultAsync(ps => ps.PlayerId == player.Id);
            if (stats == null)
            {
                _logger.LogWarning("No stats record found for player {GameName}#{TagLine}", 
                    player.GameName, player.TagLine);
                return;
            }
            
            if (!matches.Any())
            {
                // Vérifier s'il y a des données polluées à nettoyer
                var hasStaleData = stats.TotalKills > 0 || stats.TotalDeaths > 0 || stats.TotalAssists > 0 ||
                                  stats.AverageCreepScore > 0 || stats.AverageVisionScore > 0 || stats.AverageDamageDealt > 0 ||
                                  stats.CurrentWinStreak > 0 || stats.CurrentLoseStreak > 0 ||
                                  stats.LongestWinStreak > 0 || stats.LongestLoseStreak > 0;
                
                if (hasStaleData)
                {
                    _logger.LogWarning("Player {GameName}#{TagLine} has no matches but has stale stats - cleaning up", 
                        player.GameName, player.TagLine);
                }
                else
                {
                    _logger.LogInformation("Player {GameName}#{TagLine} has no matches and stats are already clean", 
                        player.GameName, player.TagLine);
                }
                
                // TOUJOURS remettre toutes les statistiques détaillées à zéro (garder les W/L de l'API League)
                stats.TotalKills = 0;
                stats.TotalDeaths = 0;
                stats.TotalAssists = 0;
                stats.AverageCreepScore = 0;
                stats.AverageVisionScore = 0;
                stats.AverageDamageDealt = 0;
                stats.CurrentWinStreak = 0;
                stats.CurrentLoseStreak = 0;
                stats.LongestWinStreak = 0;
                stats.LongestLoseStreak = 0;
                stats.TotalLpGained = 0;
                stats.TotalLpLost = 0;
                stats.LastUpdated = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Reset all detailed stats to zero for player {GameName}#{TagLine} (no matches found)", 
                    player.GameName, player.TagLine);
                return;
            }
            
            // Calculer les statistiques détaillées basées sur les matches analysés
            // IMPORTANT: On garde les TotalGames/TotalWins/TotalLosses de l'API League (plus précis)
            // et on calcule seulement les stats détaillées (KDA, CS, etc.) sur les matches analysés
            var analyzedMatches = matches.Count;
            
            var totalKills = matches.Sum(m => m.Kills);
            var totalDeaths = matches.Sum(m => m.Deaths);
            var totalAssists = matches.Sum(m => m.Assists);
            
            var avgKills = analyzedMatches > 0 ? (double)totalKills / analyzedMatches : 0;
            var avgDeaths = analyzedMatches > 0 ? (double)totalDeaths / analyzedMatches : 0;
            var avgAssists = analyzedMatches > 0 ? (double)totalAssists / analyzedMatches : 0;
            var kda = totalDeaths > 0 ? (double)(totalKills + totalAssists) / totalDeaths : totalKills + totalAssists;
            
            var avgCs = analyzedMatches > 0 ? matches.Average(m => m.CreepScore) : 0;
            var avgVisionScore = analyzedMatches > 0 ? matches.Average(m => m.VisionScore) : 0;
            var avgDamage = analyzedMatches > 0 ? matches.Average(m => m.TotalDamageDealtToChampions) : 0;
            
            // Calculer les séries de victoires/défaites
            var currentStreak = CalculateCurrentStreak(matches);
            var (longestWinStreak, longestLoseStreak) = CalculateLongestStreaks(matches);
            
            // Mettre à jour les stats (garder les TotalGames/TotalWins/TotalLosses de l'API League)
            // NE PAS écraser les vraies statistiques W/L qui viennent de l'API League
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
            
            _logger.LogInformation("Updated stats for player {GameName}#{TagLine}: {TotalGames} games total, {AnalyzedMatches} analyzed, KDA: {KDA:F2}", 
                player.GameName, player.TagLine, stats.TotalGames, analyzedMatches, kda);
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
    
    public async Task UpdateChampionStatsAsync(Player player)
    {
        try
        {
            var currentSeason = _seasonService.GetCurrentSeason();
            var matches = await _context.Matches
                .Where(m => m.PlayerId == player.Id && m.QueueId == 420 && m.Season == currentSeason)
                .ToListAsync();
            
            // Regrouper par champion
            var championGroups = matches.GroupBy(m => new { m.ChampionId, m.ChampionName });
            
            // Supprimer les anciennes stats pour ce joueur
            var existingChampionStats = await _context.ChampionStats
                .Where(cs => cs.PlayerId == player.Id)
                .ToListAsync();
            _context.ChampionStats.RemoveRange(existingChampionStats);
            
            foreach (var championGroup in championGroups)
            {
                var championMatches = championGroup.ToList();
                
                var championStats = new ChampionStats
                {
                    PlayerId = player.Id,
                    ChampionId = championGroup.Key.ChampionId,
                    ChampionName = championGroup.Key.ChampionName,
                    GamesPlayed = championMatches.Count,
                    Wins = championMatches.Count(m => m.Win),
                    Losses = championMatches.Count(m => !m.Win),
                    TotalKills = championMatches.Sum(m => m.Kills),
                    TotalDeaths = championMatches.Sum(m => m.Deaths),
                    TotalAssists = championMatches.Sum(m => m.Assists),
                    AverageCreepScore = championMatches.Average(m => m.CreepScore),
                    AverageVisionScore = championMatches.Average(m => m.VisionScore),
                    AverageDamageDealt = championMatches.Average(m => m.TotalDamageDealtToChampions),
                    AverageGameDuration = championMatches.Average(m => m.GameDuration),
                    LastPlayedAt = championMatches.Max(m => m.GameStartTime),
                    LastUpdated = DateTime.UtcNow
                };
                
                _context.ChampionStats.Add(championStats);
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated champion stats for player {GameName}#{TagLine}: {ChampionCount} champions", 
                player.GameName, player.TagLine, championGroups.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating champion stats for player {PlayerId}", player.Id);
        }
    }
    
    public async Task UpdateRoleStatsAsync(Player player)
    {
        try
        {
            var currentSeason = _seasonService.GetCurrentSeason();
            var matches = await _context.Matches
                .Where(m => m.PlayerId == player.Id && m.QueueId == 420 && m.Season == currentSeason)
                .ToListAsync();
            
            if (!matches.Any())
            {
                // Supprimer les stats existantes s'il n'y a pas de matches
                var existingRoleStatsToRemove = await _context.RoleStats
                    .Where(rs => rs.PlayerId == player.Id)
                    .ToListAsync();
                _context.RoleStats.RemoveRange(existingRoleStatsToRemove);
                await _context.SaveChangesAsync();
                return;
            }
            
            // Regrouper par rôle
            var roleGroups = matches.GroupBy(m => m.Position);
            var totalGames = matches.Count;
            
            // Supprimer les anciennes stats pour ce joueur
            var existingRoleStats = await _context.RoleStats
                .Where(rs => rs.PlayerId == player.Id)
                .ToListAsync();
            _context.RoleStats.RemoveRange(existingRoleStats);
            
            foreach (var roleGroup in roleGroups)
            {
                var roleMatches = roleGroup.ToList();
                
                var roleStats = new RoleStats
                {
                    PlayerId = player.Id,
                    Position = roleGroup.Key,
                    GamesPlayed = roleMatches.Count,
                    Wins = roleMatches.Count(m => m.Win),
                    Losses = roleMatches.Count(m => !m.Win),
                    PlayRate = (double)roleMatches.Count / totalGames * 100,
                    TotalKills = roleMatches.Sum(m => m.Kills),
                    TotalDeaths = roleMatches.Sum(m => m.Deaths),
                    TotalAssists = roleMatches.Sum(m => m.Assists),
                    AverageCreepScore = roleMatches.Average(m => m.CreepScore),
                    AverageVisionScore = roleMatches.Average(m => m.VisionScore),
                    AverageDamageDealt = roleMatches.Average(m => m.TotalDamageDealtToChampions),
                    LastUpdated = DateTime.UtcNow
                };
                
                _context.RoleStats.Add(roleStats);
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated role stats for player {GameName}#{TagLine}: {RoleCount} roles", 
                player.GameName, player.TagLine, roleGroups.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role stats for player {PlayerId}", player.Id);
        }
    }
}