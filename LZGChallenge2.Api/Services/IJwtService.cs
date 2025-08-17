using System.Security.Claims;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Services;

public interface IJwtService
{
    /// <summary>
    /// Génère un token d'accès JWT pour un utilisateur
    /// </summary>
    /// <param name="user">L'utilisateur pour lequel générer le token</param>
    /// <returns>Le token JWT et sa date d'expiration</returns>
    (string token, DateTime expiresAt) GenerateAccessToken(User user);

    /// <summary>
    /// Génère un token de rafraîchissement
    /// </summary>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <param name="userAgent">User agent du client</param>
    /// <returns>Le token de rafraîchissement</returns>
    RefreshToken GenerateRefreshToken(string ipAddress, string userAgent);

    /// <summary>
    /// Valide un token JWT et extrait les claims
    /// </summary>
    /// <param name="token">Le token à valider</param>
    /// <returns>Les claims du token si valide, null sinon</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extrait l'ID utilisateur d'un token JWT
    /// </summary>
    /// <param name="token">Le token JWT</param>
    /// <returns>L'ID utilisateur ou null si le token est invalide</returns>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Extrait l'email d'un token JWT
    /// </summary>
    /// <param name="token">Le token JWT</param>
    /// <returns>L'email ou null si le token est invalide</returns>
    string? GetEmailFromToken(string token);

    /// <summary>
    /// Vérifie si un token JWT est expiré
    /// </summary>
    /// <param name="token">Le token à vérifier</param>
    /// <returns>True si le token est expiré</returns>
    bool IsTokenExpired(string token);
}