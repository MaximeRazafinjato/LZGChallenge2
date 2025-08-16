using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Models;
using System.Text;

namespace LZGChallenge2.DiscordBot.Services;

public class NotificationService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;
    private DiscordSocketClient? _client;

    public NotificationService(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _services = services;
        _configuration = configuration;
        _logger = logger;
    }

    public void SetDiscordClient(DiscordSocketClient client)
    {
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_client?.ConnectionState == ConnectionState.Connected)
                {
                    await CheckForAchievements();
                    await SendDailyRecap();
                }
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CheckForAchievements()
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var players = await dbContext.Players
            .Include(p => p.CurrentStats)
            .Where(p => p.CurrentStats != null)
            .ToListAsync();

        foreach (var player in players)
        {
            if (player.CurrentStats == null) continue;

            await CheckWinStreakAchievement(player);
            await CheckRankUpAchievement(player);
            await CheckKDAMilestone(player);
        }
    }

    private async Task CheckWinStreakAchievement(Player player)
    {
        if (player.CurrentStats?.CurrentWinStreak >= 5)
        {
            var channelId = _configuration.GetValue<ulong>("Discord:NotificationChannelId");
            if (channelId == 0) return;

            var channel = _client?.GetChannel(channelId) as ITextChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🔥 SÉRIE DE VICTOIRES !")
                .WithDescription($"**{player.GameName}#{player.TagLine}** est en feu avec **{player.CurrentStats.CurrentWinStreak}** victoires consécutives !")
                .WithColor(Color.Gold)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }

    private async Task CheckRankUpAchievement(Player player)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (player.CurrentStats?.NetLpChange > 0)
        {
            var channelId = _configuration.GetValue<ulong>("Discord:NotificationChannelId");
            if (channelId == 0) return;

            var channel = _client?.GetChannel(channelId) as ITextChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("📈 PROGRESSION DÉTECTÉE !")
                .WithDescription($"**{player.GameName}#{player.TagLine}** a une progression nette de **{player.CurrentStats.NetLpChange} LP** !\n" +
                               $"Rang actuel: **{player.CurrentStats.CurrentTier} {player.CurrentStats.CurrentRank}** ({player.CurrentStats.CurrentLeaguePoints} LP)")
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }

    private async Task CheckKDAMilestone(Player player)
    {
        if (player.CurrentStats?.KDA >= 3.0 && player.CurrentStats.TotalGames >= 20)
        {
            var channelId = _configuration.GetValue<ulong>("Discord:NotificationChannelId");
            if (channelId == 0) return;

            var channel = _client?.GetChannel(channelId) as ITextChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🎯 EXCELLENT KDA !")
                .WithDescription($"**{player.GameName}#{player.TagLine}** maintient un KDA impressionnant de **{player.CurrentStats.KDA:F2}** sur {player.CurrentStats.TotalGames} parties !")
                .WithColor(Color.Purple)
                .WithCurrentTimestamp()
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }

    private async Task SendDailyRecap()
    {
        if (DateTime.Now.Hour != 20 || DateTime.Now.Minute != 0) return;

        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var channelId = _configuration.GetValue<ulong>("Discord:DailyRecapChannelId");
        if (channelId == 0) return;

        var channel = _client?.GetChannel(channelId) as ITextChannel;
        if (channel == null) return;

        var players = await dbContext.Players
            .Include(p => p.CurrentStats)
            .Where(p => p.CurrentStats != null)
            .OrderByDescending(p => p.CurrentStats.NetLpChange)
            .ToListAsync();

        if (!players.Any()) return;

        var sb = new StringBuilder();
        sb.AppendLine("📊 **RÉCAPITULATIF DES PERFORMANCES** 📊");
        sb.AppendLine($"*{DateTime.Today:dddd dd MMMM yyyy}*");
        sb.AppendLine();

        var topPerformer = players.First();
        var worstPerformer = players.Last();

        sb.AppendLine("🏆 **Meilleure progression:**");
        sb.AppendLine($"**{topPerformer.GameName}#{topPerformer.TagLine}** " +
                     $"({topPerformer.CurrentStats!.TotalWins}W/{topPerformer.CurrentStats.TotalLosses}L, {topPerformer.CurrentStats.NetLpChange:+#;-#;0} LP)");
        sb.AppendLine();

        if (worstPerformer.CurrentStats!.NetLpChange < 0)
        {
            sb.AppendLine("💀 **Plus grosse chute:**");
            sb.AppendLine($"**{worstPerformer.GameName}#{worstPerformer.TagLine}** " +
                         $"({worstPerformer.CurrentStats.TotalWins}W/{worstPerformer.CurrentStats.TotalLosses}L, {worstPerformer.CurrentStats.NetLpChange} LP)");
            sb.AppendLine();
        }

        sb.AppendLine("📈 **Résumé des progressions:**");
        foreach (var player in players.Take(5))
        {
            var lpChange = player.CurrentStats!.NetLpChange;
            var emoji = lpChange > 0 ? "📈" : lpChange < 0 ? "📉" : "➖";
            sb.AppendLine($"{emoji} {player.GameName}: {player.CurrentStats.TotalWins}W/{player.CurrentStats.TotalLosses}L ({lpChange:+#;-#;0} LP)");
        }

        var embed = new EmbedBuilder()
            .WithTitle("SoloQ Challenge - Récapitulatif quotidien")
            .WithDescription(sb.ToString())
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .Build();

        await channel.SendMessageAsync(embed: embed);
    }
}