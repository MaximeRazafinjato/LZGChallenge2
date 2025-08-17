using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using LZGChallenge2.Api.Options;

namespace LZGChallenge2.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> emailOptions,
        IFluentEmail fluentEmail,
        ILogger<EmailService> logger)
    {
        _emailOptions = emailOptions.Value;
        _fluentEmail = fluentEmail;
        _logger = logger;
    }

    public async Task<bool> SendEmailVerificationAsync(string email, string name, string verificationToken)
    {
        try
        {
            var verificationUrl = $"{_emailOptions.BaseUrl}/verify-email?token={verificationToken}";
            
            var emailBody = GenerateEmailVerificationTemplate(name, verificationUrl);

            var result = await _fluentEmail
                .To(email, name)
                .Subject("LZG Challenge - Vérifiez votre adresse email")
                .Body(emailBody, true)
                .SendAsync();

            if (result.Successful)
            {
                _logger.LogInformation("Email de vérification envoyé avec succès à {Email}", email);
                return true;
            }
            else
            {
                _logger.LogError("Échec de l'envoi de l'email de vérification à {Email}: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de l'email de vérification à {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string email, string name, string resetToken)
    {
        try
        {
            var resetUrl = $"{_emailOptions.BaseUrl}/reset-password?token={resetToken}";
            
            var emailBody = GeneratePasswordResetTemplate(name, resetUrl);

            var result = await _fluentEmail
                .To(email, name)
                .Subject("LZG Challenge - Réinitialisation de votre mot de passe")
                .Body(emailBody, true)
                .SendAsync();

            if (result.Successful)
            {
                _logger.LogInformation("Email de réinitialisation de mot de passe envoyé avec succès à {Email}", email);
                return true;
            }
            else
            {
                _logger.LogError("Échec de l'envoi de l'email de réinitialisation à {Email}: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de l'email de réinitialisation à {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string email, string name)
    {
        try
        {
            var emailBody = GenerateWelcomeTemplate(name);

            var result = await _fluentEmail
                .To(email, name)
                .Subject("Bienvenue dans LZG Challenge !")
                .Body(emailBody, true)
                .SendAsync();

            if (result.Successful)
            {
                _logger.LogInformation("Email de bienvenue envoyé avec succès à {Email}", email);
                return true;
            }
            else
            {
                _logger.LogError("Échec de l'envoi de l'email de bienvenue à {Email}: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de l'email de bienvenue à {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(string email, string name)
    {
        try
        {
            var emailBody = GeneratePasswordChangedTemplate(name);

            var result = await _fluentEmail
                .To(email, name)
                .Subject("LZG Challenge - Mot de passe modifié")
                .Body(emailBody, true)
                .SendAsync();

            if (result.Successful)
            {
                _logger.LogInformation("Email de notification de changement de mot de passe envoyé avec succès à {Email}", email);
                return true;
            }
            else
            {
                _logger.LogError("Échec de l'envoi de l'email de notification à {Email}: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de l'email de notification à {Email}", email);
            return false;
        }
    }

    private static string GenerateEmailVerificationTemplate(string name, string verificationUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Vérification d'email</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%); color: #1E2328; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #C89B3C; color: #1E2328; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🏆 LZG Challenge</h1>
        </div>
        <div class=""content"">
            <h2>Bonjour {name},</h2>
            <p>Merci de vous être inscrit à LZG Challenge ! Pour finaliser votre inscription, veuillez vérifier votre adresse email en cliquant sur le bouton ci-dessous :</p>
            <div style=""text-align: center;"">
                <a href=""{verificationUrl}"" class=""button"">Vérifier mon email</a>
            </div>
            <p>Si le bouton ne fonctionne pas, vous pouvez copier et coller ce lien dans votre navigateur :</p>
            <p><a href=""{verificationUrl}"">{verificationUrl}</a></p>
            <p><strong>Ce lien expire dans 24 heures.</strong></p>
            <p>Si vous n'avez pas créé de compte, vous pouvez ignorer cet email.</p>
        </div>
        <div class=""footer"">
            <p>© 2024 LZG Challenge. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GeneratePasswordResetTemplate(string name, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Réinitialisation de mot de passe</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%); color: #1E2328; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #C89B3C; color: #1E2328; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 6px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 LZG Challenge</h1>
        </div>
        <div class=""content"">
            <h2>Bonjour {name},</h2>
            <p>Vous avez demandé la réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour créer un nouveau mot de passe :</p>
            <div style=""text-align: center;"">
                <a href=""{resetUrl}"" class=""button"">Réinitialiser mon mot de passe</a>
            </div>
            <p>Si le bouton ne fonctionne pas, vous pouvez copier et coller ce lien dans votre navigateur :</p>
            <p><a href=""{resetUrl}"">{resetUrl}</a></p>
            <div class=""warning"">
                <p><strong>⚠️ Important :</strong></p>
                <ul>
                    <li>Ce lien expire dans 1 heure</li>
                    <li>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email</li>
                    <li>Votre mot de passe actuel reste inchangé tant que vous n'en créez pas un nouveau</li>
                </ul>
            </div>
        </div>
        <div class=""footer"">
            <p>© 2024 LZG Challenge. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GenerateWelcomeTemplate(string name)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Bienvenue dans LZG Challenge</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%); color: #1E2328; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Bienvenue dans LZG Challenge !</h1>
        </div>
        <div class=""content"">
            <h2>Félicitations {name} !</h2>
            <p>Votre compte a été créé avec succès et votre email a été vérifié. Vous faites maintenant partie de la communauté LZG Challenge !</p>
            <h3>🏆 Qu'est-ce que LZG Challenge ?</h3>
            <p>LZG Challenge est une compétition amicale entre joueurs de League of Legends. Suivez vos performances en SoloQ, comparez vos statistiques et grimpez dans le classement !</p>
            <h3>✨ Fonctionnalités disponibles :</h3>
            <ul>
                <li>📊 Suivi en temps réel de vos statistiques</li>
                <li>🏅 Classement dynamique entre participants</li>
                <li>📈 Historique détaillé de vos parties</li>
                <li>🎯 Statistiques par champion et rôle</li>
            </ul>
            <p>Connectez-vous dès maintenant et commencez à jouer pour voir vos statistiques s'afficher !</p>
        </div>
        <div class=""footer"">
            <p>© 2024 LZG Challenge. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GeneratePasswordChangedTemplate(string name)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Mot de passe modifié</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #C89B3C 0%, #F0E6D2 100%); color: #1E2328; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #666; }}
        .success {{ background: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 6px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 LZG Challenge</h1>
        </div>
        <div class=""content"">
            <h2>Bonjour {name},</h2>
            <div class=""success"">
                <p><strong>✅ Votre mot de passe a été modifié avec succès !</strong></p>
            </div>
            <p>Cette notification confirme que votre mot de passe a été changé le {DateTime.UtcNow:dd/MM/yyyy à HH:mm} UTC.</p>
            <p>Si vous n'êtes pas à l'origine de cette modification, contactez immédiatement notre support et changez votre mot de passe.</p>
            <p>Pour votre sécurité, nous vous recommandons :</p>
            <ul>
                <li>🔒 D'utiliser un mot de passe unique et fort</li>
                <li>🔐 De ne jamais partager vos identifiants</li>
                <li>👀 De vérifier régulièrement l'activité de votre compte</li>
            </ul>
        </div>
        <div class=""footer"">
            <p>© 2024 LZG Challenge. Tous droits réservés.</p>
        </div>
    </div>
</body>
</html>";
    }
}