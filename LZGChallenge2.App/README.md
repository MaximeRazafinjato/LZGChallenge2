# LZG Challenge - Frontend React

Interface utilisateur moderne pour suivre les compétitions SoloQ League of Legends avec mises à jour temps réel.

## 🎨 Aperçu

Interface React élégante inspirée de l'univers League of Legends avec :
- **Design responsive** adapté à tous les écrans
- **Animations fluides** avec Framer Motion
- **Thème League of Legends** avec Material-UI
- **Mises à jour temps réel** via SignalR

## ⚡ Technologies

### Stack Principal
- **React 19.1.1** - Framework UI moderne
- **Vite 7.1.2** - Build tool ultra-rapide
- **Material-UI (MUI)** - Composants UI
- **Framer Motion** - Animations
- **SignalR Client** - Communication temps réel

### Outils de Développement
- **ESLint** - Linting et qualité de code
- **pnpm** - Gestionnaire de paquets rapide
- **Vite HMR** - Hot Module Replacement

## 🚀 Démarrage Rapide

### Prérequis
- **Node.js 18+**
- **pnpm** (recommandé) ou npm

### Installation
```bash
cd LZGChallenge2.App
pnpm install
```

### Développement
```bash
pnpm dev
# Interface disponible sur http://localhost:5173
```

### Build Production
```bash
pnpm build
# Fichiers générés dans ./dist/
```

### Preview Production
```bash
pnpm preview
# Test du build de production
```

## 📱 Structure de l'Application

### Architecture des Composants
```
src/
├── main.jsx                    # Point d'entrée React
├── App.jsx                     # Composant principal avec SignalR
├── theme.js                    # Thème Material-UI personnalisé
├── components/
│   ├── Header.jsx              # En-tête de l'application
│   ├── StatsCards.jsx          # Cartes de statistiques du challenge
│   ├── Leaderboard.jsx         # Tableau de classement principal
│   ├── PlayersGrid.jsx         # Grille des participants
│   ├── AddPlayerForm.jsx       # Modal d'ajout de participant
│   ├── ChampionModal.jsx       # Modal des statistiques par champion
│   └── MatchHistoryModal.jsx   # Modal de l'historique des matches
├── services/
│   └── api.js                  # Service API centralisé
└── utils/
    └── championUtils.js        # Utilitaires pour les champions
```

### Flux de Données
```
API Backend (SignalR) ↔ App.jsx ↓ 
                                 ├─ StatsCards (statistiques globales)
                                 ├─ Leaderboard (classement)
                                 ├─ PlayersGrid (participants)
                                 └─ Modals (détails)
```

## 🎯 Fonctionnalités

### 📊 Tableau de Bord
**StatsCards.jsx** - Cartes statistiques principales :
- **Participants** : Nombre de joueurs actifs
- **Parties** : Total des matches SoloQ
- **Winrate Moyen** : Performance globale  
- **Leader** : Joueur en tête avec rang/LP

### 🏆 Leaderboard
**Leaderboard.jsx** - Classement interactif :
- **Tri dynamique** : LP, Winrate, KDA, Nombre de parties
- **Informations détaillées** : Rang, LP, séries W/L
- **Champions favoris** : Top 3 des champions par joueur
- **Actions** : Voir détails, actualiser données

### 👥 Gestion des Participants
**AddPlayerForm.jsx** - Ajout automatisé :
- **Validation** : GameName#TagLine format
- **Régions** : Support multi-régions
- **Auto-refresh** : Récupération automatique des données
- **Feedback** : Messages de succès/erreur

**PlayersGrid.jsx** - Grille des participants :
- **Cartes individuelles** : Avatar, rang, statistiques
- **Actions rapides** : Supprimer, actualiser
- **État en temps réel** : Mises à jour via SignalR

### 🔍 Analyses Détaillées

**ChampionModal.jsx** - Statistiques par champion :
- **Performance détaillée** : Winrate, KDA, CS moyen
- **Tri et filtres** : Par performance, nom, nombre de parties
- **Visualisation** : Graphiques et métriques

**MatchHistoryModal.jsx** - Historique complet :
- **Filtres avancés** : Période, résultat, champion, rôle
- **Détails des matches** : KDA, CS, dégâts, LP gain/loss
- **Recherche** : Filtrage en temps réel

## 🔄 Communication Temps Réel

### SignalR Integration
**App.jsx** gère la connexion SignalR :

