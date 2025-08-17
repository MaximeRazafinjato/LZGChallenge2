import { useState, useEffect } from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider } from '@mui/material/styles'
import { CssBaseline, Container, Box, Fab, Snackbar, Alert, LinearProgress, Typography } from '@mui/material'
import { PersonAdd } from '@mui/icons-material'
import { Toaster } from 'react-hot-toast'
import theme from './theme'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { AuthProvider } from './contexts/AuthContext'
import ProtectedRoute from './components/auth/ProtectedRoute'
import LoginForm from './components/auth/LoginForm'
import RegisterForm from './components/auth/RegisterForm'
import VerifyEmailPage from './pages/VerifyEmailPage'
import ForgotPasswordPage from './pages/ForgotPasswordPage'
import ResetPasswordPage from './pages/ResetPasswordPage'
import Header from './components/Header'
import AddPlayerForm from './components/AddPlayerForm'
import Leaderboard from './components/Leaderboard'
import PlayersGrid from './components/PlayersGrid'
import StatsCards from './components/StatsCards'

// Composant principal de l'application (page d'accueil)
const MainApp = () => {
  const [players, setPlayers] = useState([])
  const [leaderboard, setLeaderboard] = useState([])
  const [summary, setSummary] = useState(null)
  const [connection, setConnection] = useState(null)
  const [loading, setLoading] = useState(false)
  const [addPlayerModalOpen, setAddPlayerModalOpen] = useState(false)
  const [updateInProgress, setUpdateInProgress] = useState(false)
  const [notification, setNotification] = useState({ open: false, message: '', severity: 'info' })

  useEffect(() => {
    // Établir la connexion SignalR
    const signalRUrl = import.meta.env.VITE_API_BASE_URL 
      ? import.meta.env.VITE_API_BASE_URL.replace('/api', '/leaderboardHub')
      : 'https://localhost:44393/leaderboardHub';
    
    const newConnection = new HubConnectionBuilder()
      .withUrl(signalRUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    setConnection(newConnection)
  }, [])

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('SignalR Connected!')
          
          // Rejoindre le groupe Leaderboard
          connection.invoke('JoinLeaderboardGroup')
          
          // Écouter les mises à jour
          connection.on('PlayerAdded', (player) => {
            setPlayers(prev => [...prev, player])
            setUpdateInProgress(true)
            setNotification({ 
              open: true, 
              message: `${player.gameName}#${player.tagLine} ajouté au challenge`, 
              severity: 'success' 
            })
            // Mise à jour ciblée : juste leaderboard et summary (pas les players)
            Promise.all([loadLeaderboard(), loadSummary()])
              .finally(() => setUpdateInProgress(false))
          })
          
          connection.on('PlayerRemoved', (playerId) => {
            const removedPlayer = players.find(p => p.id === playerId)
            setPlayers(prev => prev.filter(p => p.id !== playerId))
            setUpdateInProgress(true)
            setNotification({ 
              open: true, 
              message: `Participant supprimé du challenge`, 
              severity: 'info' 
            })
            // Mise à jour ciblée : juste leaderboard et summary (pas les players)
            Promise.all([loadLeaderboard(), loadSummary()])
              .finally(() => setUpdateInProgress(false))
          })
          
          connection.on('PlayerUpdated', (playerId) => {
            const updatedPlayer = players.find(p => p.id === playerId)
            setUpdateInProgress(true)
            setNotification({ 
              open: true, 
              message: `Données mises à jour${updatedPlayer ? ` pour ${updatedPlayer.gameName}` : ''}`, 
              severity: 'info' 
            })
            // Mise à jour ciblée : charger les nouvelles données du joueur spécifique
            updateSpecificPlayer(playerId)
            // Mettre à jour le leaderboard et summary
            Promise.all([loadLeaderboard(), loadSummary()])
              .finally(() => setUpdateInProgress(false))
          })
        })
        .catch(e => console.log('Connection failed: ', e))
    }

    return () => {
      if (connection) {
        connection.stop()
      }
    }
  }, [connection])

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    try {
      await Promise.all([
        loadPlayers(),
        loadLeaderboard(),
        loadSummary()
      ])
    } catch (error) {
      console.error('Erreur lors du chargement des données:', error)
    } finally {
      setLoading(false)
    }
  }

  const refreshAndLoadData = async () => {
    setLoading(true)
    try {
      // Actualiser les statistiques de tous les joueurs en une seule fois
      const response = await fetch('https://localhost:44393/api/players/refresh-all-ranks', {
        method: 'POST',
      })
      
      if (response.ok) {
        const result = await response.json()
        console.log('Tous les rangs actualisés avec succès:', result.message)
        
        // Charger toutes les données mises à jour
        await Promise.all([
          loadPlayers(),
          loadLeaderboard(),
          loadSummary()
        ])
      } else {
        console.error('Erreur lors de l\'actualisation des rangs')
      }
    } catch (error) {
      console.error('Erreur lors du refresh global:', error)
    } finally {
      setLoading(false)
    }
  }

  const loadPlayers = async () => {
    try {
      const response = await fetch('https://localhost:44393/api/players')
      if (response.ok) {
        const data = await response.json()
        setPlayers(data)
      }
    } catch (error) {
      console.error('Erreur lors du chargement des joueurs:', error)
    }
  }

  const loadLeaderboard = async (sortBy = 'lp') => {
    try {
      const response = await fetch(`https://localhost:44393/api/leaderboard?sortBy=${sortBy}`)
      if (response.ok) {
        const data = await response.json()
        setLeaderboard(data)
      }
    } catch (error) {
      console.error('Erreur lors du chargement du leaderboard:', error)
    }
  }

  const loadSummary = async () => {
    try {
      const response = await fetch('https://localhost:44393/api/leaderboard/summary')
      if (response.ok) {
        const data = await response.json()
        setSummary(data)
      }
    } catch (error) {
      console.error('Erreur lors du chargement du résumé:', error)
    }
  }

  const updateSpecificPlayer = async (playerId) => {
    try {
      const response = await fetch(`https://localhost:44393/api/players/${playerId}`)
      if (response.ok) {
        const updatedPlayer = await response.json()
        setPlayers(prev => prev.map(p => 
          p.id === playerId ? updatedPlayer : p
        ))
      }
    } catch (error) {
      console.error('Erreur lors de la mise à jour du joueur:', error)
    }
  }

  const removePlayer = async (playerId) => {
    try {
      const response = await fetch(`https://localhost:44393/api/players/${playerId}`, {
        method: 'DELETE',
      })

      if (response.ok) {
        // La mise à jour sera faite via SignalR
      }
    } catch (error) {
      console.error('Erreur lors de la suppression:', error)
    }
  }

  const refreshPlayerRank = async (playerId) => {
    try {
      const response = await fetch(`https://localhost:44393/api/players/${playerId}/refresh-rank`, {
        method: 'POST',
      })

      if (response.ok) {
        console.log('Rang actualisé avec succès')
        // La mise à jour sera faite via SignalR
      } else {
        console.error('Erreur lors de l\'actualisation du rang')
      }
    } catch (error) {
      console.error('Erreur lors de l\'actualisation du rang:', error)
    }
  }

  const refreshAllPlayersRank = async () => {
    try {
      // Récupérer la liste actuelle des joueurs
      const response = await fetch('https://localhost:44393/api/players')
      if (response.ok) {
        const currentPlayers = await response.json()
        
        // Actualiser le rang de tous les joueurs actifs
        const refreshPromises = currentPlayers
          .filter(player => player.isActive)
          .map(player => refreshPlayerRank(player.id))
        
        await Promise.all(refreshPromises)
        console.log('Tous les rangs actualisés avec succès')
      }
    } catch (error) {
      console.error('Erreur lors de l\'actualisation de tous les rangs:', error)
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #010A13 0%, #0F2027 50%, #203A43 100%)',
      }}
    >
      <Header />
      
      {/* Indicateur de mise à jour en cours */}
      {updateInProgress && (
        <Box sx={{ position: 'fixed', top: 0, left: 0, right: 0, zIndex: 1400 }}>
          <LinearProgress 
            sx={{ 
              backgroundColor: 'rgba(200,155,60,0.2)',
              '& .MuiLinearProgress-bar': {
                backgroundColor: '#C89B3C'
              }
            }} 
          />
        </Box>
      )}
      
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Stats Cards */}
        <StatsCards summary={summary} loading={loading} />
        
        {/* Add Player Modal */}
        <AddPlayerForm 
          open={addPlayerModalOpen}
          onClose={() => setAddPlayerModalOpen(false)}
          onPlayerAdded={loadData} 
        />
        
        {/* Leaderboard */}
        <Leaderboard 
          leaderboard={leaderboard} 
          loading={loading}
          onRefresh={refreshAndLoadData}
          onSortChange={loadLeaderboard}
        />
        
        {/* Players Grid */}
        <PlayersGrid 
          players={players} 
          onRemovePlayer={removePlayer}
          loading={loading}
        />
      </Container>

      {/* Floating Action Button pour ajouter un joueur */}
      <Fab
        color="primary"
        aria-label="Ajouter un joueur"
        onClick={() => setAddPlayerModalOpen(true)}
        sx={{
          position: 'fixed',
          bottom: 32,
          right: 32,
          background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
          color: '#1E2328',
          width: 64,
          height: 64,
          fontSize: 28,
          '&:hover': {
            background: 'linear-gradient(45deg, #B8860B 30%, #C89B3C 90%)',
            transform: 'scale(1.1)',
            boxShadow: '0 8px 25px rgba(200,155,60,0.4)',
          },
          transition: 'all 0.3s ease',
          zIndex: 1000
        }}
      >
        <PersonAdd sx={{ fontSize: 32 }} />
      </Fab>

      {/* Notifications toast */}
      <Snackbar
        open={notification.open}
        autoHideDuration={3000}
        onClose={() => setNotification(prev => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setNotification(prev => ({ ...prev, open: false }))}
          severity={notification.severity}
          variant="filled"
          sx={{
            backgroundColor: 
              notification.severity === 'success' ? '#50C878' :
              notification.severity === 'info' ? '#C89B3C' : '#E74C3C',
            '& .MuiAlert-icon': {
              color: '#1E2328'
            },
            '& .MuiAlert-message': {
              color: '#1E2328',
              fontWeight: 500
            }
          }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </Box>
  )
}

