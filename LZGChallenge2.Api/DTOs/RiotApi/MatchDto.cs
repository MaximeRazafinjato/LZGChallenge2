namespace LZGChallenge2.Api.DTOs.RiotApi;

public class MatchDto
{
    public MetadataDto Metadata { get; set; } = null!;
    public InfoDto Info { get; set; } = null!;
}

public class MetadataDto
{
    public string DataVersion { get; set; } = null!;
    public string MatchId { get; set; } = null!;
    public List<string> Participants { get; set; } = new();
}

public class InfoDto
{
    public long GameCreation { get; set; }
    public long GameDuration { get; set; }
    public long GameEndTimestamp { get; set; }
    public long GameId { get; set; }
    public string GameMode { get; set; } = null!;
    public string GameName { get; set; } = null!;
    public long GameStartTimestamp { get; set; }
    public string GameType { get; set; } = null!;
    public string GameVersion { get; set; } = null!;
    public int MapId { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
    public string PlatformId { get; set; } = null!;
    public int QueueId { get; set; }
    public List<TeamDto> Teams { get; set; } = new();
    public string TournamentCode { get; set; } = null!;
}

public class ParticipantDto
{
    public int AllInPings { get; set; }
    public int AssistMePings { get; set; }
    public int Assists { get; set; }
    public int BaronKills { get; set; }
    public int BountyLevel { get; set; }
    public int ChampExperience { get; set; }
    public int ChampLevel { get; set; }
    public int ChampionId { get; set; }
    public string ChampionName { get; set; } = null!;
    public int CommandPings { get; set; }
    public int ChampionTransform { get; set; }
    public int ConsumablesPurchased { get; set; }
    public ChallengesDto Challenges { get; set; } = null!;
    public int DamageDealtToBuildings { get; set; }
    public int DamageDealtToObjectives { get; set; }
    public int DamageDealtToTurrets { get; set; }
    public int DamageSelfMitigated { get; set; }
    public int Deaths { get; set; }
    public int DetectorWardsPlaced { get; set; }
    public int DoubleKills { get; set; }
    public int DragonKills { get; set; }
    public bool EligibleForProgression { get; set; }
    public int EnemyMissingPings { get; set; }
    public int EnemyVisionPings { get; set; }
    public bool FirstBloodAssist { get; set; }
    public bool FirstBloodKill { get; set; }
    public bool FirstTowerAssist { get; set; }
    public bool FirstTowerKill { get; set; }
    public bool GameEndedInEarlySurrender { get; set; }
    public bool GameEndedInSurrender { get; set; }
    public int GetBackPings { get; set; }
    public int GoldEarned { get; set; }
    public int GoldSpent { get; set; }
    public string IndividualPosition { get; set; } = null!;
    public int InhibitorKills { get; set; }
    public int InhibitorTakedowns { get; set; }
    public int InhibitorsLost { get; set; }
    public int Item0 { get; set; }
    public int Item1 { get; set; }
    public int Item2 { get; set; }
    public int Item3 { get; set; }
    public int Item4 { get; set; }
    public int Item5 { get; set; }
    public int Item6 { get; set; }
    public int ItemsPurchased { get; set; }
    public int KillingSprees { get; set; }
    public int Kills { get; set; }
    public string Lane { get; set; } = null!;
    public int LargestCriticalStrike { get; set; }
    public int LargestKillingSpree { get; set; }
    public int LargestMultiKill { get; set; }
    public int LongestTimeSpentLiving { get; set; }
    public int MagicDamageDealt { get; set; }
    public int MagicDamageDealtToChampions { get; set; }
    public int MagicDamageTaken { get; set; }
    public int NeutralMinionsKilled { get; set; }
    public int NexusKills { get; set; }
    public int NexusLost { get; set; }
    public int NexusTakedowns { get; set; }
    public int ObjectivesStolen { get; set; }
    public int ObjectivesStolenAssists { get; set; }
    public int ParticipantId { get; set; }
    public int PentaKills { get; set; }
    public PerksDto Perks { get; set; } = null!;
    public int PhysicalDamageDealt { get; set; }
    public int PhysicalDamageDealtToChampions { get; set; }
    public int PhysicalDamageTaken { get; set; }
    public int ProfileIcon { get; set; }
    public string Puuid { get; set; } = null!;
    public int QuadraKills { get; set; }
    public string RiotIdGameName { get; set; } = null!;
    public string RiotIdTagline { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int SightWardsBoughtInGame { get; set; }
    public int Spell1Casts { get; set; }
    public int Spell2Casts { get; set; }
    public int Spell3Casts { get; set; }
    public int Spell4Casts { get; set; }
    public int Summoner1Casts { get; set; }
    public int Summoner1Id { get; set; }
    public int Summoner2Casts { get; set; }
    public int Summoner2Id { get; set; }
    public string SummonerId { get; set; } = null!;
    public int SummonerLevel { get; set; }
    public string SummonerName { get; set; } = null!;
    public bool TeamEarlySurrendered { get; set; }
    public int TeamId { get; set; }
    public string TeamPosition { get; set; } = null!;
    public int TimeCCingOthers { get; set; }
    public int TimePlayed { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageDealtToChampions { get; set; }
    public int TotalDamageShieldedOnTeammates { get; set; }
    public int TotalDamageTaken { get; set; }
    public int TotalHeal { get; set; }
    public int TotalHealsOnTeammates { get; set; }
    public int TotalMinionsKilled { get; set; }
    public int TotalTimeCCDealt { get; set; }
    public int TotalTimeSpentDead { get; set; }
    public int TotalUnitsHealed { get; set; }
    public int TripleKills { get; set; }
    public int TrueDamageDealt { get; set; }
    public int TrueDamageDealtToChampions { get; set; }
    public int TrueDamageTaken { get; set; }
    public int TurretKills { get; set; }
    public int TurretTakedowns { get; set; }
    public int TurretsLost { get; set; }
    public int UnrealKills { get; set; }
    public int VisionScore { get; set; }
    public int VisionWardsBoughtInGame { get; set; }
    public int WardsKilled { get; set; }
    public int WardsPlaced { get; set; }
    public bool Win { get; set; }
}

public class ChallengesDto
{
    // Simplified - only including key challenges for now
    public double KDA { get; set; }
}

public class PerksDto
{
    public PerkStatsDto StatPerks { get; set; } = null!;
    public List<PerkStyleDto> Styles { get; set; } = new();
}

public class PerkStatsDto
{
    public int Defense { get; set; }
    public int Flex { get; set; }
    public int Offense { get; set; }
}

public class PerkStyleDto
{
    public string Description { get; set; } = null!;
    public List<PerkStyleSelectionDto> Selections { get; set; } = new();
    public int Style { get; set; }
}

public class PerkStyleSelectionDto
{
    public int Perk { get; set; }
    public int Var1 { get; set; }
    public int Var2 { get; set; }
    public int Var3 { get; set; }
}

public class TeamDto
{
    public List<BanDto> Bans { get; set; } = new();
    public ObjectivesDto Objectives { get; set; } = null!;
    public int TeamId { get; set; }
    public bool Win { get; set; }
}

public class BanDto
{
    public int ChampionId { get; set; }
    public int PickTurn { get; set; }
}

public class ObjectivesDto
{
    public ObjectiveDto Baron { get; set; } = null!;
    public ObjectiveDto Champion { get; set; } = null!;
    public ObjectiveDto Dragon { get; set; } = null!;
    public ObjectiveDto Inhibitor { get; set; } = null!;
    public ObjectiveDto RiftHerald { get; set; } = null!;
    public ObjectiveDto Tower { get; set; } = null!;
}

public class ObjectiveDto
{
    public bool First { get; set; }
    public int Kills { get; set; }
}