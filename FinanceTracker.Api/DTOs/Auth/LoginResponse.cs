namespace FinanceTracker.Api.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    // Returned so the client can re-derive the AES encryption key from password + salt
    public string EncryptionSalt { get; set; } = string.Empty;

    // Encrypted user info — client decrypts these after deriving the key
    public string EncryptedUsername { get; set; } = string.Empty;
    public string EncryptedEmail { get; set; } = string.Empty;
}
