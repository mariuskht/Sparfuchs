namespace FinanceTracker.Api.DTOs.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public bool IsDefault { get; set; }

    // Populated for default (system) categories — not personal data
    public string? Name { get; set; }
    public string? Color { get; set; }

    // Populated for user-created categories — encrypted client-side
    public string? EncryptedName { get; set; }
    public string? EncryptedColor { get; set; }
    public string? EncryptedDescription { get; set; }
}
