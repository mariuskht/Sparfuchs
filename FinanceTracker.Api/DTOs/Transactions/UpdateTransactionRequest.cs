using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Transactions;

public class UpdateTransactionRequest
{
    [Required] public Guid AccountId { get; set; }
    [Required] public Guid CategoryId { get; set; }
    [Required] public string EncryptedAmount { get; set; } = string.Empty;
    public string? EncryptedNote { get; set; }
    [Required] public DateTime TransactionDate { get; set; }
}
