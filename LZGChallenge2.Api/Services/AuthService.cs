using MongoDB.Driver;
using System.Security.Cryptography;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.DTOs;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Services;

public class AuthService : IAuthService
{
    private readonly MongoDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    // Constantes pour la sécurité
    private const int MaxLoginAttempts = 5;
    private const int LockoutDurationMinutes = 30;
    private const int EmailVerificationTokenExpirationHours = 24;
    private const int PasswordResetTokenExpirationHours = 1;

    public AuthService(
        MongoDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, List<string>? Errors)> RegisterAsync(RegisterRequestDto request, string ipAddress)
    {
        try
        {
            // Vérifier si l'utilisateur existe déjà
            var existingUser = await _context.Users
                .Find(u => u.Email.ToLower() == request.Email.ToLower())
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return (false, "Un compte avec cette adresse email existe déjà", null);
            }

            // Valider la force du mot de passe
            var passwordErrors = _passwordService.ValidatePasswordStrength(request.Password);
            if (passwordErrors.Any())
            {
                return (false, "Le mot de passe ne respecte pas les critères de sécurité", passwordErrors);
            }

            // Créer l'utilisateur
            var user = new User
            {
                Email = request.Email.ToLower().Trim(),
                PasswordHash = _passwordService.HashPassword(request.Password),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Role = UserRole.User,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);

            // Générer et envoyer le token de vérification d'email
            var verificationToken = GenerateSecureToken();
            var emailVerificationToken = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = verificationToken,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(EmailVerificationTokenExpirationHours),
                IpAddress = ipAddress
            };

            await _context.EmailVerificationTokens.InsertOneAsync(emailVerificationToken);

            // Envoyer l'email de vérification
            var emailSent = await _emailService.SendEmailVerificationAsync(
                user.Email, 
                $"{user.FirstName} {user.LastName}", 
                verificationToken);

            if (!emailSent)
            {
                _logger.LogWarning("Échec de l'envoi de l'email de vérification pour {Email}", user.Email);
            }

            _logger.LogInformation("Nouvel utilisateur inscrit: {Email}", user.Email);

