import { 
  Grid, 
  Card, 
  CardContent, 
  Typography, 
  Box,
  Skeleton,
  Chip
} from '@mui/material'
import { 
  People,
  SportsEsports,
  TrendingUp,
  EmojiEvents
} from '@mui/icons-material'
import { motion } from 'framer-motion'

const StatCard = ({ icon, title, value, subtitle, color, loading, delay = 0 }) => {
  return (
    <motion.div
      initial={{ y: 30, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.5, delay }}
    >
      <Card
        sx={{
          height: '100%',
          background: 'linear-gradient(145deg, rgba(30,35,40,0.9) 0%, rgba(60,60,65,0.8) 100%)',
          border: `1px solid ${color}40`,
          borderRadius: 3,
          transition: 'all 0.3s ease',
          '&:hover': {
            transform: 'translateY(-4px)',
            boxShadow: `0 8px 32px ${color}30`,
            border: `1px solid ${color}60`,
          }
        }}
      >
        <CardContent sx={{ p: 3 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
            <Box
              sx={{
                p: 1.5,
                borderRadius: 2,
                background: `${color}20`,
                border: `1px solid ${color}40`,
              }}
            >
              {icon}
            </Box>
            <Typography variant="h4" sx={{ fontWeight: 500 }}>
              {title}
            </Typography>
          </Box>
          
          {loading ? (
            <Box>
              <Skeleton variant="text" width={80} height={40} />
              <Skeleton variant="text" width={120} height={20} />
            </Box>
          ) : (
            <Box>
              <Typography 
                variant="h2" 
                sx={{ 
                  fontWeight: 700,
                  color: color,
                  mb: 0.5
                }}
              >
                {value}
              </Typography>
              {subtitle && (
                <Typography variant="body2" color="text.secondary">
                  {subtitle}
                </Typography>
              )}
            </Box>
          )}
        </CardContent>
      </Card>
    </motion.div>
  )
}

const StatsCards = ({ summary, loading }) => {
  const formatRank = (tier, rank) => {
    if (!tier || tier === 'UNRANKED') return 'Non classé'
    return `${tier} ${rank || ''}`
  }

  const stats = [
    {
      icon: <People sx={{ color: '#C89B3C', fontSize: 28 }} />,
      title: 'Participants',
      value: summary?.totalPlayers || '0',
      subtitle: 'Joueurs actifs dans le challenge',
      color: '#C89B3C'
    },
    {
      icon: <SportsEsports sx={{ color: '#00F5FF', fontSize: 28 }} />,
      title: 'Parties',
      value: summary?.totalGames || '0',
      subtitle: 'Total des parties jouées',
      color: '#00F5FF'
    },
    {
      icon: <TrendingUp sx={{ color: '#F39C12', fontSize: 28 }} />,
      title: 'Winrate Moyen',
      value: summary?.averageWinRate ? `${summary.averageWinRate}%` : '0%',
      subtitle: 'Moyenne de tous les joueurs',
      color: '#F39C12'
    },
    {
      icon: <EmojiEvents sx={{ color: '#E74C3C', fontSize: 28 }} />,
      title: 'Leader',
      value: summary?.topPlayer ? summary.topPlayer.gameName : 'Aucun',
      subtitle: summary?.topPlayer 
        ? `${formatRank(summary.topPlayer.currentTier, summary.topPlayer.currentRank)} - ${summary.topPlayer.currentLeaguePoints} LP`
        : 'Pas de classement',
      color: '#E74C3C'
    }
  ]

  return (
    <Box sx={{ mb: 4 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Typography variant="h3" sx={{ fontWeight: 600 }}>
          Statistiques du Challenge
        </Typography>
        <Chip 
          label="Live" 
          size="small" 
          sx={{ 
            background: 'rgba(0,245,255,0.2)',
            color: '#00F5FF',
            fontWeight: 600,
            animation: 'pulse 2s infinite'
          }} 
        />
      </Box>
      
      <Grid container spacing={3}>
        {stats.map((stat, index) => (
          <Grid item xs={12} sm={6} md={3} key={stat.title}>
            <StatCard 
              {...stat} 
              loading={loading}
              delay={index * 0.1}
            />
          </Grid>
        ))}
      </Grid>
      
      <style>
        {`
          @keyframes pulse {
            0% { opacity: 1; }
            50% { opacity: 0.5; }
            100% { opacity: 1; }
          }
        `}
      </style>
    </Box>
  )
}

export default StatsCards