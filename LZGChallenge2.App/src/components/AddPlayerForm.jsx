import { useState } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Grid,
  Typography,
  Box,
  Alert,
  CircularProgress,
  Fade,
  IconButton
} from '@mui/material'
import { PersonAdd, Send, Close } from '@mui/icons-material'

const AddPlayerForm = ({ open, onClose, onPlayerAdded }) => {
  const [formData, setFormData] = useState({
    gameName: '',
    tagLine: '',
    region: 'EUW1'
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [message, setMessage] = useState({ type: '', text: '' })

  const regions = [
    { value: 'EUW1', label: 'Europe West (EUW)' },
    { value: 'EUNE1', label: 'Europe Nordic & East (EUNE)' },
    { value: 'NA1', label: 'North America (NA)' },
    { value: 'KR', label: 'Korea (KR)' },
    { value: 'JP1', label: 'Japan (JP)' },
    { value: 'BR1', label: 'Brazil (BR)' },
    { value: 'LA1', label: 'Latin America North (LAN)' },
    { value: 'LA2', label: 'Latin America South (LAS)' },
    { value: 'OC1', label: 'Oceania (OCE)' },
    { value: 'TR1', label: 'Turkey (TR)' },
    { value: 'RU', label: 'Russia (RU)' }
  ]

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    // Clear message when user types
    if (message.text) {
      setMessage({ type: '', text: '' })
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setIsSubmitting(true)
    setMessage({ type: '', text: '' })

    try {
      const response = await fetch('https://localhost:44393/api/players', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      })

      if (response.ok) {
        const newPlayer = await response.json()
        setMessage({
          type: 'success',
          text: `${formData.gameName}#${formData.tagLine} a été ajouté avec succès ! Récupération des matches en cours...`
        })
        setFormData({ gameName: '', tagLine: '', region: 'EUW1' })
        onPlayerAdded?.(newPlayer)
        
        // Fermer la modal après un délai pour montrer le message de succès
        setTimeout(() => {
          onClose?.()
          setMessage({ type: '', text: '' })
        }, 3000)
      } else {
        const errorText = await response.text()
        setMessage({
          type: 'error',
          text: errorText || 'Erreur lors de l\'ajout du joueur'
        })
      }
    } catch (error) {
      setMessage({
        type: 'error',
        text: 'Erreur de connexion. Vérifiez que le serveur est démarré.'
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog 
      open={open} 
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: {
          backgroundImage: 'linear-gradient(145deg, #1E2328 0%, #3C3C41 100%)',
          border: '1px solid rgba(200,155,60,0.3)',
          borderRadius: 3,
        }
      }}
    >
      <DialogTitle sx={{ 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'space-between',
        pb: 2 
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              p: 1.5,
              borderRadius: '50%',
              aspectRatio: '1/1',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              background: 'rgba(200,155,60,0.2)',
              border: '1px solid rgba(200,155,60,0.3)',
            }}
          >
            <PersonAdd sx={{ color: 'primary.main', fontSize: 28 }} />
          </Box>
          <Typography variant="h3" sx={{ fontWeight: 600 }}>
            Ajouter un participant
          </Typography>
        </Box>
        
        <IconButton 
          onClick={onClose}
          sx={{ 
            color: 'text.secondary',
            '&:hover': { backgroundColor: 'rgba(240,230,210,0.1)' }
          }}
        >
          <Close />
        </IconButton>
      </DialogTitle>

      <DialogContent sx={{ px: 3 }}>
        <Box component="form" onSubmit={handleSubmit}>
          <Grid container spacing={3} sx={{ mb: 3, mt: 1 }}>
            <Grid item xs={12} md={4} >
              <TextField
                fullWidth
                label="Nom d'invocateur"
                name="gameName"
                value={formData.gameName}
                onChange={handleInputChange}
                placeholder="Ex: Faker"
                required
                disabled={isSubmitting}
                variant="outlined"
                sx={{
                  '& .MuiOutlinedInput-root': {
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '&:hover': {
                      backgroundColor: 'rgba(240,230,210,0.08)',
                    },
                    '&.Mui-focused': {
                      backgroundColor: 'rgba(240,230,210,0.1)',
                    }
                  }
                }}
              />
            </Grid>

            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Tag"
                name="tagLine"
                value={formData.tagLine}
                onChange={handleInputChange}
                placeholder="Ex: KR1"
                required
                disabled={isSubmitting}
                variant="outlined"
                sx={{
                  '& .MuiOutlinedInput-root': {
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '&:hover': {
                      backgroundColor: 'rgba(240,230,210,0.08)',
                    },
                    '&.Mui-focused': {
                      backgroundColor: 'rgba(240,230,210,0.1)',
                    }
                  }
                }}
              />
            </Grid>

            <Grid item xs={12} md={4}>
              <FormControl fullWidth>
                <InputLabel>Région</InputLabel>
                <Select
                  name="region"
                  value={formData.region}
                  onChange={handleInputChange}
                  label="Région"
                  disabled={isSubmitting}
                  sx={{
                    backgroundColor: 'rgba(240,230,210,0.05)',
                    '&:hover': {
                      backgroundColor: 'rgba(240,230,210,0.08)',
                    },
                    '&.Mui-focused': {
                      backgroundColor: 'rgba(240,230,210,0.1)',
                    }
                  }}
                >
                  {regions.map(region => (
                    <MenuItem key={region.value} value={region.value}>
                      {region.label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>

          {/* Message d'état */}
          <Fade in={!!message.text}>
            <Box sx={{ mb: 2 }}>
              {message.text && (
                <Alert 
                  severity={message.type === 'success' ? 'success' : 'error'}
                  sx={{
                    backgroundColor: message.type === 'success' 
                      ? 'rgba(0,245,255,0.1)' 
                      : 'rgba(231,76,60,0.1)',
                    border: message.type === 'success'
                      ? '1px solid rgba(0,245,255,0.3)'
                      : '1px solid rgba(231,76,60,0.3)',
                    color: message.type === 'success' ? '#00F5FF' : '#E74C3C'
                  }}
                >
                  {message.text}
                </Alert>
              )}
            </Box>
          </Fade>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 3 }}>
        <Button
          onClick={onClose}
          disabled={isSubmitting}
          sx={{
            color: 'text.secondary',
            '&:hover': {
              backgroundColor: 'rgba(240,230,210,0.1)',
            }
          }}
        >
          Annuler
        </Button>
        
        <Button
          onClick={handleSubmit}
          disabled={isSubmitting || !formData.gameName.trim() || !formData.tagLine.trim()}
          startIcon={isSubmitting ? <CircularProgress size={20} /> : <Send />}
          sx={{
            px: 3,
            py: 1,
            fontSize: '1rem',
            fontWeight: 600,
            background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
            color: '#1E2328',
            '&:hover': {
              background: 'linear-gradient(45deg, #B8860B 30%, #C89B3C 90%)',
              transform: 'translateY(-2px)',
              boxShadow: '0 8px 25px rgba(200,155,60,0.3)',
            },
            '&:disabled': {
              background: 'rgba(120,120,120,0.3)',
              color: 'rgba(240,230,210,0.5)',
            }
          }}
        >
          {isSubmitting ? 'Ajout et récupération des données...' : 'Rejoindre le Challenge'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default AddPlayerForm