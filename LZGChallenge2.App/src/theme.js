import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#C89B3C', // League of Legends gold
      light: '#F0E6D2',
      dark: '#B8860B',
    },
    secondary: {
      main: '#0F2027', // Dark blue
      light: '#463714',
      dark: '#000000',
    },
    background: {
      default: '#010A13',
      paper: '#1E2328',
    },
    success: {
      main: '#00F5FF', // Bright cyan for wins
    },
    error: {
      main: '#E74C3C', // Red for losses
    },
    warning: {
      main: '#F39C12', // Orange for warnings
    },
    info: {
      main: '#3498DB', // Blue for info
    },
    text: {
      primary: '#F0E6D2',
      secondary: '#C9AA71',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Arial", sans-serif',
    h1: {
      fontSize: '2.5rem',
      fontWeight: 700,
      color: '#C89B3C',
    },
    h2: {
      fontSize: '2rem',
      fontWeight: 600,
      color: '#F0E6D2',
    },
    h3: {
      fontSize: '1.5rem',
      fontWeight: 600,
      color: '#F0E6D2',
    },
    h4: {
      fontSize: '1.25rem',
      fontWeight: 500,
      color: '#C9AA71',
    },
    body1: {
      color: '#F0E6D2',
    },
    body2: {
      color: '#C9AA71',
    },
  },
  shape: {
    borderRadius: 12,
  },
  components: {
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundImage: 'linear-gradient(145deg, #1E2328 0%, #3C3C41 100%)',
          border: '1px solid #463714',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.3)',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          borderRadius: 8,
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 600,
        },
      },
    },
  },
});

export default theme;