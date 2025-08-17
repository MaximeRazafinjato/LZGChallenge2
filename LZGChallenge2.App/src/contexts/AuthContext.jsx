import { createContext, useContext, useReducer, useEffect } from 'react';
import { authApi } from '../services/authApi';
import { toast } from 'react-hot-toast';

const AuthContext = createContext(null);

// États d'authentification
const authReducer = (state, action) => {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    case 'LOGIN_SUCCESS':
      return {
        ...state,
        isAuthenticated: true,
        user: action.payload.user,
        accessToken: action.payload.accessToken,
        isLoading: false,
        error: null
      };
    case 'LOGOUT':
      return {
        ...state,
        isAuthenticated: false,
        user: null,
        accessToken: null,
        isLoading: false,
        error: null
      };
    case 'SET_ERROR':
      return {
        ...state,
        error: action.payload,
        isLoading: false
      };
    case 'CLEAR_ERROR':
      return {
        ...state,
        error: null
      };
    case 'UPDATE_USER':
      return {
        ...state,
        user: action.payload
      };
    default:
      return state;
  }
};

const initialState = {
  isAuthenticated: false,
  user: null,
  accessToken: null,
  isLoading: true,
  error: null
};

export const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);

  // Vérifier l'authentification au chargement
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        dispatch({ type: 'SET_LOADING', payload: true });
        
        // Vérifier s'il y a un cookie de refresh token avant de tenter le refresh
        const hasCookie = document.cookie.includes('refreshToken=');
        
        if (hasCookie) {
          // Tenter de rafraîchir le token uniquement s'il y a un cookie
          const result = await authApi.refresh();
          
          if (result.success) {
            dispatch({
              type: 'LOGIN_SUCCESS',
              payload: {
                user: result.data.user,
                accessToken: result.data.accessToken
              }
            });
          } else {
            dispatch({ type: 'LOGOUT' });
          }
        } else {
          // Pas de cookie de refresh token, l'utilisateur n'est pas connecté
          dispatch({ type: 'LOGOUT' });
        }
      } catch (error) {
        console.error('Erreur d\'initialisation de l\'authentification:', error);
        dispatch({ type: 'LOGOUT' });
      }
    };

    initializeAuth();
  }, []);

  // Actions d'authentification
  const login = async (email, password) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'CLEAR_ERROR' });

      const result = await authApi.login(email, password);

      if (result.success) {
        dispatch({
          type: 'LOGIN_SUCCESS',
          payload: {
            user: result.data.user,
            accessToken: result.data.accessToken
          }
        });
        toast.success('Connexion réussie !');
        return { success: true };
      } else {
        const errorMessage = result.data?.errors?.[0] || result.data?.message || 'Erreur de connexion';
        dispatch({ type: 'SET_ERROR', payload: errorMessage });
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue lors de la connexion';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const register = async (userData) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'CLEAR_ERROR' });

      const result = await authApi.register(userData);

      if (result.success) {
        dispatch({ type: 'SET_LOADING', payload: false });
        toast.success(result.data.message || 'Inscription réussie ! Vérifiez votre email.');
        return { success: true, message: result.data.message };
      } else {
        const errorMessage = result.data?.errors?.[0] || result.data?.message || 'Erreur d\'inscription';
        dispatch({ type: 'SET_ERROR', payload: errorMessage });
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue lors de l\'inscription';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } catch (error) {
      console.error('Erreur lors de la déconnexion:', error);
    } finally {
      dispatch({ type: 'LOGOUT' });
      toast.success('Déconnexion réussie');
    }
  };

  const verifyEmail = async (token) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      
      const result = await authApi.verifyEmail(token);
      
      if (result.success) {
        dispatch({ type: 'SET_LOADING', payload: false });
        toast.success(result.data.message || 'Email vérifié avec succès !');
        return { success: true, message: result.data.message };
      } else {
        const errorMessage = result.data?.message || 'Erreur de vérification';
        dispatch({ type: 'SET_ERROR', payload: errorMessage });
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue lors de la vérification';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const resendVerification = async (email) => {
    try {
      const result = await authApi.resendVerification(email);
      
      if (result.success) {
        toast.success(result.data.message || 'Email de vérification renvoyé');
        return { success: true };
      } else {
        const errorMessage = result.data?.message || 'Erreur lors du renvoi';
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue';
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const forgotPassword = async (email) => {
    try {
      const result = await authApi.forgotPassword(email);
      
      if (result.success) {
        toast.success(result.data.message || 'Email de réinitialisation envoyé');
        return { success: true };
      } else {
        const errorMessage = result.data?.message || 'Erreur lors de l\'envoi';
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue';
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const resetPassword = async (token, password) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      
      const result = await authApi.resetPassword(token, password);
      
      if (result.success) {
        dispatch({ type: 'SET_LOADING', payload: false });
        toast.success(result.data.message || 'Mot de passe réinitialisé avec succès !');
        return { success: true };
      } else {
        const errorMessage = result.data?.errors?.[0] || result.data?.message || 'Erreur de réinitialisation';
        dispatch({ type: 'SET_ERROR', payload: errorMessage });
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue lors de la réinitialisation';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const changePassword = async (currentPassword, newPassword) => {
    try {
      const result = await authApi.changePassword(currentPassword, newPassword);
      
      if (result.success) {
        toast.success(result.data.message || 'Mot de passe modifié avec succès !');
        return { success: true };
      } else {
        const errorMessage = result.data?.errors?.[0] || result.data?.message || 'Erreur lors du changement';
        toast.error(errorMessage);
        return { success: false, error: errorMessage };
      }
    } catch (error) {
      const errorMessage = 'Une erreur est survenue';
      toast.error(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const refreshToken = async () => {
    try {
      const result = await authApi.refresh();
      
      if (result.success) {
        dispatch({
          type: 'LOGIN_SUCCESS',
          payload: {
            user: result.data.user,
            accessToken: result.data.accessToken
          }
        });
        return result.data.accessToken;
      } else {
        dispatch({ type: 'LOGOUT' });
        return null;
      }
    } catch (error) {
      dispatch({ type: 'LOGOUT' });
      return null;
    }
  };

  const value = {
    // État
    ...state,
    
    // Actions
    login,
    register,
    logout,
    verifyEmail,
    resendVerification,
    forgotPassword,
    resetPassword,
    changePassword,
    refreshToken,
    
    // Utilitaires
    clearError: () => dispatch({ type: 'CLEAR_ERROR' })
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth doit être utilisé dans un AuthProvider');
  }
  return context;
};