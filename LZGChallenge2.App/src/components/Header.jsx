import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Box,
  Chip,
  Avatar,
  Button,
  Menu,
  MenuItem,
  Divider
} from '@mui/material'
import { 
  EmojiEvents,
  MenuBook,
  Person,
  Logout,
  Settings,
  ExpandMore
} from '@mui/icons-material'
import { motion } from 'framer-motion'
import { useState } from 'react'
import { useAuth } from '../contexts/AuthContext'

const Header = () => {
  const { user, logout } = useAuth()
  const [anchorEl, setAnchorEl] = useState(null)
  const open = Boolean(anchorEl)

  const handleClick = (event) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClose = () => {
    setAnchorEl(null)
  }

  const handleLogout = async () => {
    handleClose()
    await logout()
  }

  return (
    <AppBar 
      position="static" 
      sx={{ 
        background: 'linear-gradient(90deg, rgba(200,155,60,0.1) 0%, rgba(15,32,39,0.95) 100%)',
        backdropFilter: 'blur(10px)',
        borderBottom: '1px solid rgba(200,155,60,0.2)'
      }}
    >
      <Toolbar sx={{ minHeight: '80px !important', px: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flex: 1 }}>
          {/* Logo et titre */}
          <motion.div
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.5 }}
          >
            <Avatar
              sx={{
                width: 48,
                height: 48,
                background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
                boxShadow: '0 4px 20px rgba(200,155,60,0.3)'
              }}
            >
              <EmojiEvents sx={{ color: '#1E2328', fontSize: 28 }} />
            </Avatar>
          </motion.div>
          
          <Box>
            <Typography 
              variant="h1" 
              sx={{ 
                fontSize: '2rem',
                fontWeight: 700,
                background: 'linear-gradient(45deg, #C89B3C 30%, #F0E6D2 90%)',
                backgroundClip: 'text',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                textShadow: 'none'
              }}
            >
              SoloQ Challenge
            </Typography>
            <Typography variant="body2" sx={{ color: 'text.secondary', mt: -0.5 }}>
              Compétition League of Legends entre amis
            </Typography>
          </Box>
        </Box>

        {/* Section droite avec infos utilisateur */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Chip
            icon={<MenuBook />}
            label="Season 2025"
            size="medium"
            sx={{
              background: 'rgba(200,155,60,0.2)',
              color: 'primary.main',
              border: '1px solid rgba(200,155,60,0.3)',
              fontWeight: 600
            }}
          />

          {/* Menu utilisateur */}
          {user && (
            <>
              <Button
                onClick={handleClick}
                startIcon={<Person />}
                endIcon={<ExpandMore />}
                sx={{
                  color: '#F0E6D2',
                  textTransform: 'none',
                  background: 'rgba(200,155,60,0.1)',
                  border: '1px solid rgba(200,155,60,0.3)',
                  borderRadius: 2,
                  px: 2,
                  '&:hover': {
                    background: 'rgba(200,155,60,0.2)'
                  }
                }}
              >
                {user.firstName} {user.lastName}
              </Button>

              <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{
                  vertical: 'bottom',
                  horizontal: 'right',
                }}
                transformOrigin={{
                  vertical: 'top',
                  horizontal: 'right',
                }}
                PaperProps={{
                  sx: {
                    background: 'rgba(30, 35, 40, 0.95)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(200,155,60,0.2)',
                    borderRadius: 2,
                    mt: 1,
                    minWidth: 200
                  }
                }}
              >
                <MenuItem onClick={handleClose} sx={{ color: '#F0E6D2' }}>
                  <Settings sx={{ mr: 2, fontSize: 20 }} />
                  Paramètres
                </MenuItem>
                <Divider sx={{ borderColor: 'rgba(200,155,60,0.2)' }} />
                <MenuItem 
                  onClick={handleLogout} 
                  sx={{ 
                    color: '#F0E6D2',
                    '&:hover': {
                      background: 'rgba(231, 76, 60, 0.1)'
                    }
                  }}
                >
                  <Logout sx={{ mr: 2, fontSize: 20 }} />
                  Déconnexion
                </MenuItem>
              </Menu>
            </>
          )}
        </Box>
      </Toolbar>
    </AppBar>
  )
}

export default Header