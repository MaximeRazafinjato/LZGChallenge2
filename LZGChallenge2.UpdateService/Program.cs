using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LZGChallenge2.UpdateService.Data;
using LZGChallenge2.UpdateService.Options;
using LZGChallenge2.UpdateService.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Configuration MongoDB
        var mongoConnectionString = configuration.GetConnectionString("MongoDB") ?? 
                                   configuration["MongoDB:ConnectionString"] ?? 
                                   Environment.GetEnvironmentVariable("MongoDB__ConnectionString");
        var mongoDatabaseName = configuration["MongoDB:DatabaseName"] ?? "LZGChallenge2Db";

        if (string.IsNullOrEmpty(mongoConnectionString))
        {
            throw new InvalidOperationException("La chaîne de connexion MongoDB est requise");
        }

        services.AddMongoDb(mongoConnectionString, mongoDatabaseName);

        // Configuration Riot API
        services.Configure<RiotApiOptions>(configuration.GetSection(RiotApiOptions.SectionName));

        // HttpClient pour les appels API
        services.AddHttpClient<IUpdateService, UpdateService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5); // Timeout généreux pour les mises à jour
        });

        // Services
        services.AddScoped<IUpdateService, UpdateService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

try
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("=== LZG Challenge Update Service - Démarrage ===");
    logger.LogInformation($"Heure de début: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

    using var scope = host.Services.CreateScope();
    var updateService = scope.ServiceProvider.GetRequiredService<IUpdateService>();

    await updateService.UpdateAllPlayersAsync();

    logger.LogInformation("=== LZG Challenge Update Service - Terminé ===");
    logger.LogInformation($"Heure de fin: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
}
catch (Exception ex)
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Erreur critique dans le service de mise à jour");
    Environment.ExitCode = 1;
}

await host.StopAsync();
