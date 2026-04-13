namespace FinanceTracker.Data.Models;

public class User
{
    public Guid Id { get; set; }

    // --- Authentication (used only for login verification) ---
    public string PasswordHash { get; set; } = string.Empty;
    public string AuthSalt { get; set; } = string.Empty;         // salt for PBKDF2 password hash

    // --- Key derivation (used to re-derive the in-memory encryption key after login) ---
    public string EncryptionSalt { get; set; } = string.Empty;   // salt for PBKDF2 key derivation

    // --- Pseudonymous lookup (not reversible, no plaintext stored) ---
    // HMAC-SHA256(email.toLower(), serverHmacKey) — lets us find users by email at login
    // without ever storing the email in plaintext in the database.
    public string EmailHmac { get; set; } = string.Empty;

    // --- Encrypted personal data (AES-256-GCM, key never stored) ---
    public string EncryptedUsername { get; set; } = string.Empty;
    public string EncryptedEmail { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
