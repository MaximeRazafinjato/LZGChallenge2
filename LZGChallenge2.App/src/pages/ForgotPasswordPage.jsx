import { useState } from 'react';
import { Link } from 'react-router-dom';
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
  InputAdornment
} from '@mui/material';
import {
  Email as EmailIcon,
  ArrowBack as BackIcon,
  Send as SendIcon
} from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

// Schéma de validation
const forgotPasswordSchema = yup.object().shape({
  email: yup
    .string()
    .required('L\'email est requis')
    .email('Format d\'email invalide')
});

const ForgotPasswordPage = () => {
  const { forgotPassword } = useAuth();
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState('');
  const [isSuccess, setIsSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm({
    resolver: yupResolver(forgotPasswordSchema)
  });

  const onSubmit = async (data) => {
    setIsLoading(true);
    setMessage('');
    
    try {
      const result = await forgotPassword(data.email);
      
      if (result.success) {
        setIsSuccess(true);
        setMessage('Un email de réinitialisation a été envoyé à votre adresse email.');
      } else {
        setMessage(result.error || 'Une erreur est survenue');
      }
    } catch (error) {
      setMessage('Une erreur inattendue est survenue');
    } finally {
      setIsLoading(false);
    }
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
              Mot de passe oublié
            </Typography>
          </Box>

          {isSuccess ? (
            // Message de succès
            <Box textAlign="center">
              <Alert severity="success" sx={{ mb: 3 }}>
                {message}
              </Alert>
              
              <Typography variant="body1" sx={{ mb: 3 }}>
                Vérifiez votre boîte de réception et cliquez sur le lien dans l'email 
                pour réinitialiser votre mot de passe.
              </Typography>
              
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Si vous ne recevez pas l'email dans les prochaines minutes, 
                vérifiez votre dossier spam.
              </Typography>
              
              <Button
                variant="contained"
                component={Link}
                to="/login"
                startIcon={<BackIcon />}
                sx={{
                  background: 'linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%)',
                  color: '#1E2328',
                  fontWeight: 'bold'
                }}
              >
                Retour à la connexion
              </Button>
            </Box>
          ) : (
            // Formulaire de demande
            <>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3, textAlign: 'center' }}>
                Entrez votre adresse email et nous vous enverrons un lien pour 
                réinitialiser votre mot de passe.
              </Typography>

              {/* Message d'erreur */}
              {message && !isSuccess && (
                <Alert severity="error" sx={{ mb: 3 }}>
                  {message}
                </Alert>
              )}

              {/* Formulaire */}
              <Box component="form" onSubmit={handleSubmit(onSubmit)}>
                <TextField
                  {...register('email')}
                  fullWidth
                  label="Adresse email"
                  type="email"
                  error={!!errors.email}
                  helperText={errors.email?.message}
                  sx={{ mb: 3 }}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <EmailIcon color="action" />
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
                  startIcon={<SendIcon />}
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
                  {isLoading ? 'Envoi en cours...' : 'Envoyer le lien de réinitialisation'}
                </Button>
              </Box>
            </>
          )}

          {/* Liens de navigation */}
          <Box textAlign="center" display="flex" justifyContent="space-between">
            <MuiLink
              component={Link}
              to="/login"
              color="primary"
              underline="hover"
              sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
            >
              <BackIcon fontSize="small" />
              Retour à la connexion
            </MuiLink>
            
            <MuiLink
              component={Link}
              to="/register"
              color="primary"
              underline="hover"
            >
              Créer un compte
            </MuiLink>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ForgotPasswordPage;