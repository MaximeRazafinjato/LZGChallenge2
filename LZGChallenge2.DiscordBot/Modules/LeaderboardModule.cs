using Discord.Commands;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using System.Text;

namespace LZGChallenge2.DiscordBot.Modules;

public class LeaderboardModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;

    public LeaderboardModule(IServiceProvider services)
    {
        _services = services;
    }

    [Command("leaderboard")]
    [Alias("lb", "classement")]
    public async Task LeaderboardAsync()
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        var allPlayers = await dbContext.Players
            .Find(p => p.IsActive)
            .ToListAsync();
            
        var playersWithStats = new List<Player>();
        foreach (var player in allPlayers)
        {
            var stats = await dbContext.PlayerStats
                .Find(ps => ps.PlayerId == player.Id)
                .FirstOrDefaultAsync();
            if (stats != null)
            {
                player.CurrentStats = stats;
                playersWithStats.Add(player);
            }
        }

        var players = playersWithStats
            .OrderByDescending(p => CalculateRankScore(p.CurrentStats!.CurrentTier, p.CurrentStats.CurrentRank, p.CurrentStats.CurrentLeaguePoints))
            .Take(10)
            .ToList();

        if (!players.Any())
        {
            await ReplyAsync("Aucun joueur trouvé dans le classement.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("🏆 **CLASSEMENT SOLO QUEUE CHALLENGE** 🏆");
        sb.AppendLine();

        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var position = i + 1;
            var emoji = position switch
            {
                1 => "🥇",
                2 => "🥈", 
                3 => "🥉",
                _ => $"{position}."
            };

            var rank = string.IsNullOrEmpty(player.CurrentStats?.CurrentTier) ? "Non classé" : 
                       $"{player.CurrentStats.CurrentTier} {player.CurrentStats.CurrentRank}";
            
            var winRate = player.CurrentStats?.WinRate.ToString("F1") ?? "0.0";
            var kda = player.CurrentStats?.KDA.ToString("F2") ?? "0.00";

            sb.AppendLine($"{emoji} **{player.GameName}#{player.TagLine}**");
            sb.AppendLine($"    Rang: {rank} ({player.CurrentStats?.CurrentLeaguePoints ?? 0} LP)");
            sb.AppendLine($"    WR: {winRate}% | KDA: {kda}");
            sb.AppendLine();
        }

        await ReplyAsync(sb.ToString());
    }

    private static int CalculateRankScore(string? tier, string? rank, int leaguePoints)
    {
        // Valeurs de base pour chaque tier
        var tierValues = new Dictionary<string, int>
        {
            { "IRON", 0 },
            { "BRONZE", 400 },
            { "SILVER", 800 },
            { "GOLD", 1200 },
            { "PLATINUM", 1600 },
            { "EMERALD", 2000 },
            { "DIAMOND", 2400 },
            { "MASTER", 2800 },
            { "GRANDMASTER", 3200 },
            { "CHALLENGER", 3600 }
        };
        
        // Si pas de tier, retourner 0
        if (string.IsNullOrEmpty(tier) || !tierValues.ContainsKey(tier.ToUpper()))
        {
            return 0;
        }
        
        var baseScore = tierValues[tier.ToUpper()];
        
        // Ajouter les points pour le rank (IV = 0, III = 100, II = 200, I = 300)
        var rankValue = rank?.ToUpper() switch
        {
            "IV" => 0,
            "III" => 100,
            "II" => 200,
            "I" => 300,
            _ => 0 // Master+ n'ont pas de rank
        };
        
        // Ajouter les LP (max 100 par division)
        var lpValue = Math.Min(leaguePoints, 100);
        
        return baseScore + rankValue + lpValue;
    }

    [Command("stats")]
    [Alias("stat")]
    public async Task StatsAsync([Remainder] string? playerName = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        Player? player = null;

        if (string.IsNullOrEmpty(playerName))
        {
            await ReplyAsync("Usage: `!stats <nomDuJoueur#TAG>` ou `!stats nomDuJoueur`");
            return;
        }

        if (playerName.Contains('#'))
        {
            var parts = playerName.Split('#');
            if (parts.Length == 2)
            {
                player = await dbContext.Players
                    .Find(p => p.GameName.ToLower() == parts[0].ToLower() && 
                              p.TagLine.ToLower() == parts[1].ToLower())
                    .FirstOrDefaultAsync();
                if (player != null)
                {
                    var stats = await dbContext.PlayerStats
                        .Find(ps => ps.PlayerId == player.Id)
                        .FirstOrDefaultAsync();
                    player.CurrentStats = stats;
                }
            }
        }
        else
        {
            player = await dbContext.Players
                .Find(p => p.GameName.ToLower() == playerName.ToLower())
                .FirstOrDefaultAsync();
            if (player != null)
            {
                var stats = await dbContext.PlayerStats
                    .Find(ps => ps.PlayerId == player.Id)
                    .FirstOrDefaultAsync();
                player.CurrentStats = stats;
            }
        }

        if (player == null)
        {
            await ReplyAsync($"Joueur '{playerName}' introuvable.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📊 **STATISTIQUES DE {player.GameName}#{player.TagLine}** 📊");
        sb.AppendLine();

        var rank = string.IsNullOrEmpty(player.CurrentStats?.CurrentTier) ? "Non classé" : 
                   $"{player.CurrentStats.CurrentTier} {player.CurrentStats.CurrentRank}";
        
        sb.AppendLine($"**Rang actuel:** {rank} ({player.CurrentStats?.CurrentLeaguePoints ?? 0} LP)");
        
        if (player.CurrentStats != null)
        {
            sb.AppendLine($"**Parties:** {player.CurrentStats.TotalGames} ({player.CurrentStats.TotalWins}W/{player.CurrentStats.TotalLosses}L)");
            sb.AppendLine($"**Winrate:** {player.CurrentStats.WinRate:F1}%");
            sb.AppendLine($"**KDA:** {player.CurrentStats.KDA:F2} ({player.CurrentStats.AverageKills:F1}/{player.CurrentStats.AverageDeaths:F1}/{player.CurrentStats.AverageAssists:F1})");
            
            if (player.CurrentStats.CurrentWinStreak > 0)
                sb.AppendLine($"**Série actuelle:** {player.CurrentStats.CurrentWinStreak} victoires 🔥");
            else if (player.CurrentStats.CurrentLoseStreak > 0)
                sb.AppendLine($"**Série actuelle:** {player.CurrentStats.CurrentLoseStreak} défaites 💀");
                
            sb.AppendLine($"**Meilleure série:** {player.CurrentStats.LongestWinStreak} victoires");
        }

        await ReplyAsync(sb.ToString());
    }
}