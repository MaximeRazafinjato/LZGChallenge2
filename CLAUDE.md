# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.
**Always update all ReadMes when you change something in the project.**

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

### Frontend (`LZGChallenge2.App/`)
- `pnpm dev` → Start dev server (Vite HMR, port 5173+)  
- `pnpm build` → Production build in `dist/`  
- `pnpm lint` → Run ESLint  
- `pnpm preview` → Preview prod build  

### Backend (`LZGChallenge2.Api/`)
- `dotnet run` → Start API (`https://localhost:44393`)  
- `dotnet build` → Build the API  
- `dotnet restore` → Restore NuGet packages  

### Discord Bot (`LZGChallenge2.DiscordBot/`)
- `dotnet run` → Start Discord bot  
- `dotnet build` / `dotnet restore` → Standard build & restore 

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