            return (true, "Inscription réussie ! Veuillez vérifier votre email pour activer votre compte.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'inscription pour {Email}", request.Email);
            return (false, "Une erreur est survenue lors de l'inscription", null);
        }
    }

    public async Task<(bool Success, LoginResponseDto? Response, List<string>? Errors)> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent)
    {
        try
        {
            var user = await _context.Users
                .Find(u => u.Email.ToLower() == request.Email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                await Task.Delay(500); // Protection contre les attaques par timing
                return (false, null, new List<string> { "Email ou mot de passe incorrect" });
            }

            // Vérifier si le compte est verrouillé
            if (user.LockoutUntil.HasValue && user.LockoutUntil > DateTime.UtcNow)
            {
                var remainingMinutes = (int)(user.LockoutUntil.Value - DateTime.UtcNow).TotalMinutes;
                return (false, null, new List<string> { $"Compte verrouillé. Réessayez dans {remainingMinutes} minutes." });
            }

            // Vérifier le mot de passe
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                await HandleFailedLoginAsync(user);
                return (false, null, new List<string> { "Email ou mot de passe incorrect" });
            }

            // Vérifier si l'email est vérifié
            if (!user.IsEmailVerified)
            {
                return (false, null, new List<string> { "Veuillez vérifier votre email avant de vous connecter" });
            }

            // Vérifier si le compte est actif
            if (!user.IsActive)
            {
                return (false, null, new List<string> { "Ce compte est désactivé" });
            }

            // Connexion réussie - réinitialiser les tentatives de connexion
            await ResetLoginAttemptsAsync(user);

            // Générer les tokens
            var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress, userAgent);
            refreshToken.UserId = user.Id;

            // Sauvegarder le refresh token
            await _context.RefreshTokens.InsertOneAsync(refreshToken);

            // Mettre à jour la dernière connexion
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            var userDto = MapToUserDto(user);
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                User = userDto,
                ExpiresAt = expiresAt,
                RefreshToken = refreshToken.Token
            };

            _logger.LogInformation("Connexion réussie pour {Email}", user.Email);

            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion pour {Email}", request.Email);
            return (false, null, new List<string> { "Une erreur est survenue lors de la connexion" });
        }
    }

    public async Task<(bool Success, LoginResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Find(rt => rt.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (token == null || !token.IsActive)
            {
                return (false, null, "Token de rafraîchissement invalide");
            }

            var user = await _context.Users
                .Find(u => u.Id == token.UserId)
                .FirstOrDefaultAsync();

            if (user == null || !user.IsActive)
            {
                return (false, null, "Utilisateur introuvable ou inactif");
            }

            // Révoquer l'ancien token
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Remplacé par nouveau token";
            await _context.RefreshTokens.ReplaceOneAsync(rt => rt.Id == token.Id, token);

            // Générer de nouveaux tokens
            var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken(ipAddress, userAgent);
            newRefreshToken.UserId = user.Id;
            token.ReplacedByToken = newRefreshToken.Token;

            await _context.RefreshTokens.InsertOneAsync(newRefreshToken);

            var userDto = MapToUserDto(user);
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                User = userDto,
                ExpiresAt = expiresAt,
                RefreshToken = newRefreshToken.Token
            };

            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement du token");
            return (false, null, "Erreur lors du rafraîchissement du token");
        }
    }

    public async Task<bool> LogoutAsync(string refreshToken, string ipAddress)
    {
        try
        {
            var token = await _context.RefreshTokens
                .Find(rt => rt.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (token == null)
                return false;

            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = $"Déconnexion depuis {ipAddress}";

            await _context.RefreshTokens.ReplaceOneAsync(rt => rt.Id == token.Id, token);

            _logger.LogInformation("Déconnexion réussie pour l'utilisateur {UserId}", token.UserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
            return false;
        }
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string token, string ipAddress)
    {
        try
        {
            var verificationToken = await _context.EmailVerificationTokens
                .Find(evt => evt.Token == token)
                .FirstOrDefaultAsync();

            if (verificationToken == null || !verificationToken.IsValid)
            {
                return (false, "Token de vérification invalide ou expiré");
            }

            var user = await _context.Users
                .Find(u => u.Id == verificationToken.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, "Utilisateur introuvable");
            }

            // Marquer l'email comme vérifié
            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            // Marquer le token comme utilisé
            verificationToken.UsedAt = DateTime.UtcNow;
            await _context.EmailVerificationTokens.ReplaceOneAsync(evt => evt.Id == verificationToken.Id, verificationToken);

            // Envoyer l'email de bienvenue
            await _emailService.SendWelcomeEmailAsync(user.Email, $"{user.FirstName} {user.LastName}");

            _logger.LogInformation("Email vérifié avec succès pour {Email}", user.Email);

            return (true, "Email vérifié avec succès ! Vous pouvez maintenant vous connecter.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification d'email");
            return (false, "Une erreur est survenue lors de la vérification");
        }
    }

    public async Task<(bool Success, string Message)> ResendVerificationEmailAsync(string email, string ipAddress)
    {
        try
        {
            var user = await _context.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                // Ne pas révéler si l'email existe ou non
                return (true, "Si cette adresse email est enregistrée, vous recevrez un email de vérification");
            }

            if (user.IsEmailVerified)
            {
                return (false, "Cette adresse email est déjà vérifiée");
            }

            // Révoquer les anciens tokens de vérification
            await _context.EmailVerificationTokens.UpdateManyAsync(
                evt => evt.UserId == user.Id && evt.UsedAt == null,
                Builders<EmailVerificationToken>.Update.Set(evt => evt.UsedAt, DateTime.UtcNow));

            // Générer un nouveau token
            var verificationToken = GenerateSecureToken();
            var emailVerificationToken = new EmailVerificationToken
            {
                UserId = user.Id,
                Token = verificationToken,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(EmailVerificationTokenExpirationHours),
                IpAddress = ipAddress
            };

            await _context.EmailVerificationTokens.InsertOneAsync(emailVerificationToken);

            // Envoyer l'email
            await _emailService.SendEmailVerificationAsync(
                user.Email, 
                $"{user.FirstName} {user.LastName}", 
                verificationToken);

            return (true, "Email de vérification envoyé");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du renvoi de l'email de vérification");
            return (false, "Une erreur est survenue");
        }
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email, string ipAddress)
    {
        try
        {
            var user = await _context.Users
                .Find(u => u.Email.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();

            if (user == null)
            {
                // Ne pas révéler si l'email existe ou non
                return (true, "Si cette adresse email est enregistrée, vous recevrez un email de réinitialisation");
            }

            // Révoquer les anciens tokens de réinitialisation
            await _context.PasswordResetTokens.UpdateManyAsync(
                prt => prt.UserId == user.Id && prt.UsedAt == null,
                Builders<PasswordResetToken>.Update.Set(prt => prt.UsedAt, DateTime.UtcNow));

            // Générer un nouveau token
            var resetToken = GenerateSecureToken();
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = resetToken,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenExpirationHours),
                IpAddress = ipAddress
            };

            await _context.PasswordResetTokens.InsertOneAsync(passwordResetToken);

            // Envoyer l'email
            await _emailService.SendPasswordResetAsync(
                user.Email, 
                $"{user.FirstName} {user.LastName}", 
                resetToken);

            _logger.LogInformation("Demande de réinitialisation de mot de passe pour {Email}", user.Email);

            return (true, "Email de réinitialisation envoyé");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la demande de réinitialisation");
            return (false, "Une erreur est survenue");
        }
    }

    public async Task<(bool Success, string Message, List<string>? Errors)> ResetPasswordAsync(ResetPasswordRequestDto request, string ipAddress)
    {
        try
        {
            var resetToken = await _context.PasswordResetTokens
                .Find(prt => prt.Token == request.Token)
                .FirstOrDefaultAsync();

            if (resetToken == null || !resetToken.IsValid)
            {
                return (false, "Token de réinitialisation invalide ou expiré", null);
            }

            var user = await _context.Users
                .Find(u => u.Id == resetToken.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, "Utilisateur introuvable", null);
            }

            // Valider le nouveau mot de passe
            var passwordErrors = _passwordService.ValidatePasswordStrength(request.Password);
            if (passwordErrors.Any())
            {
                return (false, "Le mot de passe ne respecte pas les critères de sécurité", passwordErrors);
            }

            // Mettre à jour le mot de passe
            user.PasswordHash = _passwordService.HashPassword(request.Password);
            user.UpdatedAt = DateTime.UtcNow;
            user.LoginAttempts = 0; // Réinitialiser les tentatives de connexion
            user.LockoutUntil = null;

            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            // Marquer le token comme utilisé
            resetToken.UsedAt = DateTime.UtcNow;
            await _context.PasswordResetTokens.ReplaceOneAsync(prt => prt.Id == resetToken.Id, resetToken);

            // Révoquer tous les refresh tokens existants pour cette utilisateur
            await RevokeAllUserTokensAsync(user.Id, "Réinitialisation de mot de passe");

            // Envoyer une notification de changement de mot de passe
            await _emailService.SendPasswordChangedNotificationAsync(
                user.Email, 
                $"{user.FirstName} {user.LastName}");

            _logger.LogInformation("Mot de passe réinitialisé avec succès pour {Email}", user.Email);

            return (true, "Mot de passe réinitialisé avec succès", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la réinitialisation du mot de passe");
            return (false, "Une erreur est survenue lors de la réinitialisation", null);
        }
    }

    public async Task<(bool Success, string Message, List<string>? Errors)> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        try
        {
            var user = await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return (false, "Utilisateur introuvable", null);
            }

            // Vérifier l'ancien mot de passe
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return (false, "Mot de passe actuel incorrect", null);
            }

            // Valider le nouveau mot de passe
            var passwordErrors = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (passwordErrors.Any())
            {
                return (false, "Le nouveau mot de passe ne respecte pas les critères de sécurité", passwordErrors);
            }

            // Vérifier que le nouveau mot de passe est différent de l'ancien
            if (_passwordService.VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                return (false, "Le nouveau mot de passe doit être différent de l'ancien", null);
            }

            // Mettre à jour le mot de passe
            user.PasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            // Envoyer une notification
            await _emailService.SendPasswordChangedNotificationAsync(
                user.Email, 
                $"{user.FirstName} {user.LastName}");

            _logger.LogInformation("Mot de passe changé avec succès pour {Email}", user.Email);

            return (true, "Mot de passe modifié avec succès", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de mot de passe pour {UserId}", userId);
            return (false, "Une erreur est survenue lors du changement de mot de passe", null);
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId}", userId);
            return null;
        }
    }

    public async Task<int> RevokeAllUserTokensAsync(string userId, string reason)
    {
        try
        {
            var activeTokens = await _context.RefreshTokens
                .Find(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.ReasonRevoked = reason;
            }

            if (activeTokens.Any())
            {
                await _context.RefreshTokens.BulkWriteAsync(
                    activeTokens.Select(token => 
                        new ReplaceOneModel<RefreshToken>(
                            Builders<RefreshToken>.Filter.Eq(rt => rt.Id, token.Id), 
                            token)));
            }

            _logger.LogInformation("Révoqué {Count} tokens pour l'utilisateur {UserId}", activeTokens.Count, userId);

            return activeTokens.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la révocation des tokens pour {UserId}", userId);
            return 0;
        }
    }

    private async Task HandleFailedLoginAsync(User user)
    {
        user.LoginAttempts++;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.LoginAttempts >= MaxLoginAttempts)
        {
            user.LockoutUntil = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            _logger.LogWarning("Compte verrouillé pour {Email} après {Attempts} tentatives", user.Email, user.LoginAttempts);
        }

        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    private async Task ResetLoginAttemptsAsync(User user)
    {
        if (user.LoginAttempts > 0 || user.LockoutUntil.HasValue)
        {
            user.LoginAttempts = 0;
            user.LockoutUntil = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}