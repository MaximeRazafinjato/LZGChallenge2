using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using LZGChallenge2.Api.DTOs.RiotApi;
using LZGChallenge2.Api.Options;

namespace LZGChallenge2.Api.Services;

public interface IRiotApiService
{
    Task<AccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine);
    Task<SummonerDto?> GetSummonerByPuuidAsync(string puuid);
    Task<List<LeagueEntryDto>> GetLeagueEntriesByPuuidAsync(string puuid);
    Task<List<string>> GetMatchIdsByPuuidAsync(string puuid, int count = 20, int start = 0);
    Task<MatchDto?> GetMatchByIdAsync(string matchId);
}

public class RiotApiService : IRiotApiService
{
    private readonly HttpClient _httpClient;
    private readonly RiotApiOptions _options;
    private readonly RateLimitService _rateLimitService;
    private readonly ILogger<RiotApiService> _logger;
    
    public RiotApiService(
        HttpClient httpClient,
        IOptions<RiotApiOptions> options,
        RateLimitService rateLimitService,
        ILogger<RiotApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _rateLimitService = rateLimitService;
        _logger = logger;
        
        _httpClient.DefaultRequestHeaders.Add("X-Riot-Token", _options.ApiKey);
    }
    
    public async Task<AccountDto?> GetAccountByRiotIdAsync(string gameName, string tagLine)
    {
        await _rateLimitService.WaitForRateLimitAsync();
        
        try
        {
            var url = $"{_options.RegionalUrl}/riot/account/v1/accounts/by-riot-id/{Uri.EscapeDataString(gameName)}/{Uri.EscapeDataString(tagLine)}";
            _logger.LogInformation("Calling Riot API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var account = await response.Content.ReadFromJsonAsync<AccountDto>();
                _logger.LogInformation("Successfully retrieved account for {GameName}#{TagLine}", gameName, tagLine);
                return account;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Account not found for {GameName}#{TagLine}", gameName, tagLine);
                return null;
            }
            
            _logger.LogError("Failed to get account for {GameName}#{TagLine}. Status: {StatusCode}, Content: {Content}",
                gameName, tagLine, response.StatusCode, await response.Content.ReadAsStringAsync());
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting account for {GameName}#{TagLine}", gameName, tagLine);
            return null;
        }
    }
    
    public async Task<SummonerDto?> GetSummonerByPuuidAsync(string puuid)
    {
        await _rateLimitService.WaitForRateLimitAsync();
        
        try
        {
            var url = $"{_options.BaseUrl}/lol/summoner/v4/summoners/by-puuid/{puuid}";
            _logger.LogInformation("Calling Riot API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var summoner = await response.Content.ReadFromJsonAsync<SummonerDto>();
                _logger.LogInformation("Successfully retrieved summoner for puuid {Puuid}. Level: {Level}", 
                    puuid, summoner?.SummonerLevel);
                return summoner;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get summoner for puuid {Puuid}. Status: {StatusCode}, Content: {Content}",
                puuid, response.StatusCode, errorContent);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting summoner for puuid {Puuid}", puuid);
            return null;
        }
    }
    
    public async Task<List<LeagueEntryDto>> GetLeagueEntriesByPuuidAsync(string puuid)
    {
        await _rateLimitService.WaitForRateLimitAsync();
        
        try
        {
            // D'abord récupérer le summoner pour avoir le summonerId
            var summoner = await GetSummonerByPuuidAsync(puuid);
            if (summoner == null)
            {
                _logger.LogError("Cannot get league entries: summoner not found for puuid {Puuid}", puuid);
                return new List<LeagueEntryDto>();
            }
            
            // Utiliser le PUUID comme summonerId (car c'est ce qu'on stocke maintenant)
            var url = $"{_options.BaseUrl}/lol/league/v4/entries/by-puuid/{puuid}";
            _logger.LogInformation("Calling Riot API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var entries = await response.Content.ReadFromJsonAsync<List<LeagueEntryDto>>() ?? new List<LeagueEntryDto>();
                _logger.LogInformation("Successfully retrieved {Count} league entries for puuid {Puuid}",
                    entries.Count, puuid);
                return entries;
            }
            
            _logger.LogError("Failed to get league entries for puuid {Puuid}. Status: {StatusCode}",
                puuid, response.StatusCode);
            
            return new List<LeagueEntryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting league entries for puuid {Puuid}", puuid);
            return new List<LeagueEntryDto>();
        }
    }
    
    public async Task<List<string>> GetMatchIdsByPuuidAsync(string puuid, int count = 20, int start = 0)
    {
        await _rateLimitService.WaitForRateLimitAsync();
        
        try
        {
            var url = $"{_options.RegionalUrl}/lol/match/v5/matches/by-puuid/{puuid}/ids?queue=420&count={count}&start={start}";
            _logger.LogInformation("Calling Riot API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var matchIds = await response.Content.ReadFromJsonAsync<List<string>>() ?? new List<string>();
                _logger.LogInformation("Successfully retrieved {Count} match IDs for puuid {Puuid}",
                    matchIds.Count, puuid);
                return matchIds;
            }
            
            _logger.LogError("Failed to get match IDs for puuid {Puuid}. Status: {StatusCode}",
                puuid, response.StatusCode);
            
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting match IDs for puuid {Puuid}", puuid);
            return new List<string>();
        }
    }
    
    public async Task<MatchDto?> GetMatchByIdAsync(string matchId)
    {
        await _rateLimitService.WaitForRateLimitAsync();
        
        try
        {
            var url = $"{_options.RegionalUrl}/lol/match/v5/matches/{matchId}";
            _logger.LogInformation("Calling Riot API: {Url}", url);
            
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var match = await response.Content.ReadFromJsonAsync<MatchDto>(options);
                _logger.LogInformation("Successfully retrieved match {MatchId}", matchId);
                return match;
            }
            
            _logger.LogError("Failed to get match {MatchId}. Status: {StatusCode}",
                matchId, response.StatusCode);
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting match {MatchId}", matchId);
            return null;
        }
    }
}