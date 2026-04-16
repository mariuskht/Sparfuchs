namespace FinanceTracker.Data.Models;

public class User
{
    public Guid Id { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
    public string AuthSalt { get; set; } = string.Empty;

    // Two wrapped copies of the master key — one per unlock path (password / recovery phrase)
    public string EncryptionSalt { get; set; } = string.Empty;
    public string PasswordWrappedKey { get; set; } = string.Empty;
    public string RecoverySalt { get; set; } = string.Empty;
    public string RecoveryWrappedKey { get; set; } = string.Empty;
    public string RecoveryVerifier { get; set; } = string.Empty;

    // HMAC-SHA256(email, serverKey) — enables login lookup without storing plaintext email
    public string EmailHmac { get; set; } = string.Empty;

    public string EncryptedUsername { get; set; } = string.Empty;
    public string EncryptedEmail { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
