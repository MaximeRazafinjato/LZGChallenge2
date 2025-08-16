# LZGChallenge2 Discord Bot

Bot Discord pour le SoloQ Challenge League of Legends.

## Configuration

### 1. Token Discord

1. Créez une application Discord sur https://discord.com/developers/applications
2. Créez un bot dans l'onglet "Bot"
3. Copiez le token et ajoutez-le dans `appsettings.json`:

```json
{
  "Discord": {
    "BotToken": "VOTRE_TOKEN_ICI"
  }
}
```

### 2. Configuration des canaux

Ajoutez les IDs des canaux Discord dans `appsettings.json`:

```json
{
  "Discord": {
    "BotToken": "VOTRE_TOKEN_ICI",
    "NotificationChannelId": 123456789012345678,
    "DailyRecapChannelId": 123456789012345678
  }
}
```

Pour obtenir l'ID d'un canal:
1. Activez le mode développeur dans Discord (Paramètres utilisateur > Avancé > Mode développeur)
2. Clic droit sur le canal > Copier l'ID

### 3. Base de données

Le bot utilise la même base de données que l'API. Assurez-vous que la chaîne de connexion est correcte dans `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LZGChallenge2;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. API Riot Games

Configurez votre clé API Riot Games:

```json
{
  "RiotApi": {
    "ApiKey": "VOTRE_CLE_API_RIOT",
    "BaseUrl": "https://euw1.api.riotgames.com",
    "RegionalUrl": "https://europe.api.riotgames.com"
  }
}
```

## Permissions Discord

Le bot nécessite les permissions suivantes:
- Lire les messages
- Envoyer des messages
- Utiliser les commandes slash
- Gérer les messages
- Mentionner tout le monde

## Commandes disponibles

### Commandes de consultation

- `!leaderboard` ou `!lb` ou `!classement` - Affiche le classement des joueurs
- `!stats <joueur>` - Affiche les statistiques d'un joueur
- `!champions <joueur>` - Top champions d'un joueur
- `!progress <joueur>` - Progression quotidienne d'un joueur

### Commandes de mise à jour

- `!live` ou `!refresh` ou `!update` - Met à jour toutes les données

## Notifications automatiques

Le bot envoie automatiquement:
- Notifications d'achievements (séries de victoires, KDA élevé)
- Récapitulatif quotidien à 20h

## Démarrage

```bash
cd LZGChallenge2.DiscordBot
dotnet run
```

## Déploiement

Pour un déploiement en production, configurez les variables d'environnement ou utilisez `appsettings.Production.json`.

## Dépendances

- Discord.Net 3.16.0
- Microsoft.Extensions.Hosting 9.0.8
- Microsoft.EntityFrameworkCore.SqlServer 9.0.8