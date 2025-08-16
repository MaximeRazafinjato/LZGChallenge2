using System.Text.Json.Serialization;

namespace LZGChallenge2.Api.DTOs.RiotApi;

public class SummonerDto
{
    [JsonPropertyName("profileIconId")]
    public int ProfileIconId { get; set; }
    
    [JsonPropertyName("revisionDate")]
    public long RevisionDate { get; set; }
    
    [JsonPropertyName("puuid")]
    public string Puuid { get; set; } = null!;
    
    [JsonPropertyName("summonerLevel")]
    public long SummonerLevel { get; set; }
}