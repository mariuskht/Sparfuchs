using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Auth;

public class RecoverVerifyRequest
{
    /// <summary>Plaintext email — HMAC is computed server-side using the secret HMAC_KEY from .env.</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2(recoveryPhrase, RecoverySalt) — proves phrase knowledge without revealing it.</summary>
    [Required] public string RecoveryVerifier { get; set; } = string.Empty;
}
