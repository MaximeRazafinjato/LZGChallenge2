# LZG Challenge 2 - League of Legends SoloQ Competition

Une application full-stack pour organiser et suivre des compétitions SoloQ entre amis sur League of Legends.

## 🎮 Aperçu

LZG Challenge 2 est une plateforme complète permettant de :
- Organiser des compétitions SoloQ entre amis
- Suivre en temps réel les statistiques des participants
- Consulter des leaderboards dynamiques
- Analyser les performances par champion et rôle
- Recevoir des notifications Discord

## ✨ Fonctionnalités

### 🏆 Statistiques en Temps Réel
- **Participants actifs** : Nombre de joueurs dans le challenge
- **Parties totales** : Compteur global des matches joués
- **Winrate moyen** : Performance moyenne de tous les participants
- **Leader actuel** : Joueur en tête du classement

### 📊 Leaderboard Dynamique
- Classement par LP, winrate, ou KDA
- Mise à jour automatique via SignalR
- Informations détaillées : rang, points, séries de victoires/défaites

### 🔍 Analyses Détaillées
- **Statistiques par champion** : Performance avec chaque champion joué
- **Répartition par rôle** : Analyse des performances selon le rôle
- **Historique des matches** : Liste complète des parties SoloQ avec détails

### ⚡ Ajout Automatisé
- Récupération automatique de l'historique des matches lors de l'ajout
- Calcul immédiat des statistiques complètes
- Plus besoin de refresh manuel après ajout

### 🤖 Bot Discord (Optionnel)
- Consultation du leaderboard sans ouvrir le site
- Commandes pour voir les statistiques individuelles
- Notifications en temps réel

## 🏗️ Architecture

### Frontend (React + Vite)
- **React 19** avec Material-UI pour l'interface
- **SignalR Client** pour les mises à jour temps réel
- **Framer Motion** pour les animations
- **Vite** pour le développement rapide

### Backend (.NET 9 API)
- **ASP.NET Core Web API** avec MongoDB
- **SignalR Hub** pour la communication temps réel
- **Riot Games API** pour récupérer les données de jeu
- **Architecture service-orientée** avec injection de dépendance

### Base de Données (MongoDB)
- **Players** : Informations des participants
- **Matches** : Historique des parties SoloQ
- **PlayerStats** : Statistiques agrégées
- **ChampionStats** : Performance par champion
- **RoleStats** : Performance par rôle

## 🚀 Installation et Démarrage

### Prérequis
- **.NET 9 SDK**
- **Node.js 18+** avec **pnpm**
- **MongoDB Atlas** (ou instance locale)
- **Clé API Riot Games**

### 1. Cloner le Projet
```bash
git clone [repository-url]
cd LZGChallenge2
```

### 2. Configuration Backend
```bash
cd LZGChallenge2.Api
dotnet restore

# Configurer appsettings.json avec :
# - MongoDB connection string
# - Riot API key
```

### 3. Démarrage Backend
```bash
dotnet run
# API disponible sur https://localhost:44393
```

### 4. Configuration Frontend
```bash
cd ../LZGChallenge2.App
pnpm install
```

### 5. Démarrage Frontend
```bash
pnpm dev
# Interface disponible sur http://localhost:5173
```

### 6. Bot Discord (Optionnel)
```bash
cd ../LZGChallenge2.DiscordBot
dotnet restore

# Configurer le token Discord en variable d'environnement
set Discord__BotToken=votre_token_discord

dotnet run
```

## 📝 Configuration

### Variables d'Environnement
```bash
# Discord Bot
Discord__BotToken=MTQw...

# MongoDB (si différent de appsettings.json)
MongoDB__ConnectionString=mongodb+srv://...
```

### appsettings.json
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://...",
    "DatabaseName": "LZGChallenge2Db"
  },
  "RiotApi": {
    "ApiKey": "RGAPI-...",
    "BaseUrl": "https://euw1.api.riotgames.com",
    "RegionalUrl": "https://europe.api.riotgames.com"
  }
}
```

## 🎯 Utilisation

### Ajouter un Participant
1. Cliquer sur le bouton **"Ajouter un joueur"** (➕)
2. Saisir le **GameName#TagLine** (ex: "Faker#KR1")
3. Sélectionner la **région** (EUW1 par défaut)
4. **Automatiquement** : récupération de l'historique et calcul des stats

### Consulter les Statistiques
- **Cartes statistiques** : Vue d'ensemble en haut de page
- **Leaderboard** : Classement principal avec tri personnalisable
- **Détails joueur** : Clic sur un joueur pour voir champion/rôle stats
- **Historique** : Bouton "Historique" pour voir toutes les parties

### Mise à Jour des Données
- **Automatique** : Nouvelles données récupérées à l'ajout
- **Manuel** : Bouton "Actualiser" pour refresh global
- **Temps réel** : Mises à jour automatiques via SignalR

## 📋 API Endpoints

### Leaderboard
- `GET /api/leaderboard` - Classement principal
- `GET /api/leaderboard/summary` - Statistiques du challenge

### Participants
- `GET /api/players` - Liste des participants
- `POST /api/players` - Ajouter un participant (avec auto-refresh)
- `DELETE /api/players/{id}` - Supprimer un participant

### Statistiques
- `GET /api/championstats/{playerId}` - Stats par champion
- `GET /api/matches/{playerId}` - Historique des matches

## 🔧 Développement

### Structure du Projet
```
LZGChallenge2/
├── LZGChallenge2.App/          # Frontend React
├── LZGChallenge2.Api/          # Backend .NET
├── LZGChallenge2.DiscordBot/   # Bot Discord
├── CLAUDE.md                   # Guide pour Claude Code
└── README.md                   # Ce fichier
```

### Scripts de Développement
```bash
# Frontend
pnpm dev          # Serveur de développement
pnpm build        # Build de production
pnpm lint         # Linting ESLint

# Backend
dotnet run        # Démarrage API
dotnet build      # Compilation
dotnet test       # Tests (si configurés)
```

## 🐛 Dépannage

### Problèmes Courants

**"Joueur introuvable sur Riot Games"**
- Vérifier le format : `GameName#TagLine`
- Vérifier que la clé API Riot est valide
- S'assurer que le joueur existe dans la région sélectionnée

**"Erreur de connexion à la base de données"**
- Vérifier la connection string MongoDB
- S'assurer que l'IP est autorisée sur MongoDB Atlas

**"Les statistiques ne s'affichent pas"**
- Vérifier que l'API fonctionne sur https://localhost:44393
- Contrôler les erreurs CORS dans la console du navigateur
- S'assurer que SignalR se connecte correctement

## 🔒 Sécurité

- **Tokens Discord** : Stockés en variables d'environnement uniquement
- **Clés API** : Rotation régulière recommandée
- **CORS** : Configuré pour localhost en développement
- **HTTPS** : Requis pour les connexions SignalR en production

## 📄 Licence

Ce projet est un outil personnel pour organiser des compétitions entre amis. Non destiné à un usage commercial.

## 🤝 Contribution

Projet personnel - contributions bienvenues via issues et pull requests.

---

**Développé avec** ❤️ **pour la communauté League of Legends**