namespace FinanceTracker.Data.Models;

public class Account
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
}