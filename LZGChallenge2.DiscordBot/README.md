# LZG Challenge 2 - Bot Discord

Bot Discord pour consulter et gérer les compétitions SoloQ League of Legends avec commandes intuitives et notifications automatiques.

## 🎮 Aperçu

Le bot Discord complète l'application web en permettant aux participants de :
- Consulter le leaderboard directement sur Discord
- Voir leurs statistiques individuelles
- Être notifiés des mises à jour importantes
- Interagir avec le challenge sans ouvrir le site web

## 🚀 Installation et Configuration

### Prérequis
- **.NET 9 SDK**
- **Application Discord** créée sur https://discord.com/developers/applications
- **MongoDB Atlas** (même base que l'API)
- **Clé API Riot Games**

### 1. Configuration Discord

#### Créer l'Application Discord
1. Allez sur https://discord.com/developers/applications
2. Créez une nouvelle application
3. Dans l'onglet "Bot" :
   - Créez un bot
   - Copiez le token (gardez-le secret !)
   - Activez les intents nécessaires

#### Permissions Requises
Le bot nécessite les permissions suivantes :
- **Lire les messages** - Pour recevoir les commandes
- **Envoyer des messages** - Pour répondre aux commandes
- **Utiliser les commandes slash** - Pour les commandes modernes
- **Intégrer des liens** - Pour afficher les embeds
- **Mentionner tout le monde** - Pour les notifications

### 2. Configuration du Token

**⚠️ IMPORTANT** : Le token Discord doit être configuré en **variable d'environnement** pour la sécurité.

#### Windows (PowerShell)
```powershell
$env:Discord__BotToken="VOTRE_TOKEN_DISCORD"
```

#### Windows (CMD)
```cmd
set Discord__BotToken=VOTRE_TOKEN_DISCORD
```

#### Linux/macOS
```bash
export Discord__BotToken="VOTRE_TOKEN_DISCORD"
```

### 3. Configuration MongoDB

Le bot utilise la même base de données MongoDB que l'API. Configurez dans `appsettings.json` :

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://user:pass@cluster.mongodb.net/",
    "DatabaseName": "LZGChallenge2Db"
  }
}
```

### 4. Configuration Riot API

```json
{
  "RiotApi": {
    "ApiKey": "RGAPI-votre-cle-api",
    "BaseUrl": "https://euw1.api.riotgames.com",
    "RegionalUrl": "https://europe.api.riotgames.com",
    "RateLimitPerSecond": 20,
    "RateLimitPer2Minutes": 100
  }
}
```

## 🤖 Commandes Disponibles

### 📊 Consultation des Données

#### Leaderboard
- `!leaderboard` ou `!lb` ou `!classement`
  - Affiche le classement complet par LP
  - Montre rang, LP, winrate et KDA de chaque participant

#### Statistiques Individuelles
- `!stats <joueur>` 
  - Statistiques complètes d'un participant
  - Rang actuel, LP, winrate, KDA, séries de victoires/défaites
  
- `!champions <joueur>`
  - Top champions du joueur avec winrate et KDA
  - Performance détaillée par champion

- `!progress <joueur>`
  - Progression quotidienne et évolution du rang
  - Historique des gains/pertes de LP

#### Analyses par Rôle
- `!roles <joueur>` ou `!rôles <joueur>`
  - Répartition par rôle (TOP, JUNGLE, MID, ADC, SUPPORT)
  - Performance dans chaque position

- `!mainrole <joueur>` ou `!main <joueur>`
  - Rôle principal et statistiques associées

- `!roleleaderboard <rôle>` ou `!rlb <rôle>`
  - Classement spécifique par rôle
  - Exemple : `!rlb jungle` pour le classement jungle

### 🔄 Mise à Jour des Données

**⚠️ NOTE IMPORTANTE** : Depuis la dernière mise à jour de l'application, l'ajout de nouveaux participants via l'interface web déclenche automatiquement la récupération des données. La commande `!refresh` reste disponible pour les mises à jour manuelles.

- `!live` ou `!refresh` ou `!update`
  - Met à jour toutes les données de tous les participants
  - Récupère les derniers matches et recalcule les statistiques

## 🔔 Notifications Automatiques

Le bot envoie automatiquement :

### Achievements
- **Séries de victoires** importantes (5+ wins streak)
- **KDA exceptionnels** (8.0+)
- **Promotions de rang**
- **Records personnels**

### Récapitulatifs
- **Récap quotidien** à 20h avec les performances du jour
- **Classement mis à jour** après refresh global
- **Nouveaux participants** ajoutés au challenge

## 🚀 Démarrage

### Démarrage Local
```bash
cd LZGChallenge2.DiscordBot

# Configurer le token Discord
set Discord__BotToken=VOTRE_TOKEN

# Démarrer le bot
dotnet run
```

### Vérification du Fonctionnement
1. Le bot doit apparaître en ligne sur votre serveur Discord
2. Testez avec `!lb` pour voir le leaderboard
3. Vérifiez les logs dans la console pour d'éventuelles erreurs

## 🔧 Configuration Avancée

### Variables d'Environnement de Production
```bash
# Token Discord (OBLIGATOIRE)
Discord__BotToken=MTQw...

# MongoDB (si différent de appsettings.json)
MongoDB__ConnectionString=mongodb+srv://...

# Riot API (si différent de appsettings.json)  
RiotApi__ApiKey=RGAPI-...
```

### Configuration des Canaux (Optionnel)
```json
{
  "Discord": {
    "NotificationChannelId": 123456789012345678,
    "DailyRecapChannelId": 123456789012345678
  }
}
```

## 🐛 Dépannage

### Problèmes Courants

**Le bot n'apparaît pas en ligne**
- Vérifiez que le token Discord est correct
- Assurez-vous que les intents sont activés dans le portail Discord
- Contrôlez les permissions du bot sur votre serveur

**Commandes ne fonctionnent pas**
- Vérifiez que le bot a les permissions "Lire les messages" et "Envoyer des messages"
- Testez d'abord avec `!ping` ou `!help`

**Erreurs de base de données**
- Vérifiez la connection string MongoDB
- Assurez-vous que la base est accessible depuis votre réseau
- Contrôlez que l'API fonctionne correctement

**Données obsolètes**
- Utilisez `!refresh` pour forcer une mise à jour
- Vérifiez que l'API Riot fonctionne (rate limits)

## 🔒 Sécurité

### Bonnes Pratiques
- **Token Discord** : JAMAIS dans le code source, toujours en variable d'environnement
- **Clé API Riot** : Rotation régulière recommandée
- **Permissions** : Principe du moindre privilège
- **Logs** : Surveillance des erreurs et tentatives d'accès

### Production
- Utilisez `appsettings.Production.json` pour la config de prod
- Activez les logs de sécurité
- Surveillez l'utilisation des APIs (rate limits)

## 📦 Dépendances

### Framework Principal
- **.NET 9** - Runtime principal
- **Discord.Net 3.16.0** - Client Discord
- **Microsoft.Extensions.Hosting 9.0.8** - Service hosting

### Base de Données
- **MongoDB.Driver** - Client MongoDB
- **Microsoft.Extensions.Configuration** - Configuration

### Intégrations
- **Riot Games API** - Via services partagés avec l'API
- **SignalR Client** - Pour synchronisation temps réel (optionnel)

---

**Bot Discord développé pour LZG Challenge 2** 🎮