// Utilitaires pour les champions League of Legends

// URL de base pour les images de champions Riot Games
const CHAMPION_IMAGE_BASE_URL = 'https://ddragon.leagueoflegends.com/cdn/14.1.1/img/champion'

/**
 * Génère l'URL de l'image d'un champion
 * @param {string} championName - Nom du champion
 * @returns {string} URL de l'image du champion
 */
export const getChampionImageUrl = (championName) => {
  if (!championName) return null
  
  // Nettoyer le nom du champion pour l'URL
  const cleanName = championName
    .replace(/[^a-zA-Z0-9]/g, '') // Supprimer les caractères spéciaux
    .replace(/\s+/g, '') // Supprimer les espaces
  
  return `${CHAMPION_IMAGE_BASE_URL}/${cleanName}.png`
}

/**
 * Génère l'URL de l'image d'un champion avec fallback
 * @param {string} championName - Nom du champion
 * @returns {string} URL de l'image avec fallback vers une image par défaut
 */
export const getChampionImageUrlWithFallback = (championName) => {
  const imageUrl = getChampionImageUrl(championName)
  
  if (!imageUrl) {
    // Image par défaut si pas de nom de champion
    return `${CHAMPION_IMAGE_BASE_URL}/Unknown.png`
  }
  
  return imageUrl
}

/**
 * Normalise le nom d'un champion pour les URLs d'images Riot
 * Certains champions ont des noms spéciaux dans l'API
 * @param {string} championName - Nom du champion
 * @returns {string} Nom normalisé
 */
export const normalizeChampionName = (championName) => {
  if (!championName) return ''
  
  const nameMap = {
    'Wukong': 'MonkeyKing',
    'Nunu & Willump': 'Nunu',
    'Kai\'Sa': 'Kaisa',
    'Kha\'Zix': 'Khazix',
    'Vel\'Koz': 'Velkoz',
    'Cho\'Gath': 'Chogath',
    'Dr. Mundo': 'DrMundo',
    'Jarvan IV': 'JarvanIV',
    'LeBlanc': 'Leblanc',
    'Lee Sin': 'LeeSin',
    'Master Yi': 'MasterYi',
    'Miss Fortune': 'MissFortune',
    'Rek\'Sai': 'RekSai',
    'Twisted Fate': 'TwistedFate',
    'Xin Zhao': 'XinZhao',
    'Aurelion Sol': 'AurelionSol',
    'Tahm Kench': 'TahmKench',
    'Bel\'Veth': 'Belveth',
    'K\'Sante': 'KSante',
    'Kog\'Maw': 'KogMaw',
    'Renata Glasc': 'Renata'
  }
  
  return nameMap[championName] || championName.replace(/[^a-zA-Z0-9]/g, '')
}

/**
 * Génère l'URL finale de l'image avec normalisation
 * @param {string} championName - Nom du champion
 * @returns {string} URL de l'image normalisée
 */
export const getChampionImageUrlNormalized = (championName) => {
  if (!championName) return `${CHAMPION_IMAGE_BASE_URL}/Unknown.png`
  
  const normalizedName = normalizeChampionName(championName)
  return `${CHAMPION_IMAGE_BASE_URL}/${normalizedName}.png`
}

/**
 * Retourne les styles pour une image de champion
 * @param {string} size - Taille (small, medium, large)
 * @returns {object} Objet de styles
 */
export const getChampionImageStyles = (size = 'medium') => {
  const sizes = {
    small: 32,
    medium: 48,
    large: 64
  }
  
  const imageSize = sizes[size] || sizes.medium
  
  return {
    width: imageSize,
    height: imageSize,
    borderRadius: '50%',
    objectFit: 'cover',
    aspectRatio: '1/1',
    border: '2px solid rgba(200,155,60,0.3)'
  }
}