#!/bin/bash

# Script pour configurer le service LZG Update en tant que cron job Linux
# Exécuter avec sudo

set -e

SERVICE_PATH=""
INTERVAL_MINUTES=5
USER="lzg-service"
LOG_FILE="/var/log/lzg-update.log"

# Fonction d'aide
show_help() {
    echo "Usage: $0 -p SERVICE_PATH [-i INTERVAL_MINUTES] [-u USER] [-l LOG_FILE]"
    echo ""
    echo "Options:"
    echo "  -p SERVICE_PATH      Chemin vers le dossier publish du service (obligatoire)"
    echo "  -i INTERVAL_MINUTES  Intervalle en minutes (défaut: 5)"
    echo "  -u USER              Utilisateur pour exécuter le service (défaut: lzg-service)"
    echo "  -l LOG_FILE          Fichier de logs (défaut: /var/log/lzg-update.log)"
    echo "  -h                   Afficher cette aide"
    echo ""
    echo "Exemple:"
    echo "  sudo $0 -p /opt/lzg-challenge-update -i 5"
}

# Parser les arguments
while getopts "p:i:u:l:h" opt; do
    case $opt in
        p) SERVICE_PATH="$OPTARG" ;;
        i) INTERVAL_MINUTES="$OPTARG" ;;
        u) USER="$OPTARG" ;;
        l) LOG_FILE="$OPTARG" ;;
        h) show_help; exit 0 ;;
        *) echo "Option invalide. Utilisez -h pour l'aide."; exit 1 ;;
    esac
done

# Vérifier que le chemin de service est fourni
if [ -z "$SERVICE_PATH" ]; then
    echo "❌ Erreur: Le chemin du service (-p) est obligatoire"
    show_help
    exit 1
fi

# Vérifier que le script est exécuté en tant que root
if [ "$EUID" -ne 0 ]; then
    echo "❌ Ce script doit être exécuté avec sudo"
    exit 1
fi

echo "=== Configuration du Service LZG Challenge Update ==="
echo "📂 Chemin du service: $SERVICE_PATH"
echo "⏰ Intervalle: $INTERVAL_MINUTES minutes"
echo "👤 Utilisateur: $USER"
echo "📝 Fichier de logs: $LOG_FILE"
echo ""

# Vérifier si le chemin existe
if [ ! -d "$SERVICE_PATH" ]; then
    echo "❌ Le chemin spécifié n'existe pas: $SERVICE_PATH"
    exit 1
fi

# Vérifier si le fichier DLL existe
if [ ! -f "$SERVICE_PATH/LZGChallenge2.UpdateService.dll" ]; then
    echo "❌ Le fichier LZGChallenge2.UpdateService.dll n'existe pas dans $SERVICE_PATH"
    echo "Assurez-vous d'avoir exécuté 'dotnet publish' avant d'utiliser ce script"
    exit 1
fi

# Créer l'utilisateur si nécessaire
if ! id "$USER" &>/dev/null; then
    echo "👤 Création de l'utilisateur $USER..."
    useradd -r -s /bin/false -d /opt/lzg-challenge "$USER"
fi

# Créer le répertoire de logs
LOG_DIR=$(dirname "$LOG_FILE")
mkdir -p "$LOG_DIR"
touch "$LOG_FILE"
chown "$USER:$USER" "$LOG_FILE"

# Créer le script de lancement
LAUNCH_SCRIPT="/usr/local/bin/lzg-update.sh"
echo "📝 Création du script de lancement: $LAUNCH_SCRIPT"

cat > "$LAUNCH_SCRIPT" << EOF
#!/bin/bash
# Script de lancement automatique pour LZG Challenge Update Service
# Généré automatiquement le $(date)

export DOTNET_ROOT=/usr/share/dotnet
export PATH=\$PATH:\$DOTNET_ROOT

cd "$SERVICE_PATH"

echo "\$(date '+%Y-%m-%d %H:%M:%S') - Démarrage du service LZG Update" >> "$LOG_FILE"

if /usr/bin/dotnet LZGChallenge2.UpdateService.dll >> "$LOG_FILE" 2>&1; then
    echo "\$(date '+%Y-%m-%d %H:%M:%S') - Service terminé avec succès (exit code: 0)" >> "$LOG_FILE"
else
    EXIT_CODE=\$?
    echo "\$(date '+%Y-%m-%d %H:%M:%S') - Service terminé avec erreur (exit code: \$EXIT_CODE)" >> "$LOG_FILE"
fi
EOF

chmod +x "$LAUNCH_SCRIPT"
chown root:root "$LAUNCH_SCRIPT"

# Configurer les permissions sur le répertoire de service
chown -R "$USER:$USER" "$SERVICE_PATH"

# Calculer la ligne cron
CRON_EXPRESSION="*/$INTERVAL_MINUTES * * * *"

echo "⏰ Configuration du cron job..."
echo "Expression cron: $CRON_EXPRESSION"

# Ajouter la ligne cron pour l'utilisateur
(crontab -u "$USER" -l 2>/dev/null || true; echo "$CRON_EXPRESSION $LAUNCH_SCRIPT") | crontab -u "$USER" -

# Vérifier que le cron est installé
if [ -f "/etc/crontab" ]; then
    echo "✅ Cron job configuré avec succès pour l'utilisateur $USER"
else
    echo "⚠️  Attention: cron ne semble pas être installé sur ce système"
    echo "Installez cron: apt-get install cron (Ubuntu/Debian) ou yum install cronie (CentOS/RHEL)"
fi

# Tester le service
echo "🧪 Test d'exécution du service..."
if sudo -u "$USER" "$LAUNCH_SCRIPT"; then
    echo "✅ Test réussi! Le service s'exécute correctement."
else
    echo "⚠️  Le test a échoué. Vérifiez les logs: $LOG_FILE"
fi

echo ""
echo "📋 Commandes utiles:"
echo "   Voir les logs:           tail -f $LOG_FILE"
echo "   Tester manuellement:     sudo -u $USER $LAUNCH_SCRIPT"
echo "   Voir les cron jobs:      sudo crontab -u $USER -l"
echo "   Éditer les cron jobs:    sudo crontab -u $USER -e"
echo "   Statut du service cron:  systemctl status cron"
echo ""
echo "📂 Fichiers créés:"
echo "   Script de lancement:     $LAUNCH_SCRIPT"
echo "   Fichier de logs:         $LOG_FILE"
echo "   Répertoire de service:   $SERVICE_PATH"
echo ""
echo "✅ Configuration terminée! Le service s'exécutera toutes les $INTERVAL_MINUTES minutes."