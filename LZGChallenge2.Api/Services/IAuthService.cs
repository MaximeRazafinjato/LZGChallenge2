using LZGChallenge2.Api.DTOs;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Services;

public interface IAuthService
{
    /// <summary>
    /// Inscrit un nouvel utilisateur
    /// </summary>
    /// <param name="request">Données d'inscription</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Résultat de l'inscription</returns>
    Task<(bool Success, string Message, List<string>? Errors)> RegisterAsync(RegisterRequestDto request, string ipAddress);

    /// <summary>
    /// Connecte un utilisateur
    /// </summary>
    /// <param name="request">Données de connexion</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <param name="userAgent">User agent du client</param>
    /// <returns>Résultat de la connexion avec token et informations utilisateur</returns>
    Task<(bool Success, LoginResponseDto? Response, List<string>? Errors)> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent);

    /// <summary>
    /// Rafraîchit un token d'accès
    /// </summary>
    /// <param name="refreshToken">Token de rafraîchissement</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <param name="userAgent">User agent du client</param>
    /// <returns>Nouveau token d'accès</returns>
    Task<(bool Success, LoginResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent);

    /// <summary>
    /// Déconnecte un utilisateur et révoque le refresh token
    /// </summary>
    /// <param name="refreshToken">Token de rafraîchissement à révoquer</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Succès de la déconnexion</returns>
    Task<bool> LogoutAsync(string refreshToken, string ipAddress);

    /// <summary>
    /// Vérifie l'adresse email d'un utilisateur
    /// </summary>
    /// <param name="token">Token de vérification</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Résultat de la vérification</returns>
    Task<(bool Success, string Message)> VerifyEmailAsync(string token, string ipAddress);

    /// <summary>
    /// Renvoie un email de vérification
    /// </summary>
    /// <param name="email">Adresse email</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Résultat de l'envoi</returns>
    Task<(bool Success, string Message)> ResendVerificationEmailAsync(string email, string ipAddress);

    /// <summary>
    /// Initie une demande de réinitialisation de mot de passe
    /// </summary>
    /// <param name="email">Adresse email</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Résultat de la demande</returns>
    Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string ipAddress);

    /// <summary>
    /// Réinitialise le mot de passe avec un token
    /// </summary>
    /// <param name="request">Données de réinitialisation</param>
    /// <param name="ipAddress">Adresse IP du client</param>
    /// <returns>Résultat de la réinitialisation</returns>
    Task<(bool Success, string Message, List<string>? Errors)> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress);

    /// <summary>
    /// Change le mot de passe d'un utilisateur connecté
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="request">Données de changement de mot de passe</param>
    /// <returns>Résultat du changement</returns>
    Task<(bool Success, string Message, List<string>? Errors)> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);

    /// <summary>
    /// Récupère les informations d'un utilisateur par son ID
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <returns>Informations de l'utilisateur</returns>
    Task<UserDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Révoque tous les refresh tokens d'un utilisateur
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="reason">Raison de la révocation</param>
    /// <returns>Nombre de tokens révoqués</returns>
    Task<int> RevokeAllUserTokensAsync(string userId, string reason);
}