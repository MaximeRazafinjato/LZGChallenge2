namespace LZGChallenge2.Api.DTOs;

public class MatchHistoryDto
{
    public string MatchId { get; set; } = null!;
    public DateTime GameStartTime { get; set; }
    public long GameDuration { get; set; } // en secondes
    public bool Win { get; set; }
    public string ChampionName { get; set; } = null!;
    public int ChampionId { get; set; }
    public string Position { get; set; } = null!;
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int CreepScore { get; set; }
    public long TotalDamageDealtToChampions { get; set; }
    public long GoldEarned { get; set; }
    public int VisionScore { get; set; }
    public int Level { get; set; }
    public int? LpChange { get; set; }
    public string? TierAtTime { get; set; }
    public string? RankAtTime { get; set; }
    public int Season { get; set; } = 2025;
    public int QueueId { get; set; } = 420; // Toujours SoloQ
    public double KDA { get; set; }
    public string FormattedGameDuration { get; set; } = null!;
    public string ResultText { get; set; } = null!;
    public string KDAText { get; set; } = null!;
}

public class MatchFilterDto
{
    public string? Period { get; set; } // "7days", "30days", "season", "all"
    public string? Result { get; set; } // "all", "wins", "losses"
    public string? Champion { get; set; } // Nom du champion ou "all"
    public string? Position { get; set; } // "all", "TOP", "JUNGLE", etc.
    public int Limit { get; set; } = 20;
    public int Offset { get; set; } = 0;
}

public class MatchStatsDto
{
    public int TotalGames { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
    public double AverageKDA { get; set; }
    public int TotalLpChange { get; set; }
    public string? StartRank { get; set; }
    public string? CurrentRank { get; set; }
    public DateTime? FirstGameDate { get; set; }
    public DateTime? LastGameDate { get; set; }
}