namespace LZGChallenge2.Api.Options;

public class RiotApiOptions
{
    public const string SectionName = "RiotApi";
    
    public string ApiKey { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string RegionalUrl { get; set; } = null!;
    public int RateLimitPerSecond { get; set; } = 20;
    public int RateLimitPer2Minutes { get; set; } = 100;
}