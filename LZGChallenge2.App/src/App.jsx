import { useState, useEffect } from 'react'
import { ThemeProvider } from '@mui/material/styles'
import { CssBaseline, Container, Box, Fab } from '@mui/material'
import { PersonAdd } from '@mui/icons-material'
import theme from './theme'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import Header from './components/Header'
import AddPlayerForm from './components/AddPlayerForm'
import Leaderboard from './components/Leaderboard'
import PlayersGrid from './components/PlayersGrid'
import StatsCards from './components/StatsCards'

function App() {
  const [players, setPlayers] = useState([])
  const [leaderboard, setLeaderboard] = useState([])
  const [summary, setSummary] = useState(null)
  const [connection, setConnection] = useState(null)
  const [loading, setLoading] = useState(false)
  const [addPlayerModalOpen, setAddPlayerModalOpen] = useState(false)

  useEffect(() => {
    // Établir la connexion SignalR
    const newConnection = new HubConnectionBuilder()
      .withUrl('https://localhost:44393/leaderboardHub')
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
            loadData()
          })
          
          connection.on('PlayerRemoved', (playerId) => {
            setPlayers(prev => prev.filter(p => p.id !== playerId))
            loadData()
          })
          
          connection.on('StatsUpdated', (updatedPlayer) => {
            setPlayers(prev => prev.map(p => p.id === updatedPlayer.id ? updatedPlayer : p))
            loadData()
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
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #010A13 0%, #0F2027 50%, #203A43 100%)',
        }}
      >
        <Header />
        
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
      </Box>
    </ThemeProvider>
  )
}

export default App
