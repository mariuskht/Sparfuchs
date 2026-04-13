namespace FinanceTracker.Data.Models;

public class Category
{
    public Guid Id { get; set; }

    /// <summary>
    /// Null for default (system) categories — they are not personal data.
    /// Set for user-created categories (IsDefault = false).
    /// </summary>
    public Guid? UserId { get; set; }

    public bool IsDefault { get; set; }

    // --- Plaintext: only populated for default (system) categories (IsDefault = true) ---
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }

    // --- Encrypted (AES-256-GCM): only populated for user-created categories (IsDefault = false) ---
    public string? EncryptedName { get; set; }
    public string? EncryptedColor { get; set; }
    public string? EncryptedDescription { get; set; }

    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
