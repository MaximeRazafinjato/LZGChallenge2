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
  LinearProgress,
  Skeleton,
  IconButton,
  InputAdornment
} from '@mui/material'
import {
  Close,
  Search,
  EmojiEvents,
  SportsEsports,
  FilterList
} from '@mui/icons-material'
import { motion, AnimatePresence } from 'framer-motion'
import { championStatsApi } from '../services/api'
import { getChampionImageUrlNormalized } from '../utils/championUtils'

const ChampionModal = ({ open, onClose, playerId, playerName }) => {
  const [champions, setChampions] = useState([])
  const [loading, setLoading] = useState(false)
  const [filters, setFilters] = useState({
    sortBy: 'games',
    role: 'all',
    search: '',
    limit: 50
  })

  const loadChampions = async () => {
    if (!playerId) return
    
    setLoading(true)
    try {
      const response = await championStatsApi.getFiltered(playerId, filters)
      setChampions(response.data)
    } catch (error) {
      console.error('Erreur lors du chargement des champions:', error)
      setChampions([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (open && playerId) {
      loadChampions()
    }
  }, [open, playerId, filters])

  const handleFilterChange = (key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }))
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

  const getRoleColor = (role) => {
    const colors = {
      'TOP': '#FF6B6B',
      'JUNGLE': '#4ECDC4', 
      'MIDDLE': '#45B7D1',
      'BOTTOM': '#FFA07A',
      'UTILITY': '#98D8C8',
      'UNKNOWN': '#95A5A6'
    }
    return colors[role] || colors.UNKNOWN
  }

  const getRoleIcon = (role) => {
    const icons = {
      'TOP': '⚔️',
      'JUNGLE': '🌲',
      'MIDDLE': '🔮', 
      'BOTTOM': '🏹',
      'UTILITY': '🛡️',
      'UNKNOWN': '❓'
    }
    return icons[role] || icons.UNKNOWN
  }

  const sortOptions = [
    { value: 'games', label: 'Parties jouées' },
    { value: 'winrate', label: 'Taux de victoire' },
    { value: 'kda', label: 'KDA' },
    { value: 'alphabetical', label: 'Alphabétique' }
  ]

  const roleOptions = [
    { value: 'all', label: 'Tous les rôles' },
    { value: 'top', label: 'Top' },
    { value: 'jungle', label: 'Jungle' },
    { value: 'mid', label: 'Mid' },
    { value: 'adc', label: 'ADC' },
    { value: 'support', label: 'Support' }
  ]

  return (
    <AnimatePresence>
      {open && (
        <Dialog
          open={open}
          onClose={onClose}
          maxWidth="lg"
          fullWidth
          PaperProps={{
            sx: {
              backgroundColor: 'rgba(30, 35, 40, 0.95)',
              backdropFilter: 'blur(10px)',
              border: '1px solid rgba(200,155,60,0.3)',
              borderRadius: 3,
              minHeight: '80vh'
            }
          }}
        >
          <DialogTitle sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'space-between',
            pb: 2,
            borderBottom: '1px solid rgba(200,155,60,0.2)'
          }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <Box
                sx={{
                  p: 1,
                  borderRadius: '50%',
                  aspectRatio: '1/1',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
                }}
              >
                <EmojiEvents sx={{ color: '#1E2328', fontSize: 24 }} />
              </Box>
              <Box>
                <Typography variant="h5" sx={{ fontWeight: 600, color: '#F0E6D2' }}>
                  Champions de {playerName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Toutes les statistiques de la saison en cours
                </Typography>
              </Box>
            </Box>
            <IconButton
              onClick={onClose}
              sx={{
                color: 'rgba(200,155,60,0.7)',
                '&:hover': { 
                  color: '#C89B3C',
                  backgroundColor: 'rgba(200,155,60,0.1)'
                }
              }}
            >
              <Close />
            </IconButton>
          </DialogTitle>

          <DialogContent sx={{ p: 3}}>
            {/* Filtres */}
            <Box sx={{ 
              display: 'flex', 
              gap: 2, 
              mb: 3, 
              mt: 2,
              flexWrap: 'wrap',
              alignItems: 'center'
            }}>
              <FilterList sx={{ color: 'text.secondary' }} />
              
              <TextField
                size="small"
                placeholder="Rechercher un champion..."
                value={filters.search}
                onChange={(e) => handleFilterChange('search', e.target.value)}
                sx={{ 
                  minWidth: 200,
                  '& .MuiOutlinedInput-root': {
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '& fieldset': {
                      borderColor: 'rgba(200,155,60,0.3)'
                    },
                    '&:hover fieldset': {
                      borderColor: 'rgba(200,155,60,0.5)'
                    }
                  }
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <Search sx={{ color: 'text.secondary' }} />
                    </InputAdornment>
                  ),
                }}
              />

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Tri par</InputLabel>
                <Select
                  value={filters.sortBy}
                  label="Tri par"
                  onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(200,155,60,0.3)'
                    }
                  }}
                >
                  {sortOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Rôle</InputLabel>
                <Select
                  value={filters.role}
                  label="Rôle"
                  onChange={(e) => handleFilterChange('role', e.target.value)}
                  sx={{
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '& .MuiOutlinedInput-notchedOutline': {
                      borderColor: 'rgba(200,155,60,0.3)'
                    }
                  }}
                >
                  {roleOptions.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>

            {/* Tableau des champions */}
            {loading ? (
              <Box>
                {[...Array(8)].map((_, i) => (
                  <Skeleton 
                    key={i} 
                    variant="rectangular" 
                    height={60} 
                    sx={{ mb: 1, borderRadius: 1 }} 
                  />
                ))}
              </Box>
            ) : champions.length === 0 ? (
              <Box sx={{ textAlign: 'center', py: 8 }}>
                <SportsEsports sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
                <Typography variant="h5" sx={{ mb: 1 }}>
                  Aucun champion trouvé
                </Typography>
                <Typography color="text.secondary">
                  Essayez de modifier vos filtres de recherche.
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
                    border: '1px solid rgba(200,155,60,0.2)',
                    borderRadius: 2
                  }}
                >
                  <Table>
                    <TableHead>
                      <TableRow sx={{ backgroundColor: 'rgba(200,155,60,0.1)' }}>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Champion</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Rôle</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Parties</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>W/L</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Winrate</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>KDA</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>CS/min</TableCell>
                        <TableCell sx={{ color: '#F0E6D2', fontWeight: 600 }}>Dégâts/min</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {champions.map((champion, index) => (
                        <motion.tr
                          key={champion.championId}
                          initial={{ x: -20, opacity: 0 }}
                          animate={{ x: 0, opacity: 1 }}
                          transition={{ duration: 0.3, delay: index * 0.05 }}
                          component={TableRow}
                          sx={{
                            '&:hover': { 
                              backgroundColor: 'rgba(200,155,60,0.1)',
                              transform: 'scale(1.002)',
                              transition: 'all 0.2s ease'
                            }
                          }}
                        >
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, minWidth: 0 }}>
                              <img
                                src={getChampionImageUrlNormalized(champion.championName)}
                                alt={champion.championName}
                                style={{
                                  width: 40,
                                  height: 40,
                                  borderRadius: '50%',
                                  objectFit: 'cover',
                                  aspectRatio: '1/1',
                                  border: '2px solid rgba(200,155,60,0.3)',
                                  boxShadow: '0 2px 6px rgba(0,0,0,0.2)',
                                  display: 'block',
                                  flexShrink: 0
                                }}
                                onError={(e) => {
                                  e.target.src = 'https://ddragon.leagueoflegends.com/cdn/14.1.1/img/champion/Unknown.png'
                                }}
                              />
                              <Typography variant="body1" sx={{ fontWeight: 600 }}>
                                {champion.championName}
                              </Typography>
                            </Box>
                          </TableCell>
                          
                          <TableCell>
                            <Chip
                              label={`${getRoleIcon(champion.primaryRole)} ${champion.primaryRole}`}
                              size="small"
                              sx={{
                                backgroundColor: getRoleColor(champion.primaryRole) + '20',
                                color: getRoleColor(champion.primaryRole),
                                border: `1px solid ${getRoleColor(champion.primaryRole)}50`,
                                fontWeight: 500
                              }}
                            />
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body1" sx={{ fontWeight: 600 }}>
                              {champion.gamesPlayed}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {champion.wins}W / {champion.losses}L
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Box>
                              <Typography 
                                variant="body1" 
                                sx={{ 
                                  fontWeight: 600,
                                  color: getWinRateColor(champion.winRate)
                                }}
                              >
                                {champion.winRate?.toFixed(1)}%
                              </Typography>
                              <LinearProgress
                                variant="determinate"
                                value={champion.winRate || 0}
                                sx={{
                                  mt: 0.5,
                                  height: 3,
                                  borderRadius: 2,
                                  backgroundColor: 'rgba(120,120,120,0.3)',
                                  '& .MuiLinearProgress-bar': {
                                    backgroundColor: getWinRateColor(champion.winRate),
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
                                color: getKDAColor(champion.kda)
                              }}
                            >
                              {champion.kda?.toFixed(2)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {champion.averageKills?.toFixed(1)}/{champion.averageDeaths?.toFixed(1)}/{champion.averageAssists?.toFixed(1)}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {champion.averageCreepScore?.toFixed(1)}
                            </Typography>
                          </TableCell>
                          
                          <TableCell>
                            <Typography variant="body2">
                              {(champion.averageDamageDealt || 0).toLocaleString()}
                            </Typography>
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

export default ChampionModal