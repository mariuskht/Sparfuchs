using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    // Encrypted with the key derived client-side — server never sees plaintext
    [Required] public string EncryptedUsername { get; set; } = string.Empty;
    [Required] public string EncryptedEmail { get; set; } = string.Empty;

    // Salt used by the client to derive the AES key (PBKDF2). Server stores it
    // so the client can re-derive the key on future logins.
    [Required] public string EncryptionSalt { get; set; } = string.Empty;
}
