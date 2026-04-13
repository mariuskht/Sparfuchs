using FinanceTracker.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data;

public class FinanceTrackerDbContext : DbContext
{
    public FinanceTrackerDbContext(DbContextOptions<FinanceTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();

            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.AuthSalt).IsRequired();
            entity.Property(u => u.EncryptionSalt).IsRequired();

            // EmailHmac is used for login lookup — unique, but reveals nothing about the email.
            entity.Property(u => u.EmailHmac).IsRequired();
            entity.HasIndex(u => u.EmailHmac).IsUnique();

            // Encrypted fields — no max length since Base64 ciphertext length varies.
            entity.Property(u => u.EncryptedUsername).IsRequired();
            entity.Property(u => u.EncryptedEmail).IsRequired();
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();

            entity.Property(a => a.EncryptedName).IsRequired();
            entity.Property(a => a.EncryptedBalance).IsRequired();

            entity.HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();

            // Name is only set for default categories (IsDefault = true) — index for fast lookup.
            entity.HasIndex(c => c.Name);

            // Optional: user-created categories have UserId set; default categories have UserId = null.
            entity.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Account)
                .WithMany()
                .HasForeignKey(c => c.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();

            entity.Property(t => t.EncryptedAmount).IsRequired();

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.AccountId);
            entity.HasIndex(t => t.TransactionDate);
        });

        SeedDefaultCategories(modelBuilder);
    }

    private static void SeedDefaultCategories(ModelBuilder modelBuilder)
    {
        // Default categories are system-wide, not tied to any user (UserId = null).
        // Names and colors are NOT personal data — no encryption needed.
        // Static timestamps required: EF throws if DateTime.UtcNow is used in HasData.
        var seededAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Gehalt",       Color = "#4CAF50", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Lebensmittel", Color = "#FF9800", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Transport",    Color = "#2196F3", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Miete",        Color = "#9C27B0", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Versicherung", Color = "#F44336", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Freizeit",     Color = "#00BCD4", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt },
            new Category { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "Sonstiges",    Color = "#9E9E9E", IsDefault = true, CreatedAt = seededAt, UpdatedAt = seededAt }
        );
    }
}