// Composant App principal avec routeur et authentification
function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <Router>
          <Routes>
            {/* Routes publiques d'authentification */}
            <Route path="/login" element={<LoginForm />} />
            <Route path="/register" element={<RegisterForm />} />
            <Route path="/verify-email" element={<VerifyEmailPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            
            {/* Routes protégées */}
            <Route 
              path="/" 
              element={
                <ProtectedRoute>
                  <MainApp />
                </ProtectedRoute>
              } 
            />
            
            {/* Page d'erreur pour les routes non autorisées */}
            <Route 
              path="/unauthorized" 
              element={
                <Box
                  display="flex"
                  justifyContent="center"
                  alignItems="center"
                  minHeight="100vh"
                  sx={{
                    background: 'linear-gradient(135deg, #1e2328 0%, #3c3c41 100%)',
                    textAlign: 'center'
                  }}
                >
                  <Typography variant="h4" color="error">
                    Accès non autorisé
                  </Typography>
                </Box>
              } 
            />
            
            {/* Redirection par défaut */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Router>
        
        {/* Toast notifications globales */}
        <Toaster
          position="top-right"
          toastOptions={{
            duration: 4000,
            style: {
              background: '#1E2328',
              color: '#F0E6D2',
              border: '1px solid #C89B3C'
            }
          }}
        />
      </AuthProvider>
    </ThemeProvider>
  )
}

export default App
