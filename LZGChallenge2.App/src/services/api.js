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

export default api;