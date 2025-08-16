# LZG Challenge 2 - API Backend

API REST .NET 9 pour la gestion des compétitions SoloQ League of Legends avec SignalR pour les mises à jour temps réel.

## 🏗️ Architecture

### Technologies
- **.NET 9** - Framework principal
- **ASP.NET Core Web API** - API REST
- **MongoDB** - Base de données NoSQL
- **SignalR** - Communication temps réel
- **Riot Games API** - Récupération des données de jeu

### Structure du Projet
```
LZGChallenge2.Api/
├── Controllers/
│   ├── LeaderboardController.cs    # Classement et statistiques
│   ├── PlayersController.cs        # Gestion des participants
│   ├── ChampionStatsController.cs  # Statistiques par champion
│   └── MatchesController.cs        # Historique des matches
├── Services/
│   ├── RiotApiService.cs          # Interface Riot Games API
│   ├── MatchUpdateService.cs      # Synchronisation des données
│   └── SeasonService.cs           # Gestion des saisons
├── Models/                        # Modèles de données MongoDB
├── DTOs/                          # Data Transfer Objects
├── Data/
│   └── MongoDbContext.cs          # Contexte MongoDB
├── Hubs/
│   └── LeaderboardHub.cs          # Hub SignalR
├── Options/                       # Configuration
└── Program.cs                     # Point d'entrée
```

## 🚀 Démarrage Rapide

### Prérequis
- **.NET 9 SDK**
- **MongoDB Atlas** ou instance locale
- **Clé API Riot Games**

### Installation
```bash
cd LZGChallenge2.Api
dotnet restore
dotnet build
```

### Configuration
Créer/modifier `appsettings.json` :
```json
{
  \"MongoDB\": {
    \"ConnectionString\": \"mongodb+srv://user:pass@cluster.mongodb.net/\",
    \"DatabaseName\": \"LZGChallenge2Db\"
  },
  \"RiotApi\": {
    \"ApiKey\": \"RGAPI-your-api-key\",
    \"BaseUrl\": \"https://euw1.api.riotgames.com\",
    \"RegionalUrl\": \"https://europe.api.riotgames.com\",
    \"RateLimitPerSecond\": 20,
    \"RateLimitPer2Minutes\": 100
  }
}
```

### Démarrage
```bash
dotnet run
# API disponible sur https://localhost:44393
# OpenAPI/Swagger en développement
```

## 📋 Endpoints API

### 🏆 Leaderboard
| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/leaderboard` | GET | Classement principal avec tri |
| `/api/leaderboard/summary` | GET | Statistiques du challenge |
| `/api/leaderboard/stats` | GET | Statistiques de base |

**Paramètres de tri** : `?sortBy=lp|winrate|kda|games`

### 👥 Players (Participants)
| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/players` | GET | Liste des participants actifs |
| `/api/players/{id}` | GET | Détails d'un participant |
| `/api/players` | POST | Ajouter un participant (avec auto-refresh) |
| `/api/players/{id}` | PUT | Modifier un participant |
| `/api/players/{id}` | DELETE | Supprimer un participant |
| `/api/players/{id}/refresh` | POST | Actualiser les données d'un participant |
| `/api/players/refresh-all` | POST | Actualiser tous les participants |

### 📊 Champion Statistics
| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/championstats/{playerId}` | GET | Toutes les stats par champion |
| `/api/championstats/{playerId}/filtered` | GET | Stats filtrées par champion |

### 🎮 Matches
| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/api/matches/{playerId}` | GET | Historique des matches |
| `/api/matches/{playerId}/stats` | GET | Statistiques des matches |
| `/api/matches/season-info` | GET | Informations de la saison |

## 🔄 SignalR Events

### Hub : `/leaderboardHub`

**Events émis par l'API :**
- `PlayerAdded` : Nouveau participant ajouté
- `PlayerRemoved` : Participant supprimé  
- `PlayerUpdated` : Données d'un participant mises à jour
- `AllPlayersUpdated` : Refresh global terminé

**Connexion côté client :**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:44393/leaderboardHub')
  .build();

