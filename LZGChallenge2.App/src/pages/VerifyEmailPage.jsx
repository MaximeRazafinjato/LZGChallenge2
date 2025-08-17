import { useEffect, useState, useRef } from 'react';
import { useSearchParams, Link, useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Alert,
  Button,
  CircularProgress
} from '@mui/material';
import {
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
  Email as EmailIcon
} from '@mui/icons-material';
import { authApi } from '../services/authApi';

const VerifyEmailPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('loading'); // 'loading', 'success', 'error'
  const [message, setMessage] = useState('');
  const hasVerified = useRef(false);

  useEffect(() => {
    const token = searchParams.get('token');

    if (!token) {
      setStatus('error');
      setMessage('Token de vérification manquant');
      return;
    }

    // Éviter les appels multiples
    if (hasVerified.current) {
      return;
    }

    const handleVerification = async () => {
      hasVerified.current = true;
      try {
        const result = await authApi.verifyEmail(token);
        
        if (result.success) {
          setStatus('success');
          setMessage(result.data.message || 'Email vérifié avec succès !');
          
          // Rediriger vers la page de connexion après 3 secondes
          setTimeout(() => {
            navigate('/login');
          }, 3000);
        } else {
          setStatus('error');
          setMessage(result.data?.message || 'Erreur lors de la vérification');
        }
      } catch (error) {
        setStatus('error');
        setMessage('Une erreur inattendue est survenue');
      }
    };

    handleVerification();
  }, [searchParams, navigate]);

  const renderContent = () => {
    switch (status) {
      case 'loading':
        return (
          <Box textAlign="center">
            <CircularProgress size={60} sx={{ mb: 3, color: '#C89B3C' }} />
            <Typography variant="h6" gutterBottom>
              Vérification en cours...
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Veuillez patienter pendant que nous vérifions votre email.
            </Typography>
          </Box>
        );

      case 'success':
        return (
          <Box textAlign="center">
            <SuccessIcon 
              sx={{ 
                fontSize: 80, 
                color: 'success.main', 
                mb: 2 
              }} 
            />
            <Typography variant="h5" gutterBottom color="success.main">
              Email vérifié avec succès !
            </Typography>
            <Alert severity="success" sx={{ mb: 3 }}>
              {message}
            </Alert>
            <Typography variant="body1" sx={{ mb: 3 }}>
              Votre compte est maintenant activé. Vous pouvez vous connecter.
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
        );

      case 'error':
        return (
          <Box textAlign="center">
            <ErrorIcon 
              sx={{ 
                fontSize: 80, 
                color: 'error.main', 
                mb: 2 
              }} 
            />
            <Typography variant="h5" gutterBottom color="error.main">
              Erreur de vérification
            </Typography>
            <Alert severity="error" sx={{ mb: 3 }}>
              {message}
            </Alert>
            <Typography variant="body1" sx={{ mb: 3 }}>
              Le lien de vérification est peut-être expiré ou invalide.
            </Typography>
            <Box display="flex" justifyContent="center" gap={2}>
              <Button
                variant="outlined"
                component={Link}
                to="/resend-verification"
                startIcon={<EmailIcon />}
              >
                Renvoyer l'email
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
          </Box>
        );

      default:
        return null;
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
          maxWidth: 500,
          width: '100%',
          borderRadius: 2,
          boxShadow: '0 8px 32px rgba(0,0,0,0.3)'
        }}
      >
        <CardContent sx={{ p: 4 }}>
          {/* En-tête */}
          <Box textAlign="center" mb={4}>
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
              Vérification d'email
            </Typography>
          </Box>

          {/* Contenu dynamique */}
          {renderContent()}
        </CardContent>
      </Card>
    </Box>
  );
};

export default VerifyEmailPage;