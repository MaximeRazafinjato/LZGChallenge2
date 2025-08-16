# LZG Challenge - Service de Mise à Jour Automatique

Service console .NET 9 pour mettre à jour automatiquement les données des participants en arrière-plan.

## 🎯 Objectif

Ce service est conçu pour être exécuté en tant que **job périodique** sur un serveur (cron, Task Scheduler, etc.) pour maintenir les données à jour sans intervention manuelle.

## ⚡ Fonctionnalités

- **Mise à jour automatique** de tous les participants actifs
- **Logs détaillés** pour suivi et débogage
- **Gestion d'erreurs robuste** avec continuation en cas d'échec individuel
- **Configuration flexible** via appsettings.json et variables d'environnement
- **Appels API optimisés** vers l'API principale
- **Exit codes** appropriés pour monitoring

## 🚀 Installation et Configuration

### Prérequis
- **.NET 9 SDK**
- **Accès MongoDB** (même base que l'API)
- **API LZG Challenge** en fonctionnement sur `https://localhost:44393`

### Configuration

#### appsettings.json
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://user:pass@cluster.mongodb.net/",
    "DatabaseName": "LZGChallenge2Db"
  },
  "RiotApi": {
    "ApiKey": "RGAPI-your-api-key",
    "BaseUrl": "https://euw1.api.riotgames.com",
    "RegionalUrl": "https://europe.api.riotgames.com",
    "RateLimitPerSecond": 20,
    "RateLimitPer2Minutes": 100
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

#### Variables d'Environnement (Production)
```bash
# MongoDB (prioritaire sur appsettings.json)
MongoDB__ConnectionString=mongodb+srv://...

# Riot API (si différent)
RiotApi__ApiKey=RGAPI-...
```

## 🔧 Utilisation

### Exécution Manuelle
```bash
cd LZGChallenge2.UpdateService
dotnet run
```

### Build pour Production
```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Exécution en Production
```bash
cd publish
dotnet LZGChallenge2.UpdateService.dll
```

## ⏰ Configuration en Tant que Job

### Windows - Task Scheduler

#### Créer une Tâche
1. Ouvrir **Task Scheduler** (`taskschd.msc`)
2. Créer une tâche de base
3. **Nom** : "LZG Challenge Update"
4. **Déclencheur** : Répéter toutes les 5 minutes

#### Configuration
- **Programme** : `dotnet.exe`
- **Arguments** : `"C:\\path\\to\\LZGChallenge2.UpdateService.dll"`
- **Répertoire de démarrage** : `"C:\\path\\to\\publish"`
- **Utilisateur** : Compte de service ou utilisateur système

#### Planification Recommandée
```
Démarrer : Tous les jours à 00:00
Répéter toutes les : 5 minutes
Pendant : 1 jour
```

### Linux - Cron Job

#### Installation du Service
```bash
# Copier les fichiers
sudo cp -r ./publish /opt/lzg-challenge-update/
sudo chown -R lzg-service:lzg-service /opt/lzg-challenge-update/

# Créer le script de lancement
sudo tee /usr/local/bin/lzg-update.sh << 'EOF'
#!/bin/bash
cd /opt/lzg-challenge-update
/usr/bin/dotnet LZGChallenge2.UpdateService.dll
EOF

sudo chmod +x /usr/local/bin/lzg-update.sh
```

#### Configuration Cron
```bash
# Éditer la crontab
sudo crontab -e

# Ajouter la ligne (toutes les 5 minutes)
*/5 * * * * /usr/local/bin/lzg-update.sh >> /var/log/lzg-update.log 2>&1
```

### Docker (Recommandé pour Production)

#### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "LZGChallenge2.UpdateService.dll"]
```

#### docker-compose.yml
```yaml
version: '3.8'
services:
  lzg-update:
    build: .
    environment:
      - MongoDB__ConnectionString=mongodb+srv://...
      - RiotApi__ApiKey=RGAPI-...
    restart: no  # Important : ne pas redémarrer automatiquement
    depends_on:
      - lzg-api
```

#### Exécution Périodique avec Cron
```bash
# Script pour Docker
#!/bin/bash
docker-compose run --rm lzg-update

# Crontab
*/5 * * * * /path/to/docker-update.sh >> /var/log/lzg-update.log 2>&1
```

## 📊 Monitoring et Logs

### Logs de Sortie
Le service produit des logs structurés :

```
[2024-01-15 14:30:00] Information: === LZG Challenge Update Service - Démarrage ===
[2024-01-15 14:30:01] Information: Début de la mise à jour automatique de tous les joueurs
[2024-01-15 14:30:01] Information: Mise à jour de 8 joueurs actifs
[2024-01-15 14:30:03] Information: Joueur Faker#KR1 mis à jour avec succès
[2024-01-15 14:30:05] Information: Joueur Caps#EUW mis à jour avec succès
[2024-01-15 14:30:12] Information: Mise à jour automatique terminée
[2024-01-15 14:30:12] Information: === LZG Challenge Update Service - Terminé ===
```

### Exit Codes
- **0** : Succès complet
- **1** : Erreur critique (problème de configuration, base de données inaccessible)

### Monitoring Recommandé
```bash
# Vérifier les logs récents
tail -f /var/log/lzg-update.log

# Vérifier le statut de la dernière exécution
echo $? # Après exécution manuelle

# Surveillance avec systemd (Linux)
journalctl -u lzg-update.service -f
```

## 🐛 Dépannage

### Problèmes Courants

**Service ne démarre pas**
- Vérifier la configuration MongoDB
- S'assurer que l'API principale est accessible
- Contrôler les permissions sur les fichiers

**Mise à jour échoue**
- Vérifier que l'API LZG Challenge fonctionne
- Contrôler les rate limits Riot API
- Vérifier la connectivité réseau

**Pas de participants trouvés**
- Vérifier que des joueurs sont marqués comme `IsActive = true`
- Contrôler la base de données MongoDB

### Debug en Mode Verbose
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "LZGChallenge2.UpdateService": "Debug"
    }
  }
}
```

## 🔒 Sécurité

### Bonnes Pratiques
- **Variables d'environnement** pour les secrets en production
- **Utilisateur dédié** avec permissions minimales
- **Logs rotation** pour éviter l'accumulation
- **Monitoring des échecs** pour détection rapide

### Recommandations Production
- Exécuter sous compte de service dédié
- Limiter l'accès réseau aux APIs nécessaires
- Chiffrer la configuration sensible
- Surveiller les logs d'erreur

## 📈 Performance

### Optimisations
- **Appels parallèles** pour mise à jour de plusieurs joueurs
- **Timeout approprié** (5 minutes) pour éviter les blocages
- **Gestion d'erreurs individuelles** sans arrêt global
- **Rate limiting respecté** pour l'API Riot

### Recommandations
- **Fréquence optimale** : Toutes les 5 minutes
- **Heure de pointe** : Éviter les heures de forte charge
- **Monitoring** : Surveiller le temps d'exécution moyen

---

**Service de mise à jour développé pour LZG Challenge** 🎮

### Architecture du Système

```
Cron/Task Scheduler (toutes les 5min)
    ↓
LZG Update Service
    ↓ HTTP calls
LZG Challenge API
    ↓ SignalR
Frontend React (mises à jour temps réel)
```