connection.on('PlayerAdded', (player) => {
  // Gérer l'ajout d'un joueur
});
```

## 💾 Modèles de Données MongoDB

### Players Collection
```csharp
{
  \"Id\": \"ObjectId\",
  \"RiotId\": \"GameName#TagLine\",
  \"GameName\": \"string\",
  \"TagLine\": \"string\",
  \"Puuid\": \"string\",
  \"SummonerId\": \"string\",
  \"Region\": \"string\",
  \"IsActive\": bool,
  \"JoinedAt\": DateTime
}
```

### PlayerStats Collection
```csharp
{
  \"PlayerId\": \"ObjectId\",
  \"CurrentTier\": \"DIAMOND|EMERALD|...\",
  \"CurrentRank\": \"I|II|III|IV\",
  \"CurrentLeaguePoints\": int,
  \"TotalGames\": int,
  \"TotalWins\": int,
  \"TotalLosses\": int,
  \"WinRate\": double,
  \"KDA\": double,
  \"AverageKills\": double,
  \"AverageDeaths\": double,
  \"AverageAssists\": double,
  \"LastUpdated\": DateTime
}
```

### Matches Collection
```csharp
{
  \"PlayerId\": \"ObjectId\",
  \"MatchId\": \"string\",
  \"GameStartTime\": DateTime,
  \"GameDuration\": int,
  \"Win\": bool,
  \"ChampionId\": int,
  \"ChampionName\": \"string\",
  \"Position\": \"TOP|JUNGLE|MIDDLE|BOTTOM|UTILITY\",
  \"Kills\": int,
  \"Deaths\": int,
  \"Assists\": int,
  \"CreepScore\": int,
  \"Season\": int,
  \"QueueId\": 420
}
```

## ⚙️ Services

### RiotApiService
Interface avec l'API Riot Games :
- Récupération des informations de compte
- Données de ranked SoloQ
- Historique des matches
- Rate limiting automatique

### MatchUpdateService
Synchronisation des données :
- Récupération des nouveaux matches
- Calcul des statistiques agrégées
- Mise à jour des collections MongoDB
- Gestion des erreurs et retry

### SeasonService
Gestion des saisons League of Legends :
- Détection de la saison actuelle
- Filtrage des matches par saison
- Gestion des transitions de saison

## 🔧 Configuration Avancée

### Rate Limiting Riot API
```json
{
  \"RiotApi\": {
    \"RateLimitPerSecond\": 20,
    \"RateLimitPer2Minutes\": 100
  }
}
```

### CORS Configuration
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(\"http://localhost:5173\", \"http://localhost:5174\", \"http://localhost:5175\")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### MongoDB Indexes
Indexes recommandés pour les performances :
```javascript
// Players
db.players.createIndex({ \"RiotId\": 1 }, { unique: true })
db.players.createIndex({ \"IsActive\": 1 })

// Matches
db.matches.createIndex({ \"PlayerId\": 1, \"GameStartTime\": -1 })
db.matches.createIndex({ \"MatchId\": 1 }, { unique: true })
db.matches.createIndex({ \"Season\": 1, \"QueueId\": 1 })

// PlayerStats
db.playerstats.createIndex({ \"PlayerId\": 1 }, { unique: true })
```

## 🔒 Sécurité

### Variables d'Environnement
Pour la production, utiliser des variables d'environnement :
```bash
MongoDB__ConnectionString=mongodb+srv://...
RiotApi__ApiKey=RGAPI-...
```

### Validation des Entrées
- Validation automatique des DTOs avec Data Annotations
- Sanitisation des entrées utilisateur
- Gestion des erreurs avec messages appropriés

## 📊 Monitoring et Logs

### Logging
Configuration dans `appsettings.json` :
```json
{
  \"Logging\": {
    \"LogLevel\": {
      \"Default\": \"Information\",
      \"Microsoft.AspNetCore\": \"Warning\",
      \"LZGChallenge2.Api.Services.RiotApiService\": \"Information\"
    }
  }
}
```

### Health Checks
Endpoints de santé disponibles :
- `/health` - Santé générale de l'API
- `/health/mongodb` - Connexion MongoDB
- `/health/riot` - Disponibilité API Riot

## 🧪 Tests

### Tests d'Intégration
```bash
# Tester tous les endpoints
dotnet test

# Tests spécifiques
dotnet test --filter \"Category=Integration\"
```

### Test Manuel avec HTTP Client
Utiliser `LZGChallenge2.Api.http` pour tester les endpoints :
```http
### Obtenir le leaderboard
GET https://localhost:44393/api/leaderboard

### Ajouter un participant
POST https://localhost:44393/api/players
Content-Type: application/json

{
  \"gameName\": \"TestPlayer\",
  \"tagLine\": \"EUW\",
  \"region\": \"EUW1\"
}
```

## 🚀 Déploiement

### Variables d'Environnement Requises
```bash
ASPNETCORE_ENVIRONMENT=Production
MongoDB__ConnectionString=mongodb+srv://...
RiotApi__ApiKey=RGAPI-...
```

### Configuration Production
- Activer HTTPS uniquement
- Configurer CORS pour le domaine de production
- Utiliser des secrets sécurisés pour les clés API
- Activer les logs de production

---

**API développée pour LZG Challenge 2** 🎮