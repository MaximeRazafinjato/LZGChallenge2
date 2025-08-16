using System.Text.Json.Serialization;

namespace LZGChallenge2.Api.DTOs.RiotApi;

public class AccountDto
{
    [JsonPropertyName("puuid")]
    public string Puuid { get; set; } = null!;
    
    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = null!;
    
    [JsonPropertyName("tagLine")]
    public string TagLine { get; set; } = null!;
}