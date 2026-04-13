namespace FinanceTracker.Api.DTOs.Transactions;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public string EncryptedAmount { get; set; } = string.Empty;
    public string? EncryptedNote { get; set; }
    public DateTime TransactionDate { get; set; }
}
