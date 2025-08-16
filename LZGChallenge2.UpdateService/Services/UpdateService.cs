using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LZGChallenge2.UpdateService.Data;
using LZGChallenge2.UpdateService.Options;
using MongoDB.Driver;

namespace LZGChallenge2.UpdateService.Services;

public interface IUpdateService
{
    Task UpdateAllPlayersAsync();
}

public class UpdateService : IUpdateService
{
    private readonly MongoDbContext _context;
    private readonly RiotApiOptions _riotApiOptions;
    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdateService> _logger;

    public UpdateService(
        MongoDbContext context,
        IOptions<RiotApiOptions> riotApiOptions,
        HttpClient httpClient,
        ILogger<UpdateService> logger)
    {
        _context = context;
        _riotApiOptions = riotApiOptions.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task UpdateAllPlayersAsync()
    {
        try
        {
            _logger.LogInformation("Début de la mise à jour automatique de tous les joueurs");

            var players = await _context.Players
                .Find(p => p.IsActive)
                .ToListAsync();

            if (!players.Any())
            {
                _logger.LogInformation("Aucun joueur actif trouvé pour la mise à jour");
                return;
            }

            _logger.LogInformation($"Mise à jour de {players.Count} joueurs actifs");

            var updateTasks = players.Select(async player =>
            {
                try
                {
                    await UpdatePlayerDataAsync(player.Id);
                    _logger.LogInformation($"Joueur {player.GameName}#{player.TagLine} mis à jour avec succès");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erreur lors de la mise à jour du joueur {player.GameName}#{player.TagLine}");
                }
            });

            await Task.WhenAll(updateTasks);

            _logger.LogInformation("Mise à jour automatique terminée");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour automatique des joueurs");
            throw;
        }
    }

    private async Task UpdatePlayerDataAsync(string playerId)
    {
        try
        {
            _logger.LogDebug($"Appel de l'API pour mise à jour du joueur {playerId}");

            // Tenter de se connecter à l'API avec timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            var response = await _httpClient.PostAsync($"https://localhost:44393/api/players/{playerId}/refresh", null, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug($"Mise à jour réussie pour le joueur {playerId}");
            }
            else
            {
                _logger.LogWarning($"Échec de la mise à jour pour le joueur {playerId}. Status: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("actively refused"))
        {
            _logger.LogError($"L'API LZG Challenge n'est pas démarrée (localhost:44393). Veuillez démarrer l'API avant d'exécuter ce service.");
            _logger.LogInformation("Pour démarrer l'API: cd LZGChallenge2.Api && dotnet run");
            throw new InvalidOperationException("API LZG Challenge non accessible", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Erreur réseau lors de la mise à jour du joueur {playerId}");
            throw;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError($"Timeout lors de la connexion à l'API pour le joueur {playerId}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur inattendue lors de la mise à jour du joueur {playerId}");
            throw;
        }
    }
}