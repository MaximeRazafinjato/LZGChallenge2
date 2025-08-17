import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
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
  Divider,
  LinearProgress
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  PersonAdd as RegisterIcon,
  Email as EmailIcon,
  Person as PersonIcon
} from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';

// Schéma de validation
const registerSchema = yup.object().shape({
  firstName: yup
    .string()
    .required('Le prénom est requis')
    .min(2, 'Le prénom doit contenir au moins 2 caractères')
    .max(50, 'Le prénom ne peut pas dépasser 50 caractères'),
  lastName: yup
    .string()
    .required('Le nom est requis')
    .min(2, 'Le nom doit contenir au moins 2 caractères')
    .max(50, 'Le nom ne peut pas dépasser 50 caractères'),
  email: yup
    .string()
    .required('L\'email est requis')
    .email('Format d\'email invalide'),
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

const RegisterForm = () => {
  const { register: registerUser, isLoading, error } = useAuth();
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [registrationSuccess, setRegistrationSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors }
  } = useForm({
    resolver: yupResolver(registerSchema)
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
    const result = await registerUser(data);
    
    if (result.success) {
      setRegistrationSuccess(true);
      // Ne pas rediriger automatiquement, laisser l'utilisateur lire le message
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const toggleConfirmPasswordVisibility = () => {
    setShowConfirmPassword(!showConfirmPassword);
  };

  // Affichage du message de succès
  if (registrationSuccess) {
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
            maxWidth: 500,
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
              🎉 Inscription réussie !
            </Typography>
            
            <Alert severity="success" sx={{ mb: 3 }}>
              Votre compte a été créé avec succès. Veuillez vérifier votre email pour activer votre compte.
            </Alert>
            
            <Typography variant="body1" sx={{ mb: 3 }}>
              Un email de vérification a été envoyé à votre adresse email. 
              Cliquez sur le lien dans l'email pour activer votre compte et pouvoir vous connecter.
            </Typography>
            
            <Button
              variant="contained"
              onClick={() => navigate('/login')}
              sx={{
                background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                color: '#1E2328',
                fontWeight: 'bold'
              }}
            >
              Aller à la page de connexion
            </Button>
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
          maxWidth: 500,
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
              Créer un compte
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
            {/* Prénom et Nom */}
            <Box display="flex" gap={2} mb={2}>
              <TextField
                {...register('firstName')}
                fullWidth
                label="Prénom"
                error={!!errors.firstName}
                helperText={errors.firstName?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  )
                }}
              />
              <TextField
                {...register('lastName')}
                fullWidth
                label="Nom"
                error={!!errors.lastName}
                helperText={errors.lastName?.message}
              />
            </Box>

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
              sx={{ mb: 1 }}
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

            {/* Bouton d'inscription */}
            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading}
              startIcon={<RegisterIcon />}
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
              {isLoading ? 'Inscription...' : 'S\'inscrire'}
            </Button>

            <Divider sx={{ mb: 3 }}>
              <Typography variant="body2" color="text.secondary">
                ou
              </Typography>
            </Divider>

            {/* Lien vers la connexion */}
            <Box textAlign="center">
              <Typography variant="body2">
                Vous avez déjà un compte ?{' '}
                <MuiLink
                  component={Link}
                  to="/login"
                  color="primary"
                  underline="hover"
                  fontWeight="bold"
                >
                  Se connecter
                </MuiLink>
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default RegisterForm;