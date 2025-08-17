namespace LZGChallenge2.Api.Services;

public interface IPasswordService
{
    /// <summary>
    /// Hache un mot de passe en utilisant BCrypt
    /// </summary>
    /// <param name="password">Le mot de passe en clair</param>
    /// <returns>Le mot de passe haché</returns>
    string HashPassword(string password);

    /// <summary>
    /// Vérifie si un mot de passe correspond au hash
    /// </summary>
    /// <param name="password">Le mot de passe en clair</param>
    /// <param name="hash">Le hash stocké</param>
    /// <returns>True si le mot de passe est correct</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Valide la force d'un mot de passe
    /// </summary>
    /// <param name="password">Le mot de passe à valider</param>
    /// <returns>Liste des erreurs de validation</returns>
    List<string> ValidatePasswordStrength(string password);
}