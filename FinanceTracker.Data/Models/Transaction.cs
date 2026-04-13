namespace FinanceTracker.Data.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }

    // --- Encrypted (AES-256-GCM) ---
    public string EncryptedAmount { get; set; } = string.Empty; // decimal serialized as string, then encrypted
    public string? EncryptedNote { get; set; }                  // optional transaction label/description

    // --- Not encrypted: needed for server-side sorting and range queries ---
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Category? Category { get; set; }
}
