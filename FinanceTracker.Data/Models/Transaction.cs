namespace FinanceTracker.Data.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }

    public string EncryptedAmount { get; set; } = string.Empty;
    public string? EncryptedNote { get; set; }

    // Stored as plaintext to allow server-side sorting and date range queries
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Category? Category { get; set; }
}
