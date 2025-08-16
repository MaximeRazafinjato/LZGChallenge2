import { useState, useEffect } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Typography,
  TextField,
  FormControl,
  Select,
  MenuItem,
  InputLabel,
  Chip,
  IconButton,
  InputAdornment,
  Skeleton,
  Badge
} from '@mui/material'
import {
  Close,
  Search,
  History,
  FilterList,
  TrendingUp,
  TrendingDown,
  SportsEsports
} from '@mui/icons-material'
import { motion, AnimatePresence } from 'framer-motion'
import { matchesApi } from '../services/api'
import { getChampionImageUrlNormalized } from '../utils/championUtils'

const MatchHistoryModal = ({ open, onClose, playerId, playerName }) => {
  const [matches, setMatches] = useState([])
  const [matchStats, setMatchStats] = useState(null)
  const [seasonInfo, setSeasonInfo] = useState(null)
  const [loading, setLoading] = useState(false)
  const [filters, setFilters] = useState({
    period: 'all',
    result: 'all',
    champion: 'all',
    position: 'all',
    limit: 30
  })

  const loadMatches = async () => {
    if (!playerId) return
    
    setLoading(true)
    try {
      const [matchesResponse, statsResponse, seasonResponse] = await Promise.all([
        matchesApi.getFiltered(playerId, filters),
        matchesApi.getStats(playerId),
        seasonInfo ? Promise.resolve({ data: seasonInfo }) : matchesApi.getSeasonInfo()
      ])
      
      setMatches(matchesResponse.data)
      setMatchStats(statsResponse.data)
      if (!seasonInfo) {
        setSeasonInfo(seasonResponse.data)
      }
    } catch (error) {
      console.error('Erreur lors du chargement de l\'historique:', error)
      setMatches([])
      setMatchStats(null)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (open && playerId) {
      loadMatches()
    }
  }, [open, playerId, filters])

  const handleFilterChange = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }))
  }

  const getResultColor = (win) => {
    return win ? '#50C878' : '#E74C3C'
  }

  const getKDAColor = (kda) => {
    if (kda >= 3) return '#00F5FF'
    if (kda >= 2) return '#50C878'
    if (kda >= 1) return '#F39C12'
    return '#E74C3C'
  }

  const getLpChangeColor = (lpChange) => {
    if (!lpChange) return 'text.secondary'
    return lpChange > 0 ? '#50C878' : '#E74C3C'
  }

  const getRoleIcon = (position) => {
    const icons = {
      'TOP': '⚔️',
      'JUNGLE': '🌲',
      'MIDDLE': '🔮',
      'BOTTOM': '🏹',
      'UTILITY': '🛡️'
    }
    return icons[position] || '❓'
  }

  const formatDate = (dateString) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const periodOptions = [
    { value: 'all', label: `Toute la saison ${seasonInfo?.currentSeason || ''}` },
    { value: '7days', label: '7 derniers jours' },
    { value: '30days', label: '30 derniers jours' },
    { value: 'season', label: 'Depuis début saison' }
  ]

  const resultOptions = [
    { value: 'all', label: 'Toutes les parties' },
    { value: 'wins', label: 'Victoires uniquement' },
    { value: 'losses', label: 'Défaites uniquement' }
  ]

  const positionOptions = [
    { value: 'all', label: 'Tous les rôles' },
    { value: 'TOP', label: 'Top' },
    { value: 'JUNGLE', label: 'Jungle' },
    { value: 'MIDDLE', label: 'Mid' },
    { value: 'BOTTOM', label: 'ADC' },
    { value: 'UTILITY', label: 'Support' }
  ]

  return (
    <AnimatePresence>
      {open && (
        <Dialog
          open={open}
          onClose={onClose}
          maxWidth="xl"
          fullWidth
          PaperProps={{
            sx: {
              backgroundColor: 'rgba(30, 35, 40, 0.95)',
              backdropFilter: 'blur(10px)',
              border: '1px solid rgba(255,215,0,0.3)',
              borderRadius: 3,
              minHeight: '85vh'
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'space-between',
            pb: 2,
            borderBottom: '1px solid rgba(255,215,0,0.2)',
            background: 'linear-gradient(135deg, rgba(255,215,0,0.1) 0%, rgba(200,155,60,0.1) 100%)'
          }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box
                sx={{
                  p: 1,
                  borderRadius: 2,
                  background: 'linear-gradient(45deg, #FFD700 30%, #F0E6D2 90%)',
                }}
              >
                <History sx={{ color: '#1E2328', fontSize: 24 }} />
              </Box>
              <Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="h5" sx={{ fontWeight: 600, color: '#F0E6D2' }}>
                    Historique Solo Queue - {playerName}
                  </Typography>
                  <Badge 
                    badgeContent={seasonInfo?.currentSeason || '?'} 
                    sx={{ 
                      '& .MuiBadge-badge': { 
                        backgroundColor: '#FFD700',
                        color: '#1E2328',
                        fontWeight: 600,
                        fontSize: '0.7rem'
                      }
                    }}
                  >
                    <Chip 
                      label="SoloQ" 
                      size="small"
                      sx={{
                        backgroundColor: 'rgba(255,215,0,0.2)',
                        color: '#FFD700',
                        fontWeight: 600
                      }}
                    />
                  </Badge>
                </Box>
                <Typography variant="body2" color="text.secondary">
                  Saison {seasonInfo?.currentSeason || '?'} • Queue ID 420 • Solo/Duo Classé uniquement
                </Typography>
                {matchStats && (
                  <Typography variant="body2" sx={{ color: '#FFD700', mt: 0.5 }}>
                    {matchStats.totalGames} parties • {matchStats.winRate}% WR • {matchStats.totalLpChange > 0 ? '+' : ''}{matchStats.totalLpChange} LP
                  </Typography>
                )}
              </Box>
            </Box>
            <IconButton
              onClick={onClose}
              sx={{
                color: 'rgba(255,215,0,0.7)',
                '&:hover': { 
                  color: '#FFD700',
                  backgroundColor: 'rgba(255,215,0,0.1)'
                }
              }}
            >
              <Close />
            </IconButton>
          </DialogTitle>

          <DialogContent sx={{ p: 3 }}>
            {/* Filtres */}
            <Box sx={{ 
              display: 'flex', 
              gap: 2, 
              mb: 3, 
              flexWrap: 'wrap',
              alignItems: 'center',
              p: 2,
              mt: 2,
              borderRadius: 2,
              backgroundColor: 'rgba(255,215,0,0.05)',
              border: '1px solid rgba(255,215,0,0.2)'
            }}>
              <FilterList sx={{ color: '#FFD700' }} />
              
              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Période</InputLabel>
                <Select
                  value={filters.period}
                  label="Période"
                  onChange={(e) => handleFilterChange('period', e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(255,215,0,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(255,215,0,0.3)'
                    }
                  }}
                >
                  {periodOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Résultat</InputLabel>
                <Select
                  value={filters.result}
                  label="Résultat"
                  onChange={(e) => handleFilterChange('result', e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(255,215,0,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(255,215,0,0.3)'
                    }
                  }}
                >
                  {resultOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Rôle</InputLabel>
                <Select
                  value={filters.position}
                  label="Rôle"
                  onChange={(e) => handleFilterChange('position', e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(255,215,0,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(255,215,0,0.3)'
                    }
                  }}
                >
                  {positionOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            {/* Statistiques rapides */}
            {matchStats && !loading && (
              <Box sx={{ 
                display: 'flex', 
                gap: 3, 
                mb: 3, 
                p: 2, 
                borderRadius: 2,
                background: 'linear-gradient(135deg, rgba(255,215,0,0.1) 0%, rgba(200,155,60,0.05) 100%)',
                border: '1px solid rgba(255,215,0,0.2)'
              }}>
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="h6" sx={{ color: '#FFD700', fontWeight: 600 }}>
                    {matchStats.totalGames}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Parties
                  </Typography>
                </Box>
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="h6" sx={{ color: getResultColor(matchStats.winRate >= 50), fontWeight: 600 }}>
                    {matchStats.winRate}%
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Winrate
                  </Typography>
                </Box>
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="h6" sx={{ color: getKDAColor(matchStats.averageKDA), fontWeight: 600 }}>
                    {matchStats.averageKDA}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    KDA Moyen
                  </Typography>
                </Box>
                <Box sx={{ textAlign: 'center' }}>
                  <Typography variant="h6" sx={{ color: getLpChangeColor(matchStats.totalLpChange), fontWeight: 600 }}>
                    {matchStats.totalLpChange > 0 ? '+' : ''}{matchStats.totalLpChange}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    LP Total
                  </Typography>
                </Box>
              </Box>
            )}

            {/* Tableau des parties */}
            {loading ? (
              <Box>
                {[...Array(10)].map((_, i) => (
                  <Skeleton 
                    key={i} 
                    variant="rectangular" 
                    height={60} 
                    sx={{ mb: 1, borderRadius: 1 }} 
                  />
                ))}
              </Box>
            ) : matches.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 8 }}>
                <SportsEsports sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
                <Typography variant="h5" sx={{ mb: 1 }}>
                  Aucune partie Solo Queue trouvée
                </Typography>
                <Typography color="text.secondary">
                  Ce joueur n'a pas encore joué de parties classées en saison {seasonInfo?.currentSeason || '?'} avec ces filtres.
                </Typography>
              </Box>
            ) : (
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.3 }}
              >
                <TableContainer 
                  component={Paper} 
                  sx={{ 
                    backgroundColor: 'transparent', 
                    boxShadow: 'none',
                    border: '1px solid rgba(255,215,0,0.2)',
                    borderRadius: 2
                  }}
                >
                  <Table>
                    <TableHead>
                      <TableRow sx={{ backgroundColor: 'rgba(255,215,0,0.1)' }}>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Date</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Durée</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Résultat</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Champion</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Rôle</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>KDA</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>CS</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Dégâts</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>LP ±</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {matches.map((match, index) => (
                        <motion.tr
                          key={match.matchId}
                          initial={{ x: -20, opacity: 0 }}
                          animate={{ x: 0, opacity: 1 }}
                          transition={{ duration: 0.3, delay: index * 0.02 }}
                          component={TableRow}
                          sx={{
                            '&:hover': { 
                              backgroundColor: 'rgba(255,215,0,0.1)',
                              transform: 'scale(1.002)',
                              transition: 'all 0.2s ease'
                            },
                            backgroundColor: match.win ? 'rgba(80,200,120,0.05)' : 'rgba(231,76,60,0.05)'
                          }}
                        >
                          <TableCell>
                            <Typography variant="body2" sx={{ fontWeight: 500 }}>
                              {formatDate(match.gameStartTime)}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {match.formattedGameDuration}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Chip
                              label={match.resultText}
                              size="small"
                              icon={match.win ? <TrendingUp /> : <TrendingDown />}
                              sx={{
                                backgroundColor: getResultColor(match.win) + '20',
                                color: getResultColor(match.win),
                                border: `1px solid ${getResultColor(match.win)}50`,
                                fontWeight: 600
                              }}
                            />
                          </TableCell>
                          
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <img
                                src={getChampionImageUrlNormalized(match.championName)}
                                alt={match.championName}
                                style={{
                                  width: 32,
                                  height: 32,
                                  borderRadius: '50%',
                                  objectFit: 'cover',
                                  border: '2px solid rgba(255,215,0,0.3)',
                                  flexShrink: 0
                                }}
                                onError={(e) => {
                                  e.target.src = 'https://ddragon.leagueoflegends.com/cdn/14.1.1/img/champion/Unknown.png'
                                }}
                              />
                              <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                {match.championName}
                              </Typography>
                            </Box>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {getRoleIcon(match.position)} {match.position}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Box>
                              <Typography 
                                variant="body2" 
                                sx={{ 
                                  fontWeight: 600,
                                  color: getKDAColor(match.kda)
                                }}
                              >
                                {match.kda}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {match.kdaText}
                              </Typography>
                            </Box>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {match.creepScore}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {(match.totalDamageDealtToChampions || 0).toLocaleString()}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            {match.lpChange !== null ? (
                              <Typography 
                                variant="body2" 
                                sx={{ 
                                  fontWeight: 600,
                                  color: getLpChangeColor(match.lpChange)
                                }}
                              >
                                {match.lpChange > 0 ? '+' : ''}{match.lpChange}
                              </Typography>
                            ) : (
                              <Typography variant="body2" color="text.secondary">
                                -
                              </Typography>
                            )}
                          </TableCell>
                        </motion.tr>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </motion.div>
            )}
          </DialogContent>
        </Dialog>
      )}
    </AnimatePresence>
  )
}

export default MatchHistoryModal