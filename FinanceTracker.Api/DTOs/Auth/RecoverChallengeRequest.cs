using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Auth;

public class RecoverChallengeRequest
{
    /// <summary>Plaintext email — HMAC is computed server-side using the secret HMAC_KEY from .env.</summary>
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
}
