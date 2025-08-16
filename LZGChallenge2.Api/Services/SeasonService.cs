namespace LZGChallenge2.Api.Services;

public interface ISeasonService
{
    int GetCurrentSeason();
    int GetSeasonFromDate(DateTime date);
}

public class SeasonService : ISeasonService
{
    // La saison 14 a commencé le 10 janvier 2024
    // La saison 15 commencera probablement début janvier 2025
    private static readonly Dictionary<int, DateTime> SeasonStartDates = new()
    {
        { 14, new DateTime(2024, 1, 10) },
        { 15, new DateTime(2025, 1, 8) }, // Date estimée
        { 16, new DateTime(2026, 1, 8) }, // Date estimée
    };
    
    public int GetCurrentSeason()
    {
        return GetSeasonFromDate(DateTime.UtcNow);
    }
    
    public int GetSeasonFromDate(DateTime date)
    {
        // Trouver la saison basée sur la date
        var currentSeason = SeasonStartDates
            .Where(kvp => date >= kvp.Value)
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault();
            
        return currentSeason.Key != 0 ? currentSeason.Key : 14; // Par défaut saison 14
    }
}