# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a full-stack application with two main components:
- **Frontend**: React 19 + Vite application (`LZGChallenge2.App/`)
- **Backend**: .NET 9 Web API (`LZGChallenge2.Api/`)

## Contexte de l’application

Compétition « SoloQ Challenge » entre amis sur League of Legends, environ dix joueurs. Seule la Solo Queue est comptée (queueId 420). Période indicative d’un mois, mais l’outil doit fonctionner sans dépendre strictement d’une date de fin. Suivi strictement individuel. Indicateurs clés suivis : rang et LP, winrate global, KDA moyen, winrate par champion, répartition par rôle, progression quotidienne, séries de victoires et de défaites. Mise à jour en quasi temps réel : détection de fin de partie puis recalcul des agrégats, push aux clients connectés. Bot Discord optionnel pour consulter le leaderboard et les stats sans ouvrir le site.

## Development Commands

### Frontend (React + Vite)
Run from `LZGChallenge2.App/` directory:
- **Development server**: `pnpm dev` - Starts Vite dev server with HMR on default port
- **Build**: `pnpm build` - Creates production build in `dist/`
- **Lint**: `pnpm lint` - Runs ESLint on all JS/JSX files
- **Preview**: `pnpm preview` - Preview production build locally

### Backend (.NET API)
Run from `LZGChallenge2.Api/` directory:
- **Development server**: `dotnet run` - Starts API on https://localhost:44393 and https://localhost:7255
- **Build**: `dotnet build` - Compiles the application
- **Restore packages**: `dotnet restore` - Restores NuGet packages

### API Testing
- HTTP client requests are available in `LZGChallenge2.Api.http`
- Default weather forecast endpoint: `GET https://localhost:44393/weatherforecast/`

## Architecture

### Tech Stack
- **Frontend**: React 19.1.1, Vite 7.1.2, ESLint
- **Backend**: .NET 9, ASP.NET Core Web API, OpenAPI/Swagger
- **Package Managers**: pnpm (frontend), NuGet (backend)

### Project Structure
```
LZGChallenge2/
├── LZGChallenge2.App/          # React frontend
│   ├── src/
│   │   ├── main.jsx            # Application entry point
│   │   ├── App.jsx             # Main App component
│   │   └── assets/             # Static assets
│   ├── package.json            # Frontend dependencies
│   ├── vite.config.js          # Vite configuration
│   └── eslint.config.js        # ESLint configuration
└── LZGChallenge2.Api/          # .NET Web API
    ├── Controllers/
    │   └── WeatherForecastController.cs
    ├── Program.cs              # API entry point
    ├── LZGChallenge2.Api.csproj # .NET project file
    ├── LZGChallenge2.Api.http  # HTTP client requests
    └── Properties/
        └── launchSettings.json # Launch configuration
```

### API Configuration
- **Development URLs**: HTTP (44393), HTTPS (7255)
- **OpenAPI**: Available in development environment
- **CORS**: Not configured (may need setup for frontend integration)
- **Authentication**: Not implemented

### Frontend Configuration
- **Package Manager**: Uses pnpm (evidenced by `pnpm-lock.yaml`)
- **ESLint**: Configured with React hooks and refresh plugins
- **Custom Rule**: `no-unused-vars` allows variables starting with uppercase/underscores
- **Build Output**: `dist/` directory (ignored by ESLint)

## Development Notes
- No test frameworks are currently configured for either frontend or backend
- No TypeScript configured (pure JavaScript frontend, C# backend)
- Default template controllers and components are still in place
- CORS configuration will be needed to connect frontend to backend