namespace LZGChallenge2.Api.DTOs;

public class PlayerDto
{
    public string Id { get; set; } = null!;
    public string RiotId { get; set; } = null!;
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;
    public string Region { get; set; } = null!;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public PlayerStatsDto? CurrentStats { get; set; }
}

public class CreatePlayerDto
{
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;
    public string Region { get; set; } = "EUW1";
}

public class PlayerStatsDto
{
    public string? CurrentTier { get; set; }
    public string? CurrentRank { get; set; }
    public int CurrentLeaguePoints { get; set; }
    public int TotalGames { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public double WinRate { get; set; }
    public double AverageKills { get; set; }
    public double AverageDeaths { get; set; }
    public double AverageAssists { get; set; }
    public double KDA { get; set; }
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int LongestWinStreak { get; set; }
    public int LongestLoseStreak { get; set; }
    public int NetLpChange { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class LeaderboardEntryDto
{
    public string PlayerId { get; set; } = null!;
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;
    public string? CurrentTier { get; set; }
    public string? CurrentRank { get; set; }
    public int CurrentLeaguePoints { get; set; }
    public double WinRate { get; set; }
    public double KDA { get; set; }
    public int TotalGames { get; set; }
    public int NetLpChange { get; set; }
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int LongestWinStreak { get; set; }
    public int LongestLoseStreak { get; set; }
    public double AverageKills { get; set; }
    public double AverageDeaths { get; set; }
    public double AverageAssists { get; set; }
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class CompactLeaderboardEntryDto
{
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;
    public string? CurrentTier { get; set; }
    public string? CurrentRank { get; set; }
    public int CurrentLeaguePoints { get; set; }
    public double WinRate { get; set; }
    public int TotalGames { get; set; }
}

public class UpdatePlayerDto
{
    public bool IsActive { get; set; }
}

public class LeaderboardEntryWithChampionsDto
{
    public string PlayerId { get; set; } = null!;
    public string GameName { get; set; } = null!;
    public string TagLine { get; set; } = null!;
    public string? CurrentTier { get; set; }
    public string? CurrentRank { get; set; }
    public int CurrentLeaguePoints { get; set; }
    public double WinRate { get; set; }
    public double KDA { get; set; }
    public int TotalGames { get; set; }
    public int NetLpChange { get; set; }
    public int CurrentWinStreak { get; set; }
    public int CurrentLoseStreak { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int LongestWinStreak { get; set; }
    public int LongestLoseStreak { get; set; }
    public double AverageKills { get; set; }
    public double AverageDeaths { get; set; }
    public double AverageAssists { get; set; }
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<TopChampionDto> TopChampions { get; set; } = new();
}

public class TopChampionDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
    public double KDA { get; set; }
}

public class ChampionStatsFilteredDto
{
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
    public double KDA { get; set; }
    public double AverageKills { get; set; }
    public double AverageDeaths { get; set; }
    public double AverageAssists { get; set; }
    public double AverageCreepScore { get; set; }
    public double AverageVisionScore { get; set; }
    public double AverageDamageDealt { get; set; }
    public string? PrimaryRole { get; set; }
}