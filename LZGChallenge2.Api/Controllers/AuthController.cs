using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LZGChallenge2.Api.DTOs;
using LZGChallenge2.Api.Services;

namespace LZGChallenge2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Inscription d'un nouvel utilisateur
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var (success, message, errors) = await _authService.RegisterAsync(request, ipAddress);

        if (success)
        {
            return Ok(new { message });
        }

        return BadRequest(new { message, errors });
    }

    /// <summary>
    /// Connexion d'un utilisateur
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();

        var (success, response, errors) = await _authService.LoginAsync(request, ipAddress, userAgent);

        if (success && response != null)
        {
            SetRefreshTokenCookie(response.RefreshToken!);
            return Ok(new 
            { 
                accessToken = response.AccessToken,
                user = response.User,
                expiresAt = response.ExpiresAt
            });
        }

        return Unauthorized(new { message = "Échec de la connexion", errors });
    }

    /// <summary>
    /// Rafraîchissement du token d'accès
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Token de rafraîchissement manquant" });
        }

        var ipAddress = GetIpAddress();
        var userAgent = Request.Headers.UserAgent.ToString();

        var (success, response, error) = await _authService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);

        if (success && response != null)
        {
            SetRefreshTokenCookie(response.RefreshToken!);
            return Ok(new 
            { 
                accessToken = response.AccessToken,
                user = response.User,
                expiresAt = response.ExpiresAt
            });
        }

        return Unauthorized(new { message = error });
    }

    /// <summary>
    /// Déconnexion de l'utilisateur
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = GetRefreshTokenFromCookie();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var ipAddress = GetIpAddress();
            await _authService.LogoutAsync(refreshToken, ipAddress);
        }

        ClearRefreshTokenCookie();
        return Ok(new { message = "Déconnexion réussie" });
    }

    /// <summary>
    /// Vérification d'email
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var (success, message) = await _authService.VerifyEmailAsync(request.Token, ipAddress);

        if (success)
        {
            return Ok(new { message });
        }

        return BadRequest(new { message });
    }

    /// <summary>
    /// Renvoi d'email de vérification
    /// </summary>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationEmailRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var (success, message) = await _authService.ResendVerificationEmailAsync(request.Email, ipAddress);

        // Toujours retourner succès pour des raisons de sécurité
        return Ok(new { message });
    }

    /// <summary>
    /// Demande de réinitialisation de mot de passe
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var (success, message) = await _authService.ForgotPasswordAsync(request.Email, ipAddress);

        // Toujours retourner succès pour des raisons de sécurité
        return Ok(new { message });
    }

    /// <summary>
    /// Réinitialisation du mot de passe
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = GetIpAddress();
        var (success, message, errors) = await _authService.ResetPasswordAsync(request, ipAddress);

        if (success)
        {
            return Ok(new { message });
        }

        return BadRequest(new { message, errors });
    }

    /// <summary>
    /// Changement de mot de passe (utilisateur connecté)
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var (success, message, errors) = await _authService.ChangePasswordAsync(userId, request);

        if (success)
        {
            return Ok(new { message });
        }

        return BadRequest(new { message, errors });
    }

    /// <summary>
    /// Récupération du profil utilisateur actuel
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Utilisateur introuvable" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Révocation de tous les tokens de l'utilisateur (déconnexion partout)
    /// </summary>
    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var revokedCount = await _authService.RevokeAllUserTokensAsync(userId, "Révocation manuelle par l'utilisateur");
        
        ClearRefreshTokenCookie();
        
        return Ok(new { message = $"{revokedCount} sessions révoquées avec succès" });
    }

    #region Méthodes utilitaires

    private string GetIpAddress()
    {
        // Vérifier si la requête passe par un proxy
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
        }

        if (Request.Headers.ContainsKey("X-Real-IP"))
        {
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetRefreshTokenFromCookie()
    {
        return Request.Cookies["refreshToken"];
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // HTTPS uniquement
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7), // 7 jours de durée de vie
            Path = "/",
            IsEssential = true
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(-1), // Expiration dans le passé
            Path = "/",
            IsEssential = true
        };

        Response.Cookies.Append("refreshToken", "", cookieOptions);
    }

    #endregion
}