```javascript
const connection = new HubConnectionBuilder()
  .withUrl('https://localhost:44393/leaderboardHub')
  .withAutomaticReconnect()
  .build();

// Événements écoutés
connection.on('PlayerAdded', (player) => {
  // Nouveau participant ajouté
});

connection.on('PlayerRemoved', (playerId) => {
  // Participant supprimé
});

connection.on('PlayerUpdated', (playerId) => {
  // Données mises à jour
});
```

### États de Connexion
- **Connecting** : Tentative de connexion
- **Connected** : Prêt pour les mises à jour
- **Reconnecting** : Reconnexion automatique
- **Disconnected** : Mode dégradé

## 🎨 Thème et Design

### Couleurs League of Legends
```javascript
// theme.js
const theme = {
  palette: {
    primary: { main: '#C89B3C' },      // Or Riot
    secondary: { main: '#00F5FF' },    // Cyan électrique
    background: {
      default: '#010A13',              // Bleu très sombre
      paper: '#1E2328'                 // Gris sombre
    },
    success: { main: '#50C878' },      // Vert victoire
    error: { main: '#E74C3C' }         // Rouge défaite
  }
}
```

### Animations
**Framer Motion** pour les transitions :
- **Cartes** : Hover et apparition
- **Listes** : Animation séquentielle
- **Modals** : Transitions fluides
- **Chargement** : Skeletons animés

## 📡 Services API

### API Service (api.js)
Service centralisé pour toutes les requêtes :

```javascript
// Exemples d'utilisation
import { leaderboardApi, playersApi } from './services/api';

// Récupérer le classement
const leaderboard = await leaderboardApi.getLeaderboard('lp');

// Ajouter un participant
const newPlayer = await playersApi.create({
  gameName: 'Faker',
  tagLine: 'KR1',
  region: 'EUW1'
});
```

### Gestion d'Erreurs
- **Try/catch** sur toutes les requêtes
- **Messages utilisateur** appropriés
- **Retry automatique** pour certaines erreurs
- **Fallback** en cas d'échec SignalR

## 🔧 Configuration

### Variables d'Environnement
```bash
# .env (si nécessaire)
VITE_API_BASE_URL=https://localhost:44393
VITE_SIGNALR_HUB_URL=https://localhost:44393/leaderboardHub
```

### Vite Configuration
```javascript
// vite.config.js
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    cors: true
  },
  build: {
    outDir: 'dist',
    sourcemap: true
  }
})
```

### ESLint Configuration
```javascript
// eslint.config.js
export default [
  js.configs.recommended,
  ...reactHooks.configs.recommended,
  ...reactRefresh.configs.recommended,
  {
    rules: {
      'no-unused-vars': ['error', { 
        varsIgnorePattern: '^[A-Z_]' 
      }]
    }
  }
]
```

## 🧪 Tests et Développement

### Scripts Disponibles
```bash
# Développement
pnpm dev              # Serveur de développement

# Build
pnpm build            # Build de production
pnpm preview          # Preview du build

# Qualité
pnpm lint             # Linting ESLint
pnpm lint:fix         # Correction automatique
```

### Debugging
- **React DevTools** : Inspection des composants
- **Network Tab** : Monitoring des requêtes API
- **Console SignalR** : Logs de connexion temps réel

### Performance
- **Code splitting** : Chargement lazy des modals
- **Memoization** : React.memo sur les composants lourds
- **Optimized renders** : useState et useEffect optimisés

## 📱 Responsive Design

### Breakpoints Material-UI
```javascript
// Responsive design
sx={{
  display: { xs: 'block', md: 'flex' },
  gap: { xs: 1, md: 2 },
  p: { xs: 2, md: 3 }
}}
```

### Mobile First
- **Navigation** : Menu hamburger sur mobile
- **Cartes** : Stack vertical sur petits écrans
- **Modals** : Plein écran sur mobile
- **Tableaux** : Scroll horizontal si nécessaire

## 🚀 Déploiement

### Build Production
```bash
pnpm build
# Fichiers optimisés dans ./dist/
```

### Variables de Production
```bash
# À configurer selon l'environnement
VITE_API_BASE_URL=https://votre-api.com
```

### Serveur Statique
```bash
# Avec serve
npm install -g serve
serve -s dist -l 3000

# Avec nginx/apache
# Copier le contenu de ./dist/ vers le web root
```

## 🔒 Sécurité

### Bonnes Pratiques
- **Sanitisation** des entrées utilisateur
- **Validation** côté client ET serveur
- **HTTPS** requis pour SignalR en production
- **CSP Headers** recommandés

### Gestion des Tokens
- **Pas de tokens** stockés côté client
- **CORS** configuré strictement
- **Validation** des données reçues

---

**Frontend développé avec** ⚛️ **React pour LZG Challenge** 🎮