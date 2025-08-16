# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.
Always update all of the ReadMes when you do an update on anything.

## Project Overview

This is a full-stack League of Legends SoloQ Challenge application with three main components:
- **Frontend**: React 19 + Vite application (`LZGChallenge2.App/`)
- **Backend**: .NET 9 Web API (`LZGChallenge2.Api/`)
- **Discord Bot**: .NET 9 Console Application (`LZGChallenge2.DiscordBot/`)

## Contexte de l'application

Compétition « SoloQ Challenge » entre amis sur League of Legends, environ dix joueurs. Seule la Solo Queue est comptée (queueId 420). Période indicative d'un mois, mais l'outil doit fonctionner sans dépendre strictement d'une date de fin. Suivi strictement individuel.

### Fonctionnalités principales :
- **Ajout automatisé** : Lors de l'ajout d'un participant, récupération automatique de l'historique des matches
- **Statistiques temps réel** : Participants, parties totales, winrate moyen, leader actuel
- **Leaderboard dynamique** : Classement avec tri par LP, winrate, KDA
- **Statistiques détaillées** : Par champion, rôle, historique des matches
- **Notifications temps réel** : SignalR pour les mises à jour automatiques
- **Bot Discord** : Commandes pour consulter le leaderboard sans ouvrir le site

## Development Commands

### Frontend (React + Vite)
Run from `LZGChallenge2.App/` directory:
- **Development server**: `pnpm dev` - Starts Vite dev server with HMR on default port
- **Build**: `pnpm build` - Creates production build in `dist/`
- **Lint**: `pnpm lint` - Runs ESLint on all JS/JSX files
- **Preview**: `pnpm preview` - Preview production build locally

### Backend (.NET API)
Run from `LZGChallenge2.Api/` directory:
- **Development server**: `dotnet run` - Starts API on https://localhost:44393
- **Build**: `dotnet build` - Compiles the application
- **Restore packages**: `dotnet restore` - Restores NuGet packages

### Discord Bot (.NET Console)
Run from `LZGChallenge2.DiscordBot/` directory:
- **Development server**: `dotnet run` - Starts Discord bot
- **Build**: `dotnet build` - Compiles the application
- **Restore packages**: `dotnet restore` - Restores NuGet packages

### API Testing
- HTTP client requests are available in `LZGChallenge2.Api.http`
- Key endpoints: 
  - `GET https://localhost:44393/api/leaderboard` - Current leaderboard
  - `GET https://localhost:44393/api/leaderboard/summary` - Statistics summary
  - `POST https://localhost:44393/api/players` - Add new participant

## Architecture

### Tech Stack
- **Frontend**: React 19.1.1, Vite 7.1.2, Material-UI, SignalR Client, Framer Motion
- **Backend**: .NET 9, ASP.NET Core Web API, MongoDB, SignalR, OpenAPI/Swagger
- **Discord Bot**: .NET 9 Console App, Discord.NET, MongoDB
- **Database**: MongoDB Atlas
- **External APIs**: Riot Games API
- **Package Managers**: pnpm (frontend), NuGet (backend)

### Project Structure
```
LZGChallenge2/
├── LZGChallenge2.App/              # React frontend
│   ├── src/
│   │   ├── main.jsx                # Application entry point
│   │   ├── App.jsx                 # Main App component with SignalR
│   │   ├── components/             # React components
│   │   │   ├── Leaderboard.jsx     # Main leaderboard display
│   │   │   ├── StatsCards.jsx      # Challenge statistics cards
│   │   │   ├── AddPlayerForm.jsx   # Add participant modal
│   │   │   ├── ChampionModal.jsx   # Champion statistics modal
│   │   │   └── MatchHistoryModal.jsx # Match history modal
│   │   ├── services/
│   │   │   └── api.js              # API service layer
│   │   └── utils/                  # Utility functions
│   ├── package.json                # Frontend dependencies
│   ├── vite.config.js              # Vite configuration
│   └── eslint.config.js            # ESLint configuration
├── LZGChallenge2.Api/              # .NET Web API
│   ├── Controllers/
│   │   ├── LeaderboardController.cs # Leaderboard and summary endpoints
│   │   ├── PlayersController.cs    # Player CRUD operations
│   │   ├── ChampionStatsController.cs # Champion statistics
│   │   └── MatchesController.cs    # Match history
│   ├── Services/
│   │   ├── RiotApiService.cs       # Riot Games API integration
│   │   ├── MatchUpdateService.cs   # Match data synchronization
│   │   └── SeasonService.cs        # Season management
│   ├── Models/                     # Data models
│   ├── DTOs/                       # Data transfer objects
│   ├── Data/                       # MongoDB context
│   ├── Hubs/
│   │   └── LeaderboardHub.cs       # SignalR hub
│   ├── Program.cs                  # API entry point
│   └── appsettings.json            # Configuration
└── LZGChallenge2.DiscordBot/       # Discord Bot
    ├── Commands/                   # Discord slash commands
    ├── Services/
    │   ├── DiscordBotService.cs    # Main bot service
    │   └── NotificationService.cs  # Discord notifications
    ├── Program.cs                  # Bot entry point
    └── appsettings.json            # Bot configuration
```

