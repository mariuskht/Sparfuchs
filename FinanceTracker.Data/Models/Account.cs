using FinanceTracker.Core.Enums;

namespace FinanceTracker.Data.Models;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; } = AccountType.Checking;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}