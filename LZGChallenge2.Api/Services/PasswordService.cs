using System.Text.RegularExpressions;
using BCrypt.Net;

namespace LZGChallenge2.Api.Services;

public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor for hashing

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Le mot de passe ne peut pas être vide", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public List<string> ValidatePasswordStrength(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Le mot de passe est requis");
            return errors;
        }

        if (password.Length < 8)
            errors.Add("Le mot de passe doit contenir au moins 8 caractères");

        if (password.Length > 128)
            errors.Add("Le mot de passe ne peut pas dépasser 128 caractères");

        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("Le mot de passe doit contenir au moins une lettre minuscule");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("Le mot de passe doit contenir au moins une lettre majuscule");

        if (!Regex.IsMatch(password, @"\d"))
            errors.Add("Le mot de passe doit contenir au moins un chiffre");

        if (!Regex.IsMatch(password, @"[@$!%*?&]"))
            errors.Add("Le mot de passe doit contenir au moins un caractère spécial (@$!%*?&)");

        // Vérification de mots de passe communs/faibles
        var commonPasswords = new[]
        {
            "password", "123456", "123456789", "qwerty", "abc123",
            "password123", "admin", "letmein", "welcome", "monkey"
        };

        if (commonPasswords.Contains(password.ToLower()))
            errors.Add("Ce mot de passe est trop commun, veuillez en choisir un autre");

        return errors;
    }
}