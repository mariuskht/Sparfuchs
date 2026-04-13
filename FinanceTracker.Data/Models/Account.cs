using FinanceTracker.Core.Enums;

namespace FinanceTracker.Data.Models;

public class Account
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // --- Encrypted (AES-256-GCM) ---
    public string EncryptedName { get; set; } = string.Empty;
    public string EncryptedBalance { get; set; } = string.Empty; // decimal serialized as string, then encrypted

    public AccountType Type { get; set; } = AccountType.Checking;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
