import {
  Card,
  CardContent,
  Typography,
  Box,
  Grid,
  Avatar,
  IconButton,
  Chip,
  Tooltip,
  Skeleton
} from '@mui/material'
import { 
  Person,
  Delete,
  Schedule,
  SportsEsports,
  TrendingUp,
  TrendingDown
} from '@mui/icons-material'
import { motion } from 'framer-motion'

const PlayerCard = ({ player, onRemove, index }) => {
  const formatJoinDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    })
  }

  const formatRank = (tier, rank, lp) => {
    if (!tier || tier === 'UNRANKED') return 'Non classé'
    return `${tier} ${rank} (${lp} LP)`
  }

  const getTierColor = (tier) => {
    const colors = {
      'IRON': '#CD7F32',
      'BRONZE': '#CD7F32', 
      'SILVER': '#C0C0C0',
      'GOLD': '#FFD700',
      'PLATINUM': '#E5E4E2',
      'EMERALD': '#50C878',
      'DIAMOND': '#B9F2FF',
      'MASTER': '#9966CC',
      'GRANDMASTER': '#FF6B6B',
      'CHALLENGER': '#F7DC6F'
    }
    return colors[tier] || '#606060'
  }

  return (
    <motion.div
      initial={{ y: 20, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.4, delay: index * 0.1 }}
      whileHover={{ y: -5, transition: { duration: 0.2 } }}
    >
      <Card
        sx={{
          height: '100%',
          position: 'relative',
          transition: 'all 0.3s ease',
          '&:hover': {
            boxShadow: '0 8px 32px rgba(200,155,60,0.3)',
            transform: 'translateY(-4px)',
          }
        }}
      >
        <CardContent sx={{ p: 3, height: '100%', display: 'flex', flexDirection: 'column' }}>
          {/* Header avec avatar et actions */}
          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flex: 1 }}>
              <Avatar
                sx={{
                  width: 48,
                  height: 48,
                  bgcolor: player.currentStats?.currentTier 
                    ? getTierColor(player.currentStats.currentTier)
                    : 'primary.main',
                  color: '#1E2328',
                  fontWeight: 700,
                  fontSize: '1.2rem'
                }}
              >
                {player.gameName?.[0]?.toUpperCase()}
              </Avatar>
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography 
                  variant="h4" 
                  sx={{ 
                    fontWeight: 600,
                    fontSize: '1.1rem',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    whiteSpace: 'nowrap'
                  }}
                >
                  {player.gameName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  #{player.tagLine}
                </Typography>
              </Box>
            </Box>
            
            <Tooltip title="Retirer du challenge">
              <IconButton
                onClick={() => onRemove(player.id)}
                size="small"
                sx={{
                  color: 'error.main',
                  '&:hover': {
                    backgroundColor: 'rgba(231,76,60,0.1)',
                  }
                }}
              >
                <Delete fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>

          {/* Région */}
          <Box sx={{ mb: 2 }}>
            <Chip
              label={player.region}
              size="small"
              sx={{
                backgroundColor: 'rgba(200,155,60,0.2)',
                color: 'primary.main',
                fontWeight: 600
              }}
            />
          </Box>

          {/* Date d'inscription */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
            <Schedule sx={{ fontSize: 16, color: 'text.secondary' }} />
            <Typography variant="body2" color="text.secondary">
              Rejoint le {formatJoinDate(player.joinedAt)}
            </Typography>
          </Box>

          {/* Stats actuelles */}
          {player.currentStats ? (
            <Box sx={{ mt: 'auto', pt: 2, borderTop: '1px solid', borderColor: 'divider' }}>
              {/* Rang */}
              <Box sx={{ mb: 1.5 }}>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                  Rang actuel
                </Typography>
                <Chip
                  label={formatRank(
                    player.currentStats.currentTier,
                    player.currentStats.currentRank,
                    player.currentStats.currentLeaguePoints
                  )}
                  sx={{
                    backgroundColor: getTierColor(player.currentStats.currentTier) + '30',
                    color: getTierColor(player.currentStats.currentTier),
                    border: `1px solid ${getTierColor(player.currentStats.currentTier)}50`,
                    fontWeight: 600,
                    fontSize: '0.75rem'
                  }}
                />
              </Box>

              {/* Stats de jeu */}
              <Grid container spacing={1}>
                <Grid item xs={6}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="body2" color="text.secondary">
                      Parties
                    </Typography>
                    <Typography variant="h4" sx={{ fontWeight: 600, fontSize: '1rem' }}>
                      {player.currentStats.totalGames}
                    </Typography>
                  </Box>
                </Grid>
                
                <Grid item xs={6}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="body2" color="text.secondary">
                      Winrate
                    </Typography>
                    <Typography 
                      variant="h4" 
                      sx={{ 
                        fontWeight: 600,
                        fontSize: '1rem',
                        color: player.currentStats.winRate >= 60 ? '#50C878' : 
                               player.currentStats.winRate >= 50 ? '#F39C12' : '#E74C3C'
                      }}
                    >
                      {player.currentStats.winRate?.toFixed(1)}%
                    </Typography>
                  </Box>
                </Grid>
              </Grid>

              {/* KDA */}
              <Box sx={{ mt: 1.5, textAlign: 'center' }}>
                <Typography variant="body2" color="text.secondary">
                  KDA Moyen
                </Typography>
                <Typography 
                  variant="h4" 
                  sx={{ 
                    fontWeight: 600,
                    fontSize: '1rem',
                    color: player.currentStats.kda >= 2 ? '#50C878' : 
                           player.currentStats.kda >= 1 ? '#F39C12' : '#E74C3C'
                  }}
                >
                  {player.currentStats.kda?.toFixed(2)}
                </Typography>
              </Box>

              {/* Progression LP */}
              {player.currentStats.netLpChange !== 0 && (
                <Box sx={{ mt: 1.5, display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
                  {player.currentStats.netLpChange > 0 ? (
                    <TrendingUp sx={{ fontSize: 16, color: '#50C878' }} />
                  ) : (
                    <TrendingDown sx={{ fontSize: 16, color: '#E74C3C' }} />
                  )}
                  <Typography 
                    variant="body2" 
                    sx={{ 
                      fontWeight: 600,
                      color: player.currentStats.netLpChange > 0 ? '#50C878' : '#E74C3C'
                    }}
                  >
                    {player.currentStats.netLpChange > 0 ? '+' : ''}{player.currentStats.netLpChange} LP
                  </Typography>
                </Box>
              )}
            </Box>
          ) : (
            <Box sx={{ mt: 'auto', pt: 2, borderTop: '1px solid', borderColor: 'divider', textAlign: 'center' }}>
              <SportsEsports sx={{ fontSize: 32, color: 'text.secondary', mb: 1 }} />
              <Typography variant="body2" color="text.secondary">
                Pas encore de statistiques
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Lancez une partie pour commencer !
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>
    </motion.div>
  )
}

const PlayersGrid = ({ players = [], onRemovePlayer, loading }) => {
  if (loading) {
    return (
      <Box>
        <Typography variant="h3" sx={{ fontWeight: 600, mb: 3 }}>
          Participants ({players.length})
        </Typography>
        <Grid container spacing={3}>
          {[...Array(6)].map((_, i) => (
            <Grid item xs={12} sm={6} md={4} key={i}>
              <Card>
                <CardContent sx={{ p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                    <Skeleton variant="circular" width={48} height={48} />
                    <Box sx={{ flex: 1 }}>
                      <Skeleton variant="text" width="60%" height={24} />
                      <Skeleton variant="text" width="40%" height={20} />
                    </Box>
                  </Box>
                  <Skeleton variant="rectangular" height={100} />
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Box>
    )
  }

  return (
    <motion.div
      initial={{ y: 30, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.6, delay: 0.6 }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Box
          sx={{
            p: 1.5,
            borderRadius: 2,
            background: 'rgba(0,245,255,0.2)',
            border: '1px solid rgba(0,245,255,0.3)',
          }}
        >
          <Person sx={{ color: '#00F5FF', fontSize: 28 }} />
        </Box>
        <Typography variant="h3" sx={{ fontWeight: 600 }}>
          Participants ({players.filter(p => p.isActive).length})
        </Typography>
      </Box>

      {players.length === 0 ? (
        <Card>
          <CardContent sx={{ p: 6, textAlign: 'center' }}>
            <Person sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h4" sx={{ mb: 1 }}>
              Aucun participant
            </Typography>
            <Typography color="text.secondary">
              Ajoutez des joueurs pour commencer le challenge !
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={3}>
          {players
            .filter(player => player.isActive)
            .map((player, index) => (
              <Grid item xs={12} sm={6} md={4} key={player.id}>
                <PlayerCard 
                  player={player} 
                  onRemove={onRemovePlayer}
                  index={index}
                />
              </Grid>
            ))}
        </Grid>
      )}
    </motion.div>
  )
}

export default PlayersGrid