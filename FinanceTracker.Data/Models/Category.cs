namespace FinanceTracker.Data.Models;

public class Category
{
    public Guid Id { get; set; }

    // Null for system-wide default categories, set for user-created ones
    public Guid? UserId { get; set; }

    public bool IsDefault { get; set; }

    // Plaintext — only for default (system) categories
    public string? Name { get; set; }
    public string? Color { get; set; }
    public string? Description { get; set; }

    // Encrypted — only for user-created categories
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
