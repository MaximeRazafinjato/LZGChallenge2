using System.Collections.Concurrent;

namespace LZGChallenge2.Api.Services;

public class RateLimitService
{
    private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly int _maxRequestsPerSecond;
    private readonly int _maxRequestsPer2Minutes;
    
    public RateLimitService(int maxRequestsPerSecond = 20, int maxRequestsPer2Minutes = 100)
    {
        _maxRequestsPerSecond = maxRequestsPerSecond;
        _maxRequestsPer2Minutes = maxRequestsPer2Minutes;
    }
    
    public async Task WaitForRateLimitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            var now = DateTime.UtcNow;
            var oneSecondAgo = now.AddSeconds(-1);
            var twoMinutesAgo = now.AddMinutes(-2);
            
            // Nettoyer les anciens timestamps
            while (_requestTimestamps.TryPeek(out var timestamp) && timestamp < twoMinutesAgo)
            {
                _requestTimestamps.TryDequeue(out _);
            }
            
            // Compter les requêtes dans la dernière seconde
            var requestsLastSecond = _requestTimestamps.Count(t => t > oneSecondAgo);
            
            // Compter les requêtes dans les 2 dernières minutes
            var requestsLast2Minutes = _requestTimestamps.Count;
            
            // Calculer le délai nécessaire
            var delayMs = 0;
            
            if (requestsLastSecond >= _maxRequestsPerSecond)
            {
                delayMs = Math.Max(delayMs, 1000);
            }
            
            if (requestsLast2Minutes >= _maxRequestsPer2Minutes)
            {
                delayMs = Math.Max(delayMs, 2000); // Attendre 2 secondes si on a atteint la limite de 2 minutes
            }
            
            if (delayMs > 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            
            // Ajouter le timestamp de cette requête
            _requestTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}