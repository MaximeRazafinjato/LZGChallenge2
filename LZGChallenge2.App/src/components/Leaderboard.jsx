import { useState } from 'react'
import {
  Card,
  CardContent,
  Typography,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Button,
  FormControl,
  Select,
  MenuItem,
  Avatar,
  LinearProgress,
  Skeleton,
  Tooltip
} from '@mui/material'
import { 
  EmojiEvents, 
  Refresh, 
  GpsFixed, 
  Bolt,
  Sports,
  LocalFireDepartment,
  HeartBroken,
  SportsEsports
} from '@mui/icons-material'
import { motion } from 'framer-motion'
import ChampionModal from './ChampionModal'
import { getChampionImageUrlNormalized } from '../utils/championUtils'

const Leaderboard = ({ leaderboard = [], loading, onRefresh, onSortChange }) => {
  const [sortBy, setSortBy] = useState('lp')
  const [championModalOpen, setChampionModalOpen] = useState(false)
  const [selectedPlayer, setSelectedPlayer] = useState(null)

  const handleSortChange = (newSortBy) => {
    setSortBy(newSortBy)
    onSortChange?.(newSortBy)
  }

  const calculateRankScore = (tier, rank, leaguePoints) => {
    // Valeurs de base pour chaque tier
    const tierValues = {
      'IRON': 0,
      'BRONZE': 400,
      'SILVER': 800,
      'GOLD': 1200,
      'PLATINUM': 1600,
      'EMERALD': 2000,
      'DIAMOND': 2400,
      'MASTER': 2800,
      'GRANDMASTER': 3200,
      'CHALLENGER': 3600
    }

    // Si pas de tier, retourner 0
    if (!tier || !tierValues[tier.toUpperCase()]) {
      return 0
    }

    const baseScore = tierValues[tier.toUpperCase()]

    // Ajouter les points pour le rank (IV = 0, III = 100, II = 200, I = 300)
    const rankValue = {
      'IV': 0,
      'III': 100,
      'II': 200,
      'I': 300
    }[rank?.toUpperCase()] || 0

    // Ajouter les LP (max 100 par division)
    const lpValue = Math.min(leaguePoints || 0, 100)

    return baseScore + rankValue + lpValue
  }

  const formatRank = (tier, rank, lp) => {
    if (!tier || tier === 'UNRANKED') return 'Non classé'
    return `${tier} ${rank}`
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

  const getPositionIcon = (position) => {
    switch (position) {
      case 1: return '🥇'
      case 2: return '🥈'
      case 3: return '🥉'
      default: return position
    }
  }

  const getWinRateColor = (winRate) => {
    if (winRate >= 70) return '#00F5FF'
    if (winRate >= 60) return '#50C878'
    if (winRate >= 50) return '#F39C12'
    return '#E74C3C'
  }

  const getKDAColor = (kda) => {
    if (kda >= 3) return '#00F5FF'
    if (kda >= 2) return '#50C878'
    if (kda >= 1) return '#F39C12'
    return '#E74C3C'
  }

  const handleChampionClick = (player) => {
    setSelectedPlayer(player)
    setChampionModalOpen(true)
  }

  const sortOptions = [
    { value: 'lp', label: 'Points de Ligue', icon: <EmojiEvents /> },
    { value: 'winrate', label: 'Taux de victoire', icon: <GpsFixed /> },
    { value: 'kda', label: 'KDA', icon: <Bolt /> },
    { value: 'games', label: 'Nombre de parties', icon: <Sports /> }
  ]

  if (loading) {
    return (
      <Card sx={{ mb: 4 }}>
        <CardContent sx={{ p: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
            <Skeleton variant="circular" width={32} height={32} />
            <Skeleton variant="text" width={200} height={32} />
          </Box>
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} variant="rectangular" height={60} sx={{ mb: 1 }} />
          ))}
        </CardContent>
      </Card>
    )
  }

  return (
    <motion.div
      initial={{ y: 30, opacity: 0 }}
      animate={{ y: 0, opacity: 1 }}
      transition={{ duration: 0.6, delay: 0.4 }}
    >
      <Card sx={{ mb: 4 }}>
        <CardContent sx={{ p: 4 }}>
          {/* Header */}
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box
                sx={{
                  p: 1.5,
                  borderRadius: 2,
                  background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
                }}
              >
                <EmojiEvents sx={{ color: '#1E2328', fontSize: 28 }} />
              </Box>
              <Typography variant="h3" sx={{ fontWeight: 600 }}>
                Classement du Challenge
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <FormControl size="small" sx={{ minWidth: 150 }}>
                <Select
                  value={sortBy}
                  onChange={(e) => handleSortChange(e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(200,155,60,0.3)'
                    }
                  }}
                >
                  {sortOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        {option.icon}
                        {option.label}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <Button
                onClick={onRefresh}
                disabled={loading}
                startIcon={<Refresh />}
                sx={{
                  background: 'rgba(200,155,60,0.2)',
                  border: '1px solid rgba(200,155,60,0.3)',
                  color: 'primary.main',
                  '&:hover': {
                    background: 'rgba(200,155,60,0.3)',
                  }
                }}
              >
                Actualiser
              </Button>
            </Box>
          </Box>

          {/* Empty state */}
          {leaderboard.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 8 }}>
              <EmojiEvents sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h4" sx={{ mb: 1 }}>
                Aucun classement disponible
              </Typography>
              <Typography color="text.secondary">
                Les joueurs apparaîtront ici une fois leurs statistiques mises à jour.
              </Typography>
            </Box>
          ) : (
            /* Leaderboard table */
            <TableContainer component={Paper} sx={{ backgroundColor: 'transparent', boxShadow: 'none' }}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Position</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Joueur</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Rang</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>LP</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Winrate</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>KDA</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Champions</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Parties</TableCell>
                    <TableCell sx={{ color: 'text.primary', fontWeight: 600 }}>Progression</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {leaderboard.map((player, index) => (
                    <motion.tr
                      key={player.playerId}
                      initial={{ x: -20, opacity: 0 }}
                      animate={{ x: 0, opacity: 1 }}
                      transition={{ duration: 0.3, delay: index * 0.1 }}
                      component={TableRow}
                      sx={{
                        '&:hover': { 
                          backgroundColor: 'rgba(200,155,60,0.1)',
                          transform: 'scale(1.01)',
                          transition: 'all 0.2s ease'
                        },
                        backgroundColor: index < 3 ? 'rgba(200,155,60,0.05)' : 'transparent'
                      }}
                    >
                      <TableCell>
                        <Typography variant="h4" sx={{ fontWeight: 700 }}>
                          {getPositionIcon(index + 1)}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                          <Avatar
                            sx={{
                              bgcolor: getTierColor(player.currentTier),
                              color: '#1E2328',
                              fontWeight: 600
                            }}
                          >
                            {player.gameName?.[0]?.toUpperCase()}
                          </Avatar>
                          <Box>
                            <Typography variant="body1" sx={{ fontWeight: 600 }}>
                              {player.gameName}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                              #{player.tagLine}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      
                      <TableCell>
                        <Chip
                          label={formatRank(player.currentTier, player.currentRank)}
                          sx={{
                            backgroundColor: getTierColor(player.currentTier) + '30',
                            color: getTierColor(player.currentTier),
                            border: `1px solid ${getTierColor(player.currentTier)}50`,
                            fontWeight: 600
                          }}
                        />
                      </TableCell>
                      
                      <TableCell>
                        <Typography variant="h4" sx={{ fontWeight: 700, color: 'primary.main' }}>
                          {player.currentLeaguePoints}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Box>
                          <Typography 
                            variant="body1" 
                            sx={{ 
                              fontWeight: 600,
                              color: getWinRateColor(player.winRate)
                            }}
                          >
                            {player.winRate?.toFixed(1)}%
                          </Typography>
                          <LinearProgress
                            variant="determinate"
                            value={player.winRate || 0}
                            sx={{
                              mt: 0.5,
                              height: 4,
                              borderRadius: 2,
                              backgroundColor: 'rgba(120,120,120,0.3)',
                              '& .MuiLinearProgress-bar': {
                                backgroundColor: getWinRateColor(player.winRate),
                                borderRadius: 2
                              }
                            }}
                          />
                        </Box>
                      </TableCell>
                      
                      <TableCell>
                        <Typography 
                          variant="body1" 
                          sx={{ 
                            fontWeight: 600,
                            color: getKDAColor(player.kda)
                          }}
                        >
                          {player.kda?.toFixed(2)}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Box 
                          sx={{ 
                            display: 'flex', 
                            gap: 0.5,
                            cursor: 'pointer',
                            '&:hover': {
                              transform: 'scale(1.05)',
                              transition: 'all 0.2s ease'
                            }
                          }}
                          onClick={() => handleChampionClick(player)}
                        >
                            {player.topChampions?.length > 0 ? (
                              player.topChampions.slice(0, 3).map((champion, idx) => (
                                <Tooltip 
                                  key={champion.championId}
                                  title={`${champion.championName} - ${champion.gamesPlayed} parties (${champion.winRate?.toFixed(1)}%)`}
                                >
                                  <Box
                                    sx={{
                                      position: 'relative',
                                      display: 'flex',
                                      alignItems: 'center',
                                      width: 32,
                                      height: 32,
                                      flexShrink: 0,
                                      '&:hover': {
                                        transform: 'scale(1.1)',
                                        transition: 'all 0.2s ease'
                                      }
                                    }}
                                  >
                                    <img
                                      src={getChampionImageUrlNormalized(champion.championName)}
                                      alt={champion.championName}
                                      style={{
                                        width: 32,
                                        height: 32,
                                        borderRadius: '50%',
                                        objectFit: 'cover',
                                        border: '2px solid rgba(200,155,60,0.5)',
                                        boxShadow: '0 2px 8px rgba(0,0,0,0.3)',
                                        display: 'block',
                                        flexShrink: 0
                                      }}
                                      onError={(e) => {
                                        e.target.src = 'https://ddragon.leagueoflegends.com/cdn/14.1.1/img/champion/Unknown.png'
                                      }}
                                    />
                                    <Typography
                                      variant="caption"
                                      sx={{
                                        position: 'absolute',
                                        bottom: -2,
                                        right: -2,
                                        backgroundColor: 'rgba(200,155,60,0.9)',
                                        color: '#1E2328',
                                        borderRadius: '50%',
                                        width: 16,
                                        height: 16,
                                        fontSize: '0.6rem',
                                        fontWeight: 600,
                                        display: 'flex',
                                        alignItems: 'center',
                                        justifyContent: 'center',
                                        minWidth: 'unset'
                                      }}
                                    >
                                      {champion.gamesPlayed}
                                    </Typography>
                                  </Box>
                                </Tooltip>
                              ))
                            ) : (
                              <Chip
                                icon={<SportsEsports />}
                                label="Aucun champion"
                                size="small"
                                sx={{
                                  backgroundColor: 'rgba(120,120,120,0.2)',
                                  color: 'text.secondary',
                                  fontSize: '0.7rem'
                                }}
                              />
                            )}
                        </Box>
                      </TableCell>
                      
                      <TableCell>
                        <Typography variant="body1" sx={{ fontWeight: 600 }}>
                          {player.totalGames}
                        </Typography>
                      </TableCell>
                      
                      <TableCell>
                        <Box>
                          <Typography 
                            variant="body1" 
                            sx={{ 
                              fontWeight: 600,
                              color: player.netLpChange > 0 ? '#50C878' : 
                                     player.netLpChange < 0 ? '#E74C3C' : 'text.secondary'
                            }}
                          >
                            {player.netLpChange > 0 ? '+' : ''}{player.netLpChange} LP
                          </Typography>
                          
                          {player.currentWinStreak > 1 && (
                            <Chip
                              icon={<LocalFireDepartment />}
                              label={`${player.currentWinStreak} victoires`}
                              size="small"
                              sx={{
                                mt: 0.5,
                                backgroundColor: 'rgba(80,200,120,0.2)',
                                color: '#50C878',
                                fontSize: '0.7rem'
                              }}
                            />
                          )}
                          
                          {player.currentLoseStreak > 1 && (
                            <Chip
                              icon={<HeartBroken />}
                              label={`${player.currentLoseStreak} défaites`}
                              size="small"
                              sx={{
                                mt: 0.5,
                                backgroundColor: 'rgba(231,76,60,0.2)',
                                color: '#E74C3C',
                                fontSize: '0.7rem'
                              }}
                            />
                          )}
                        </Box>
                      </TableCell>
                    </motion.tr>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Champion Modal */}
      <ChampionModal
        open={championModalOpen}
        onClose={() => setChampionModalOpen(false)}
        playerId={selectedPlayer?.playerId}
        playerName={selectedPlayer ? `${selectedPlayer.gameName}#${selectedPlayer.tagLine}` : ''}
      />
    </motion.div>
  )
}

export default Leaderboard