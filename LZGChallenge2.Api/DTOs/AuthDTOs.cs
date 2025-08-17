using System.ComponentModel.DataAnnotations;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.DTOs;

// Login
public record LoginRequestDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; } = false;
}

public record LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public UserDto User { get; init; } = new();
    public DateTime ExpiresAt { get; init; }
    public string? RefreshToken { get; init; }
}

// Register
public record RegisterRequestDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\.\-_#])[A-Za-z\d@$!%*?&\.\-_#]+$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est requis")]
    [MinLength(2, ErrorMessage = "Le prénom doit contenir au moins 2 caractères")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le nom est requis")]
    [MinLength(2, ErrorMessage = "Le nom doit contenir au moins 2 caractères")]
    public string LastName { get; init; } = string.Empty;
}

public record RegisterResponseDto
{
    public string Message { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

// User
public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public UserRole Role { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

// Email verification
public record VerifyEmailRequestDto
{
    [Required(ErrorMessage = "Le token est requis")]
    public string Token { get; init; } = string.Empty;
}

public record ResendVerificationEmailRequestDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; init; } = string.Empty;
}

// Password reset
public record ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "L'email est requis")]
    [EmailAddress(ErrorMessage = "Format d'email invalide")]
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Le token est requis")]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\.\-_#])[A-Za-z\d@$!%*?&\.\-_#]+$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

// Change password
public record ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Le mot de passe actuel est requis")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
    [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\.\-_#])[A-Za-z\d@$!%*?&\.\-_#]+$",
        ErrorMessage = "Le mot de passe doit contenir au moins une majuscule, une minuscule, un chiffre et un caractère spécial")]
    public string NewPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
    [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas")]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

// Refresh token
public record RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

// Generic response
public record ApiResponseDto
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
    public List<string>? Errors { get; init; }
}