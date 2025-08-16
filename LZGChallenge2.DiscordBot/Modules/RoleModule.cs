using Discord.Commands;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using System.Text;

namespace LZGChallenge2.DiscordBot.Modules;

public class RoleModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;

    public RoleModule(IServiceProvider services)
    {
        _services = services;
    }

    [Command("roles")]
    [Alias("role", "rôles")]
    public async Task RolesAsync([Remainder] string? playerName = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        Player? player = null;

        if (string.IsNullOrEmpty(playerName))
        {
            await ReplyAsync("Usage: `!roles <nomDuJoueur#TAG>` ou `!roles nomDuJoueur`");
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
            }
        }
        else
        {
            player = await dbContext.Players
                .Find(p => p.GameName.ToLower() == playerName.ToLower())
                .FirstOrDefaultAsync();
        }

        if (player == null)
        {
            await ReplyAsync($"Joueur '{playerName}' introuvable.");
            return;
        }

        var allRoleStats = await dbContext.RoleStats
            .Find(rs => rs.PlayerId == player.Id)
            .ToListAsync();

        var roleStats = allRoleStats
            .OrderByDescending(rs => rs.PlayRate)
            .ThenByDescending(rs => rs.GamesPlayed)
            .ToList();

        if (!roleStats.Any())
        {
            await ReplyAsync($"Aucune statistique par rôle trouvée pour {player.GameName}#{player.TagLine}.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🎭 **RÉPARTITION PAR RÔLE - {player.GameName}#{player.TagLine}** 🎭");
        sb.AppendLine();

        var roleEmojis = new Dictionary<string, string>
        {
            { "TOP", "⚔️" },
            { "JUNGLE", "🌲" },
            { "MIDDLE", "🔮" },
            { "BOTTOM", "🏹" },
            { "UTILITY", "🛡️" }
        };

        foreach (var role in roleStats)
        {
            var emoji = roleEmojis.GetValueOrDefault(role.Position.ToUpper(), "🎯");
            var roleName = role.Position.ToUpper() switch
            {
                "TOP" => "Top",
                "JUNGLE" => "Jungle", 
                "MIDDLE" => "Mid",
                "BOTTOM" => "ADC",
                "UTILITY" => "Support",
                _ => role.Position
            };

            sb.AppendLine($"{emoji} **{roleName}** ({role.PlayRate:F1}%)");
            sb.AppendLine($"    Parties: {role.GamesPlayed} ({role.Wins}W/{role.Losses}L)");
            sb.AppendLine($"    WR: {role.WinRate:F1}% | KDA: {role.KDA:F2}");
            sb.AppendLine($"    CS moy: {role.AverageCreepScore:F1} | Vision: {role.AverageVisionScore:F1}");
            sb.AppendLine();
        }

        await ReplyAsync(sb.ToString());
    }

    [Command("mainrole")]
    [Alias("main", "rôleprincipal")]
    public async Task MainRoleAsync([Remainder] string? playerName = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        Player? player = null;

        if (string.IsNullOrEmpty(playerName))
        {
            await ReplyAsync("Usage: `!mainrole <nomDuJoueur#TAG>` ou `!mainrole nomDuJoueur`");
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
            }
        }
        else
        {
            player = await dbContext.Players
                .Find(p => p.GameName.ToLower() == playerName.ToLower())
                .FirstOrDefaultAsync();
        }

        if (player == null)
        {
            await ReplyAsync($"Joueur '{playerName}' introuvable.");
            return;
        }

        var allRoleStats = await dbContext.RoleStats
            .Find(rs => rs.PlayerId == player.Id)
            .ToListAsync();

        var mainRole = allRoleStats
            .OrderByDescending(rs => rs.PlayRate)
            .ThenByDescending(rs => rs.GamesPlayed)
            .FirstOrDefault();

        if (mainRole == null)
        {
            await ReplyAsync($"Aucun rôle principal trouvé pour {player.GameName}#{player.TagLine}.");
            return;
        }

        var roleEmojis = new Dictionary<string, string>
        {
            { "TOP", "⚔️" },
            { "JUNGLE", "🌲" },
            { "MIDDLE", "🔮" },
            { "BOTTOM", "🏹" },
            { "UTILITY", "🛡️" }
        };

        var emoji = roleEmojis.GetValueOrDefault(mainRole.Position.ToUpper(), "🎯");
        var roleName = mainRole.Position.ToUpper() switch
        {
            "TOP" => "Top",
            "JUNGLE" => "Jungle",
            "MIDDLE" => "Mid", 
            "BOTTOM" => "ADC",
            "UTILITY" => "Support",
            _ => mainRole.Position
        };

        var sb = new StringBuilder();
        sb.AppendLine($"{emoji} **RÔLE PRINCIPAL DE {player.GameName}#{player.TagLine}** {emoji}");
        sb.AppendLine();
        sb.AppendLine($"**{roleName}** ({mainRole.PlayRate:F1}% des parties)");
        sb.AppendLine($"**Parties:** {mainRole.GamesPlayed} ({mainRole.Wins}W/{mainRole.Losses}L)");
        sb.AppendLine($"**Winrate:** {mainRole.WinRate:F1}%");
        sb.AppendLine($"**KDA:** {mainRole.KDA:F2} ({mainRole.AverageKills:F1}/{mainRole.AverageDeaths:F1}/{mainRole.AverageAssists:F1})");
        sb.AppendLine($"**CS moyen:** {mainRole.AverageCreepScore:F1}");
        sb.AppendLine($"**Vision score:** {mainRole.AverageVisionScore:F1}");
        sb.AppendLine($"**Dégâts moyens:** {mainRole.AverageDamageDealt:F0}");

        await ReplyAsync(sb.ToString());
    }

    [Command("roleleaderboard")]
    [Alias("roleclassement", "rlb")]
    public async Task RoleLeaderboardAsync(string? position = null)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        if (string.IsNullOrEmpty(position))
        {
            await ReplyAsync("Usage: `!roleleaderboard <rôle>`\nRôles disponibles: top, jungle, mid, adc, support");
            return;
        }

        // Normaliser le nom du rôle
        var normalizedPosition = position.ToLower() switch
        {
            "top" => "TOP",
            "jungle" => "JUNGLE", 
            "mid" or "middle" => "MIDDLE",
            "adc" or "bottom" => "BOTTOM",
            "support" or "utility" => "UTILITY",
            _ => position.ToUpper()
        };

        var allRoleStats = await dbContext.RoleStats
            .Find(rs => rs.Position.ToUpper() == normalizedPosition && rs.GamesPlayed >= 3)
            .ToListAsync();
            
        var roleStatsWithPlayers = new List<RoleStats>();
        foreach (var roleStats in allRoleStats)
        {
            var player = await dbContext.Players
                .Find(p => p.Id == roleStats.PlayerId)
                .FirstOrDefaultAsync();
            if (player != null)
            {
                roleStats.Player = player;
                roleStatsWithPlayers.Add(roleStats);
            }
        }

        var roleLeaderboard = roleStatsWithPlayers
            .OrderByDescending(rs => rs.WinRate)
            .ThenByDescending(rs => rs.GamesPlayed)
            .Take(10)
            .ToList();

        if (!roleLeaderboard.Any())
        {
            await ReplyAsync($"Aucun joueur trouvé pour le rôle {normalizedPosition} (minimum 3 parties).");
            return;
        }

        var roleEmojis = new Dictionary<string, string>
        {
            { "TOP", "⚔️" },
            { "JUNGLE", "🌲" },
            { "MIDDLE", "🔮" },
            { "BOTTOM", "🏹" },
            { "UTILITY", "🛡️" }
        };

        var emoji = roleEmojis.GetValueOrDefault(normalizedPosition, "🎯");
        var roleName = normalizedPosition switch
        {
            "TOP" => "Top",
            "JUNGLE" => "Jungle",
            "MIDDLE" => "Mid",
            "BOTTOM" => "ADC", 
            "UTILITY" => "Support",
            _ => normalizedPosition
        };

        var sb = new StringBuilder();
        sb.AppendLine($"{emoji} **CLASSEMENT {roleName.ToUpper()}** {emoji}");
        sb.AppendLine();

        for (int i = 0; i < roleLeaderboard.Count; i++)
        {
            var roleStats = roleLeaderboard[i];
            var pos = i + 1;
            var posEmoji = pos switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"{pos}."
            };

            sb.AppendLine($"{posEmoji} **{roleStats.Player.GameName}#{roleStats.Player.TagLine}**");
            sb.AppendLine($"    WR: {roleStats.WinRate:F1}% ({roleStats.Wins}W/{roleStats.Losses}L)");
            sb.AppendLine($"    KDA: {roleStats.KDA:F2} | CS: {roleStats.AverageCreepScore:F1}");
            sb.AppendLine();
        }

        await ReplyAsync(sb.ToString());
    }
}