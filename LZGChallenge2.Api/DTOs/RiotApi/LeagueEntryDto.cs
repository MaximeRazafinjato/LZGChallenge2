using System.Text.Json.Serialization;

namespace LZGChallenge2.Api.DTOs.RiotApi;

public class LeagueEntryDto
{
    [JsonPropertyName("leagueId")]
    public string LeagueId { get; set; } = null!;
    
    [JsonPropertyName("summonerId")]
    public string SummonerId { get; set; } = null!;
    
    [JsonPropertyName("queueType")]
    public string QueueType { get; set; } = null!;
    
    [JsonPropertyName("tier")]
    public string Tier { get; set; } = null!;
    
    [JsonPropertyName("rank")]
    public string Rank { get; set; } = null!;
    
    [JsonPropertyName("leaguePoints")]
    public int LeaguePoints { get; set; }
    
    [JsonPropertyName("wins")]
    public int Wins { get; set; }
    
    [JsonPropertyName("losses")]
    public int Losses { get; set; }
    
    [JsonPropertyName("hotStreak")]
    public bool HotStreak { get; set; }
    
    [JsonPropertyName("veteran")]
    public bool Veteran { get; set; }
    
    [JsonPropertyName("freshBlood")]
    public bool FreshBlood { get; set; }
    
    [JsonPropertyName("inactive")]
    public bool Inactive { get; set; }
}