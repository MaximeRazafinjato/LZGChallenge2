using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LZGChallenge2.DiscordBot.Services;

public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<DiscordBotService> logger)
    {
        _services = services;
        _configuration = configuration;
        _logger = logger;

        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | 
                           GatewayIntents.GuildMessages | 
                           GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Info
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleCommandAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        var token = _configuration["Discord:BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Discord bot token is not configured");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task LogAsync(LogMessage log)
    {
        var severity = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(severity, log.Exception, "[{Source}] {Message}", log.Source, log.Message);
    }

    private Task ReadyAsync()
    {
        _logger.LogInformation("Discord bot {Username} is connected and ready!", _client.CurrentUser.Username);
        
        var notificationService = _services.GetService<NotificationService>();
        notificationService?.SetDiscordClient(_client);
        
        return Task.CompletedTask;
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        _logger.LogInformation("Message reçu: {Content} de {Author}", message.Content, message.Author.Username);

        int argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
            return;

        _logger.LogInformation("Commande détectée: {Command}", message.Content);

        var context = new SocketCommandContext(_client, message);
        var result = await _commands.ExecuteAsync(context, argPos, _services);
        
        if (!result.IsSuccess)
        {
            _logger.LogError("Erreur d'exécution de commande: {Error}", result.ErrorReason);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.LogoutAsync();
        await _client.StopAsync();
        _client.Dispose();
        await base.StopAsync(cancellationToken);
    }
}