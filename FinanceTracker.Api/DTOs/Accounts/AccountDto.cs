namespace FinanceTracker.Api.DTOs.Accounts;

public class AccountDto
{
    public Guid Id { get; set; }
    public string EncryptedName { get; set; } = string.Empty;
    public string EncryptedBalance { get; set; } = string.Empty;
    public int Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
