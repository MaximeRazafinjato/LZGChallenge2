import axios from 'axios';

const API_BASE_URL = 'https://localhost:44393/api';

// Instance Axios avec configuration de base
const authAxios = axios.create({
  baseURL: `${API_BASE_URL}/auth`,
  withCredentials: true, // Pour envoyer les cookies automatiquement
  headers: {
    'Content-Type': 'application/json'
  }
});

// Intercepteur pour ajouter le token d'accès
authAxios.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Intercepteur pour gérer le renouvellement de token
authAxios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Éviter la boucle infinie pour les appels refresh et login
    const isRefreshCall = originalRequest.url?.includes('/refresh');
    const isLoginCall = originalRequest.url?.includes('/login');
    
    if (error.response?.status === 401 && !originalRequest._retry && !isRefreshCall && !isLoginCall) {
      originalRequest._retry = true;

      // Vérifier s'il y a un cookie de refresh token
      const hasCookie = document.cookie.includes('refreshToken=');
      
      if (hasCookie) {
        try {
          const refreshResult = await authApi.refresh();
          if (refreshResult.success) {
            setAccessToken(refreshResult.data.accessToken);
            originalRequest.headers.Authorization = `Bearer ${refreshResult.data.accessToken}`;
            return authAxios(originalRequest);
          }
        } catch (refreshError) {
          // Le refresh a échoué, rediriger vers la page de connexion
          removeAccessToken();
          window.location.href = '/login';
          return Promise.reject(refreshError);
        }
      } else {
        // Pas de cookie, rediriger vers la page de connexion
        removeAccessToken();
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

// Gestion du token d'accès en mémoire (plus sécurisé)
let accessToken = null;

const setAccessToken = (token) => {
  accessToken = token;
};

const getAccessToken = () => {
  return accessToken;
};

const removeAccessToken = () => {
  accessToken = null;
};

// API d'authentification
export const authApi = {
  // Inscription
  register: async (userData) => {
    try {
      const response = await authAxios.post('/register', userData);
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Connexion
  login: async (email, password) => {
    try {
      const response = await authAxios.post('/login', { email, password });
      
      if (response.data.accessToken) {
        setAccessToken(response.data.accessToken);
      }
      
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Rafraîchissement de token
  refresh: async () => {
    try {
      const response = await authAxios.post('/refresh');
      
      if (response.data.accessToken) {
        setAccessToken(response.data.accessToken);
      }
      
      return { success: true, data: response.data };
    } catch (error) {
      removeAccessToken();
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Déconnexion
  logout: async () => {
    try {
      await authAxios.post('/logout');
    } catch (error) {
      console.error('Erreur lors de la déconnexion:', error);
    } finally {
      removeAccessToken();
    }
  },

  // Vérification d'email
  verifyEmail: async (token) => {
    try {
      const response = await authAxios.post('/verify-email', { token });
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Renvoi d'email de vérification
  resendVerification: async (email) => {
    try {
      const response = await authAxios.post('/resend-verification', { email });
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Mot de passe oublié
  forgotPassword: async (email) => {
    try {
      const response = await authAxios.post('/forgot-password', { email });
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Réinitialisation de mot de passe
  resetPassword: async (token, password) => {
    try {
      const response = await authAxios.post('/reset-password', { token, password });
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Changement de mot de passe
  changePassword: async (currentPassword, newPassword) => {
    try {
      const response = await authAxios.post('/change-password', {
        currentPassword,
        newPassword
      });
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Récupérer le profil utilisateur
  getCurrentUser: async () => {
    try {
      const response = await authAxios.get('/me');
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  },

  // Révoquer tous les tokens
  revokeAllTokens: async () => {
    try {
      const response = await authAxios.post('/revoke-all-tokens');
      removeAccessToken();
      return { success: true, data: response.data };
    } catch (error) {
      return {
        success: false,
        data: error.response?.data || { message: 'Erreur réseau' }
      };
    }
  }
};

// Export des utilitaires de token pour usage externe
export { setAccessToken, getAccessToken, removeAccessToken };