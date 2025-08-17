namespace LZGChallenge2.Api.Services;

public interface IEmailService
{
    /// <summary>
    /// Envoie un email de vérification d'adresse email
    /// </summary>
    /// <param name="email">L'adresse email de destination</param>
    /// <param name="name">Le nom du destinataire</param>
    /// <param name="verificationToken">Le token de vérification</param>
    /// <returns>True si l'email a été envoyé avec succès</returns>
    Task<bool> SendEmailVerificationAsync(string email, string name, string verificationToken);

    /// <summary>
    /// Envoie un email de réinitialisation de mot de passe
    /// </summary>
    /// <param name="email">L'adresse email de destination</param>
    /// <param name="name">Le nom du destinataire</param>
    /// <param name="resetToken">Le token de réinitialisation</param>
    /// <returns>True si l'email a été envoyé avec succès</returns>
    Task<bool> SendPasswordResetAsync(string email, string name, string resetToken);

    /// <summary>
    /// Envoie un email de bienvenue après inscription
    /// </summary>
    /// <param name="email">L'adresse email de destination</param>
    /// <param name="name">Le nom du destinataire</param>
    /// <returns>True si l'email a été envoyé avec succès</returns>
    Task<bool> SendWelcomeEmailAsync(string email, string name);

    /// <summary>
    /// Envoie un email de notification de changement de mot de passe
    /// </summary>
    /// <param name="email">L'adresse email de destination</param>
    /// <param name="name">Le nom du destinataire</param>
    /// <returns>True si l'email a été envoyé avec succès</returns>
    Task<bool> SendPasswordChangedNotificationAsync(string email, string name);
}