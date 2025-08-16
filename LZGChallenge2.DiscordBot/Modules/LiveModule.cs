using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using LZGChallenge2.Api.Services;
using System.Text;

namespace LZGChallenge2.DiscordBot.Modules;

public class LiveModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _services;

    public LiveModule(IServiceProvider services)
    {
        _services = services;
    }

    [Command("live")]
    [Alias("refresh", "update")]
    public async Task LiveAsync()
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var riotApiService = scope.ServiceProvider.GetRequiredService<IRiotApiService>();
        var matchUpdateService = scope.ServiceProvider.GetRequiredService<MatchUpdateService>();

        await Context.Channel.SendMessageAsync("🔄 Mise à jour en cours des données de tous les joueurs...");

        var players = await dbContext.Players.ToListAsync();
        var updatedPlayers = new List<string>();
        var errors = new List<string>();

        foreach (var player in players)
        {
            try
            {
                await Task.Delay(500);

                var leagueEntries = await riotApiService.GetLeagueEntriesByPuuidAsync(player.Puuid);
                var soloQueueEntry = leagueEntries.FirstOrDefault(entry => entry.QueueType == "RANKED_SOLO_5x5");

                if (soloQueueEntry != null)
                {
                    if (player.CurrentStats == null)
                    {
                        var stats = new PlayerStats
                        {
                            PlayerId = player.Id,
                            CurrentTier = soloQueueEntry.Tier,
                            CurrentRank = soloQueueEntry.Rank,
                            CurrentLeaguePoints = soloQueueEntry.LeaguePoints,
                            TotalGames = soloQueueEntry.Wins + soloQueueEntry.Losses,
                            TotalWins = soloQueueEntry.Wins,
                            TotalLosses = soloQueueEntry.Losses
                        };
                        dbContext.PlayerStats.Add(stats);
                        player.CurrentStats = stats;
                    }
                    else
                    {
                        var oldLP = player.CurrentStats.CurrentLeaguePoints;
                        var oldTier = player.CurrentStats.CurrentTier;
                        var oldRank = player.CurrentStats.CurrentRank;

                        player.CurrentStats.CurrentTier = soloQueueEntry.Tier;
                        player.CurrentStats.CurrentRank = soloQueueEntry.Rank;
                        player.CurrentStats.CurrentLeaguePoints = soloQueueEntry.LeaguePoints;
                        player.CurrentStats.LastUpdated = DateTime.UtcNow;

                        var lpChange = player.CurrentStats.CurrentLeaguePoints - oldLP;
                        var rankChanged = oldTier != player.CurrentStats.CurrentTier || oldRank != player.CurrentStats.CurrentRank;

                        if (lpChange != 0 || rankChanged)
                        {
                            var changeInfo = "";
                            if (rankChanged)
                            {
                                changeInfo = $" ({oldTier} {oldRank} → {player.CurrentStats.CurrentTier} {player.CurrentStats.CurrentRank})";
                            }
                            else if (lpChange != 0)
                            {
                                var sign = lpChange > 0 ? "+" : "";
                                changeInfo = $" ({sign}{lpChange} LP)";
                            }
                            updatedPlayers.Add($"{player.GameName}#{player.TagLine}{changeInfo}");
                        }
                    }
                }

                await matchUpdateService.UpdatePlayerMatchesAsync(player, 500);
                
                // Les stats par champion et rôle sont automatiquement calculées dans UpdatePlayerMatchesAsync

                // Mettre à jour les statistiques de jeu
                if (player.CurrentStats != null && soloQueueEntry != null)
                {
                    player.CurrentStats.TotalGames = soloQueueEntry.Wins + soloQueueEntry.Losses;
                    player.CurrentStats.TotalWins = soloQueueEntry.Wins;
                    player.CurrentStats.TotalLosses = soloQueueEntry.Losses;

                    if (player.CurrentStats.TotalGames == 0)
                    {
                        player.CurrentStats.TotalKills = 0;
                        player.CurrentStats.TotalDeaths = 0;
                        player.CurrentStats.TotalAssists = 0;
                        player.CurrentStats.CurrentWinStreak = 0;
                        player.CurrentStats.CurrentLoseStreak = 0;
                        player.CurrentStats.LongestWinStreak = 0;
                        player.CurrentStats.LongestLoseStreak = 0;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                errors.Add($"{player.GameName}#{player.TagLine}: {ex.Message}");
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("✅ **Mise à jour terminée !**");
        sb.AppendLine();

        if (updatedPlayers.Any())
        {
            sb.AppendLine("📊 **Changements détectés:**");
            foreach (var playerUpdate in updatedPlayers.Take(10))
            {
                sb.AppendLine($"• {playerUpdate}");
            }
            if (updatedPlayers.Count > 10)
            {
                sb.AppendLine($"... et {updatedPlayers.Count - 10} autres changements");
            }
        }
        else
        {
            sb.AppendLine("Aucun changement détecté depuis la dernière mise à jour.");
        }

        if (errors.Any())
        {
            sb.AppendLine();
            sb.AppendLine("⚠️ **Erreurs:**");
            foreach (var error in errors.Take(3))
            {
                sb.AppendLine($"• {error}");
            }
            if (errors.Count > 3)
            {
                sb.AppendLine($"... et {errors.Count - 3} autres erreurs");
            }
        }

        await ReplyAsync(sb.ToString());
    }
}