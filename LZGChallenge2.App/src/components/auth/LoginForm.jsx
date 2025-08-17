import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Link as MuiLink,
  Alert,
  InputAdornment,
  IconButton,
  Divider
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Login as LoginIcon,
  Email as EmailIcon
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';

// Schéma de validation
const loginSchema = yup.object().shape({
  email: yup
    .string()
    .required('L\'email est requis')
    .email('Format d\'email invalide'),
  password: yup
    .string()
    .required('Le mot de passe est requis')
    .min(6, 'Le mot de passe doit contenir au moins 6 caractères')
});

const LoginForm = () => {
  const { login, isLoading, error } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm({
    resolver: yupResolver(loginSchema)
  });

  const onSubmit = async (data) => {
    const result = await login(data.email, data.password);
    
    if (result.success) {
      // Rediriger vers la page d'origine ou vers l'accueil
      const from = location.state?.from?.pathname || '/';
      navigate(from, { replace: true });
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      sx={{
        background: 'linear-gradient(135deg, #1e2328 0%, #3c3c41 100%)',
        padding: 2
      }}
    >
      <Card
        sx={{
          maxWidth: 400,
          width: '100%',
          borderRadius: 2,
          boxShadow: '0 8px 32px rgba(0,0,0,0.3)'
        }}
      >
        <CardContent sx={{ p: 4 }}>
          {/* En-tête */}
          <Box textAlign="center" mb={3}>
            <Typography
              variant="h4"
              component="h1"
              gutterBottom
              sx={{
                fontWeight: 'bold',
                background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                color: 'transparent'
              }}
            >
              🏆 LZG Challenge
            </Typography>
            <Typography variant="h6" color="text.secondary">
              Connexion
            </Typography>
          </Box>

          {/* Erreur globale */}
          {error && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {error}
            </Alert>
          )}

          {/* Formulaire */}
          <Box component="form" onSubmit={handleSubmit(onSubmit)}>
            {/* Email */}
            <TextField
              {...register('email')}
              fullWidth
              label="Adresse email"
              type="email"
              error={!!errors.email}
              helperText={errors.email?.message}
              sx={{ mb: 2 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <EmailIcon color="action" />
                  </InputAdornment>
                )
              }}
            />

            {/* Mot de passe */}
            <TextField
              {...register('password')}
              fullWidth
              label="Mot de passe"
              type={showPassword ? 'text' : 'password'}
              error={!!errors.password}
              helperText={errors.password?.message}
              sx={{ mb: 3 }}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={togglePasswordVisibility}
                      edge="end"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                )
              }}
            />

            {/* Bouton de connexion */}
            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading}
              startIcon={<LoginIcon />}
              sx={{
                mb: 3,
                background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                color: '#1E2328',
                fontWeight: 'bold',
                '&:hover': {
                  background: 'linear-gradient(135deg, #A17A2A 0%, #D4C5A9 100%)'
                }
              }}
            >
              {isLoading ? 'Connexion...' : 'Se connecter'}
            </Button>

            <Divider sx={{ mb: 3 }}>
              <Typography variant="body2" color="text.secondary">
                ou
              </Typography>
            </Divider>

            {/* Liens utiles */}
            <Box textAlign="center" space={2}>
              <Typography variant="body2" sx={{ mb: 1 }}>
                <MuiLink
                  component={Link}
                  to="/forgot-password"
                  color="primary"
                  underline="hover"
                >
                  Mot de passe oublié ?
                </MuiLink>
              </Typography>
              
              <Typography variant="body2">
                Pas encore de compte ?{' '}
                <MuiLink
                  component={Link}
                  to="/register"
                  color="primary"
                  underline="hover"
                  fontWeight="bold"
                >
                  S'inscrire
                </MuiLink>
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default LoginForm;