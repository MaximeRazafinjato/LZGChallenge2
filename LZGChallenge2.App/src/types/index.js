// Types pour l'application
export const PlayerStats = {
  currentTier: '',
  currentRank: '',
  currentLeaguePoints: 0,
  totalGames: 0,
  totalWins: 0,
  totalLosses: 0,
  winRate: 0,
  averageKills: 0,
  averageDeaths: 0,
  averageAssists: 0,
  kda: 0,
  averageCreepScore: 0,
  averageVisionScore: 0,
  averageDamageDealt: 0,
  currentWinStreak: 0,
  currentLoseStreak: 0,
  longestWinStreak: 0,
  longestLoseStreak: 0,
  netLpChange: 0,
  lastUpdated: new Date()
};

export const Player = {
  id: 0,
  riotId: '',
  gameName: '',
  tagLine: '',
  region: '',
  joinedAt: new Date(),
  isActive: true,
  currentStats: null
};

export const LeaderboardEntry = {
  playerId: 0,
  gameName: '',
  tagLine: '',
  currentTier: '',
  currentRank: '',
  currentLeaguePoints: 0,
  winRate: 0,
  kda: 0,
  totalGames: 0,
  netLpChange: 0,
  currentWinStreak: 0,
  currentLoseStreak: 0,
  lastUpdated: new Date()
};