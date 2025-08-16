using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using System.Text;

namespace LZGChallenge2.DiscordBot.Modules;

public class ChampionsModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;

    public ChampionsModule(IServiceProvider services)
    {
        _services = services;
    }

    [Command("champions")]
    [Alias("champs", "champion")]
    public async Task ChampionsAsync([Remainder] string? playerName = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Player? player = null;

        if (string.IsNullOrEmpty(playerName))
        {
            await ReplyAsync("Usage: `!champions <nomDuJoueur#TAG>` ou `!champions nomDuJoueur`");
            return;
        }

        if (playerName.Contains('#'))
        {
            var parts = playerName.Split('#');
            if (parts.Length == 2)
            {
                player = await dbContext.Players
                    .FirstOrDefaultAsync(p => p.GameName.ToLower() == parts[0].ToLower() && 
                                            p.TagLine.ToLower() == parts[1].ToLower());
            }
        }
        else
        {
            player = await dbContext.Players
                .FirstOrDefaultAsync(p => p.GameName.ToLower() == playerName.ToLower());
        }

        if (player == null)
        {
            await ReplyAsync($"Joueur '{playerName}' introuvable.");
            return;
        }

        var allChampionStats = await dbContext.ChampionStats
            .Where(cs => cs.PlayerId == player.Id)
            .ToListAsync();

        var championStats = allChampionStats
            .OrderByDescending(cs => cs.GamesPlayed)
            .ThenByDescending(cs => cs.WinRate)
            .Take(5)
            .ToList();

        if (!championStats.Any())
        {
            await ReplyAsync($"Aucune statistique de champion trouvée pour {player.GameName}#{player.TagLine}.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🏅 **TOP CHAMPIONS DE {player.GameName}#{player.TagLine}** 🏅");
        sb.AppendLine();

        foreach (var champion in championStats)
        {
            var winRate = champion.GamesPlayed > 0 ? (champion.Wins * 100.0 / champion.GamesPlayed) : 0;
            var kda = champion.TotalDeaths > 0 ? (champion.TotalKills + champion.TotalAssists) / champion.TotalDeaths : champion.TotalKills + champion.TotalAssists;
            
            sb.AppendLine($"**{champion.ChampionName}**");
            sb.AppendLine($"  {champion.GamesPlayed} parties ({champion.Wins}W/{champion.Losses}L) - {winRate:F1}%");
            sb.AppendLine($"  KDA: {kda:F2} ({champion.AverageKills:F1}/{champion.AverageDeaths:F1}/{champion.AverageAssists:F1})");
            sb.AppendLine();
        }

        await ReplyAsync(sb.ToString());
    }

    [Command("progress")]
    [Alias("progression")]
    public async Task ProgressAsync([Remainder] string? playerName = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Player? player = null;

        if (string.IsNullOrEmpty(playerName))
        {
            await ReplyAsync("Usage: `!progress <nomDuJoueur#TAG>` ou `!progress nomDuJoueur`");
            return;
        }

        if (playerName.Contains('#'))
        {
            var parts = playerName.Split('#');
            if (parts.Length == 2)
            {
                player = await dbContext.Players
                    .Include(p => p.CurrentStats)
                    .FirstOrDefaultAsync(p => p.GameName.ToLower() == parts[0].ToLower() && 
                                            p.TagLine.ToLower() == parts[1].ToLower());
            }
        }
        else
        {
            player = await dbContext.Players
                .Include(p => p.CurrentStats)
                .FirstOrDefaultAsync(p => p.GameName.ToLower() == playerName.ToLower());
        }

        if (player == null)
        {
            await ReplyAsync($"Joueur '{playerName}' introuvable.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📈 **PROGRESSION DE {player.GameName}#{player.TagLine}** 📈");
        sb.AppendLine();

        var currentRank = string.IsNullOrEmpty(player.CurrentStats?.CurrentTier) ? "Non classé" : 
                         $"{player.CurrentStats.CurrentTier} {player.CurrentStats.CurrentRank}";
        sb.AppendLine($"**Rang actuel:** {currentRank} ({player.CurrentStats?.CurrentLeaguePoints ?? 0} LP)");
        sb.AppendLine();

        sb.AppendLine("**Statistiques générales:**");
        if (player.CurrentStats != null)
        {
            sb.AppendLine($"Parties: {player.CurrentStats.TotalGames} ({player.CurrentStats.TotalWins}W/{player.CurrentStats.TotalLosses}L)");
            sb.AppendLine($"LP total gagné: +{player.CurrentStats.TotalLpGained}");
            sb.AppendLine($"LP total perdu: -{player.CurrentStats.TotalLpLost}");
            sb.AppendLine($"Progression nette: {player.CurrentStats.NetLpChange:+#;-#;0} LP");
            sb.AppendLine();
            
            sb.AppendLine($"**Série actuelle:** ");
            if (player.CurrentStats.CurrentWinStreak > 0)
                sb.AppendLine($"{player.CurrentStats.CurrentWinStreak} victoires 🔥");
            else if (player.CurrentStats.CurrentLoseStreak > 0)
                sb.AppendLine($"{player.CurrentStats.CurrentLoseStreak} défaites 💀");
            else
                sb.AppendLine("Aucune série en cours");
        }
        else
        {
            sb.AppendLine("Aucune statistique disponible.");
        }

        await ReplyAsync(sb.ToString());
    }
}