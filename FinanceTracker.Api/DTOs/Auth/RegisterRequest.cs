using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Auth;

public class RegisterRequest
{
    /// <summary>Plaintext email — HMAC is computed server-side using the secret HMAC_KEY from .env.</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    // Encrypted with the master key — server never sees plaintext
    [Required] public string EncryptedUsername { get; set; } = string.Empty;
    [Required] public string EncryptedEmail    { get; set; } = string.Empty;

    // Master key wrapping — password path
    [Required] public string EncryptionSalt    { get; set; } = string.Empty;
    [Required] public string PasswordWrappedKey { get; set; } = string.Empty;

    // Master key wrapping — recovery path
    [Required] public string RecoverySalt      { get; set; } = string.Empty;
    [Required] public string RecoveryWrappedKey { get; set; } = string.Empty;
    [Required] public string RecoveryVerifier  { get; set; } = string.Empty;
}
