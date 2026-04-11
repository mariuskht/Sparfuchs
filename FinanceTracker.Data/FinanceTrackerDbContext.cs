using FinanceTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public class FinanceTrackerDbContext : DbContext
{
    public FinanceTrackerDbContext(DbContextOptions<FinanceTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(255);
            entity.Property(a => a.Balance).HasPrecision(18, 2);
            entity.HasMany(a => a.Transactions).WithOne(t => t.Account).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => new { c.AccountId, c.Name }).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.HasOne(t => t.Category).WithMany(c => c.Transactions).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(t => t.Account).WithMany(a => a.Transactions).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.AccountId);
            entity.HasIndex(t => t.TransactionDate);
        });
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Gehalt", IsDefault = true, Color = "#4CAF50" },
            new() { Id = 2, Name = "Lebensmittel", IsDefault = true, Color = "#FF9800" },
            new() { Id = 3, Name = "Transport", IsDefault = true, Color = "#2196F3" },
            new() { Id = 4, Name = "Miete", IsDefault = true, Color = "#9C27B0" },
            new() { Id = 5, Name = "Versicherung", IsDefault = true, Color = "#F44336" },
            new() { Id = 6, Name = "Freizeit", IsDefault = true, Color = "#00BCD4" },
            new() { Id = 7, Name = "Sonstiges", IsDefault = true, Color = "#9E9E9E" }
        };

        modelBuilder.Entity<Category>().HasData(categories);
    }
}