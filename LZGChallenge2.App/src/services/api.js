import axios from 'axios';

const API_BASE_URL = 'https://localhost:44393/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Players API
export const playersApi = {
  getAll: () => api.get('/players'),
  getById: (id) => api.get(`/players/${id}`),
  create: (playerData) => api.post('/players', playerData),
  remove: (id) => api.delete(`/players/${id}`),
};

// Leaderboard API
export const leaderboardApi = {
  get: (sortBy = 'lp') => api.get(`/leaderboard?sortBy=${sortBy}`),
  getSummary: () => api.get('/leaderboard/summary'),
};

// Champion Stats API
export const championStatsApi = {
  getByPlayerId: (playerId) => api.get(`/championstats/${playerId}`),
  getTopChampions: (playerId, limit = 3) => api.get(`/championstats/${playerId}/top/${limit}`),
  getFiltered: (playerId, filters = {}) => {
    const params = new URLSearchParams();
    if (filters.sortBy) params.append('sortBy', filters.sortBy);
    if (filters.role) params.append('role', filters.role);
    if (filters.search) params.append('search', filters.search);
    if (filters.limit) params.append('limit', filters.limit);
    
    return api.get(`/championstats/${playerId}/filtered?${params.toString()}`);
  },
  getByPlayerName: (gameName, tagLine) => api.get(`/championstats/player/${gameName}/${tagLine}`),
};

// Matches API - SoloQ avec détection automatique de saison
export const matchesApi = {
  getByPlayerId: (playerId, limit = 50) => api.get(`/matches/${playerId}?limit=${limit}`),
  getRecent: (playerId, limit = 10) => api.get(`/matches/${playerId}/recent/${limit}`),
  getFiltered: (playerId, filters = {}) => {
    const params = new URLSearchParams();
    if (filters.period) params.append('period', filters.period);
    if (filters.result) params.append('result', filters.result);
    if (filters.champion) params.append('champion', filters.champion);
    if (filters.position) params.append('position', filters.position);
    if (filters.limit) params.append('limit', filters.limit);
    if (filters.offset) params.append('offset', filters.offset);
    
    return api.get(`/matches/${playerId}/filtered?${params.toString()}`);
  },
  getStats: (playerId) => api.get(`/matches/${playerId}/stats`),
  getSeasonInfo: () => api.get('/matches/season-info'),
};

export default api;