### Database Schema (MongoDB)
- **Players**: Player information and Riot API data
- **PlayerStats**: Aggregated statistics per player
- **Matches**: Individual match records (SoloQ only)
- **ChampionStats**: Statistics grouped by champion per player
- **RoleStats**: Statistics grouped by role per player

### API Endpoints
- **Leaderboard**: `/api/leaderboard` - Main leaderboard with sorting
- **Summary**: `/api/leaderboard/summary` - Challenge statistics overview
- **Players**: `/api/players` - CRUD operations for participants
- **Champion Stats**: `/api/championstats/{playerId}` - Player's champion performance
- **Matches**: `/api/matches/{playerId}` - Player's match history

### SignalR Events
- **PlayerAdded**: New participant added to challenge
- **PlayerRemoved**: Participant removed from challenge
- **PlayerUpdated**: Participant's data refreshed

### Configuration
- **CORS**: Configured for frontend ports (5173, 5174, 5175)
- **MongoDB**: Atlas connection string in appsettings
- **Riot API**: API key required in configuration
- **Discord Bot**: Token configured via environment variables
- **HTTPS**: Development certificates for local HTTPS

### Security Notes
- **Discord Token**: Stored in environment variables, NOT in appsettings.json
- **API Keys**: Riot API key should be rotated regularly
- **CORS**: Only allows specific localhost ports for development
- **MongoDB**: Uses connection string with authentication

## Development Workflow

### Adding New Participants
1. User clicks "Add Player" button in frontend
2. Modal opens with form (GameName#TagLine, Region)
3. API validates player exists on Riot Games
4. **Automatic data population**: 
   - Current rank and LP from Riot API
   - Complete match history retrieval (last 20 matches)
   - Champion statistics calculation
   - Role distribution analysis
5. SignalR notification sent to all connected clients
6. Frontend updates in real-time

### Data Synchronization
- **Manual refresh**: Players can refresh individual participant data
- **Bulk refresh**: Refresh all participants via API or Discord command
- **Automatic updates**: New matches are detected and stats recalculated
- **Real-time updates**: SignalR pushes changes to all connected clients

### Discord Bot Commands
- **!leaderboard**: Display current standings
- **!stats [player]**: Show specific player statistics
- **!refresh**: Trigger data refresh for all players

## Development Notes
- **MongoDB required**: Application uses MongoDB Atlas for data persistence
- **Riot API key**: Must be valid and have sufficient rate limits
- **SignalR**: Real-time communication between API and frontend
- **Material-UI**: Consistent theming with League of Legends aesthetic
- **Error handling**: Robust error handling for API failures and network issues
- **Rate limiting**: Riot API calls are rate-limited to respect API constraints

## Deployment Considerations
- **Environment Variables**: Discord token, MongoDB connection string
- **HTTPS**: Required for production SignalR connections
- **CORS**: Update allowed origins for production domain
- **API Keys**: Secure storage of Riot API key
- **Database**: MongoDB Atlas or self-hosted MongoDB instance

## Recent Fixes and Improvements
- ✅ Fixed missing `/api/leaderboard/summary` endpoint
- ✅ Automated data retrieval on participant addition
- ✅ Fixed participant deletion with proper SignalR notifications
- ✅ Corrected SignalR event names and property mappings
- ✅ Added comprehensive error handling and logging