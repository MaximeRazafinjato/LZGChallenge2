#!/bin/bash

# Script pour exécuter le service LZG Update via Docker
# À utiliser avec cron pour l'exécution périodique

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
LOG_FILE="/var/log/lzg-update-docker.log"

# Fonction de logging
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

log "=== Démarrage du service LZG Update (Docker) ==="
log "Répertoire du projet: $PROJECT_DIR"

# Aller dans le répertoire du projet
cd "$PROJECT_DIR"

# Vérifier que le fichier .env existe
if [ ! -f ".env" ]; then
    log "❌ ERREUR: Fichier .env manquant. Copiez .env.example vers .env et configurez-le."
    exit 1
fi

# Vérifier que docker-compose est disponible
if ! command -v docker-compose &> /dev/null; then
    log "❌ ERREUR: docker-compose n'est pas installé"
    exit 1
fi

# Exécuter le service via Docker Compose
log "🚀 Exécution du service de mise à jour..."

if docker-compose run --rm lzg-update-service 2>&1 | tee -a "$LOG_FILE"; then
    log "✅ Service terminé avec succès"
    exit 0
else
    EXIT_CODE=$?
    log "❌ Service terminé avec erreur (code: $EXIT_CODE)"
    exit $EXIT_CODE
fi