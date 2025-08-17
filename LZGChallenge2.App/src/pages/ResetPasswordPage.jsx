import { useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
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
  LinearProgress
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Lock as LockIcon,
  Save as SaveIcon,
  ArrowBack as BackIcon
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

// Schéma de validation
const resetPasswordSchema = yup.object().shape({
  password: yup
    .string()
    .required('Le mot de passe est requis')
    .min(8, 'Le mot de passe doit contenir au moins 8 caractères')
    .matches(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\.\-_#])/,
      'Le mot de passe doit contenir au moins une minuscule, une majuscule, un chiffre et un caractère spécial'
    ),
  confirmPassword: yup
    .string()
    .required('La confirmation est requise')
    .oneOf([yup.ref('password')], 'Les mots de passe ne correspondent pas')
});

const ResetPasswordPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { resetPassword } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);

  const token = searchParams.get('token');

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors }
  } = useForm({
    resolver: yupResolver(resetPasswordSchema)
  });

  const password = watch('password', '');

  // Calcul de la force du mot de passe
  const getPasswordStrength = (password) => {
    let strength = 0;
    if (password.length >= 8) strength += 25;
    if (/[a-z]/.test(password)) strength += 25;
    if (/[A-Z]/.test(password)) strength += 25;
    if (/\d/.test(password)) strength += 12.5;
    if (/[@$!%*?&]/.test(password)) strength += 12.5;
    return Math.min(strength, 100);
  };

  const getPasswordStrengthColor = (strength) => {
    if (strength < 30) return 'error';
    if (strength < 60) return 'warning';
    if (strength < 90) return 'info';
    return 'success';
  };

  const getPasswordStrengthText = (strength) => {
    if (strength < 30) return 'Faible';
    if (strength < 60) return 'Moyen';
    if (strength < 90) return 'Fort';
    return 'Très fort';
  };

  const onSubmit = async (data) => {
    if (!token) {
      setMessage('Token de réinitialisation manquant');
      return;
    }

    setIsLoading(true);
    setMessage('');
    
    try {
      const result = await resetPassword(token, data.password);
      
      if (result.success) {
        setIsSuccess(true);
        setMessage('Votre mot de passe a été réinitialisé avec succès !');
        
        // Rediriger vers la page de connexion après 3 secondes
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        setMessage(result.error || 'Une erreur est survenue lors de la réinitialisation');
      }
    } catch (error) {
      setMessage('Une erreur inattendue est survenue');
    } finally {
      setIsLoading(false);
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const toggleConfirmPasswordVisibility = () => {
    setShowConfirmPassword(!showConfirmPassword);
  };

  // Vérifier si le token est présent
  if (!token) {
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
            maxWidth: 450,
            width: '100%',
            borderRadius: 2,
            boxShadow: '0 8px 32px rgba(0,0,0,0.3)'
          }}
        >
          <CardContent sx={{ p: 4, textAlign: 'center' }}>
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
              🔐 LZG Challenge
            </Typography>
            
            <Alert severity="error" sx={{ mb: 3 }}>
              Token de réinitialisation manquant ou invalide
            </Alert>
            
            <Typography variant="body1" sx={{ mb: 3 }}>
              Le lien de réinitialisation est peut-être expiré ou incorrect.
            </Typography>
            
            <Box display="flex" justifyContent="center" gap={2}>
              <Button
                variant="outlined"
                component={Link}
                to="/forgot-password"
              >
                Demander un nouveau lien
              </Button>
              <Button
                variant="contained"
                component={Link}
                to="/login"
                sx={{
                  background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                  color: '#1E2328',
                  fontWeight: 'bold'
                }}
              >
                Retour à la connexion
              </Button>
            </Box>
          </CardContent>
        </Card>
      </Box>
    );
  }

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
          maxWidth: 450,
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
              🔐 LZG Challenge
            </Typography>
            <Typography variant="h6" color="text.secondary">
              Nouveau mot de passe
            </Typography>
          </Box>

          {isSuccess ? (
            // Message de succès
            <Box textAlign="center">
              <Alert severity="success" sx={{ mb: 3 }}>
                {message}
              </Alert>
              
              <Typography variant="body1" sx={{ mb: 3 }}>
                Vous pouvez maintenant vous connecter avec votre nouveau mot de passe.
              </Typography>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Vous serez automatiquement redirigé vers la page de connexion dans quelques secondes...
              </Typography>
              
              <Button
                variant="contained"
                component={Link}
                to="/login"
                sx={{
                  background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                  color: '#1E2328',
                  fontWeight: 'bold'
                }}
              >
                Se connecter maintenant
              </Button>
            </Box>
          ) : (
            // Formulaire de réinitialisation
            <>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3, textAlign: 'center' }}>
                Choisissez un nouveau mot de passe sécurisé pour votre compte.
              </Typography>

              {/* Message d'erreur */}
              {message && !isSuccess && (
                <Alert severity="error" sx={{ mb: 3 }}>
                  {message}
                </Alert>
              )}

              {/* Formulaire */}
              <Box component="form" onSubmit={handleSubmit(onSubmit)}>
                {/* Nouveau mot de passe */}
                <TextField
                  {...register('password')}
                  fullWidth
                  label="Nouveau mot de passe"
                  type={showPassword ? 'text' : 'password'}
                  error={!!errors.password}
                  helperText={errors.password?.message}
                  sx={{ mb: 1 }}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <LockIcon color="action" />
                      </InputAdornment>
                    ),
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

                {/* Indicateur de force du mot de passe */}
                {password && (
                  <Box mb={2}>
                    <Box display="flex" justifyContent="space-between" alignItems="center" mb={1}>
                      <Typography variant="caption" color="text.secondary">
                        Force du mot de passe
                      </Typography>
                      <Typography 
                        variant="caption" 
                        color={`${getPasswordStrengthColor(getPasswordStrength(password))}.main`}
                        fontWeight="bold"
                      >
                        {getPasswordStrengthText(getPasswordStrength(password))}
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={getPasswordStrength(password)}
                      color={getPasswordStrengthColor(getPasswordStrength(password))}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                )}

                {/* Confirmation mot de passe */}
                <TextField
                  {...register('confirmPassword')}
                  fullWidth
                  label="Confirmer le mot de passe"
                  type={showConfirmPassword ? 'text' : 'password'}
                  error={!!errors.confirmPassword}
                  helperText={errors.confirmPassword?.message}
                  sx={{ mb: 3 }}
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          onClick={toggleConfirmPasswordVisibility}
                          edge="end"
                        >
                          {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    )
                  }}
                />

                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  size="large"
                  disabled={isLoading}
                  startIcon={<SaveIcon />}
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
                  {isLoading ? 'Réinitialisation...' : 'Réinitialiser le mot de passe'}
                </Button>
              </Box>
            </>
          )}

          {/* Lien retour */}
          <Box textAlign="center">
            <MuiLink
              component={Link}
              to="/login"
              color="primary"
              underline="hover"
              sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 0.5 }}
            >
              <BackIcon fontSize="small" />
              Retour à la connexion
            </MuiLink>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ResetPasswordPage;