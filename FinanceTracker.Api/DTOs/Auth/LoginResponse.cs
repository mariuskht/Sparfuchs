namespace FinanceTracker.Api.DTOs.Auth;

public class LoginResponse
{
    public string EncryptionSalt { get; set; } = string.Empty;
    public string PasswordWrappedKey { get; set; } = string.Empty;
    public string EncryptedUsername { get; set; } = string.Empty;
    public string EncryptedEmail { get; set; } = string.Empty;
}