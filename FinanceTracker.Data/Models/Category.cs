namespace FinanceTracker.Data.Models;

public class Category
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool IsDefault { get; set; }
    public string? Color { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? AccountId { get; set; }
    public Account? Account { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}