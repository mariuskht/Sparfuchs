using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Categories;

public class UpdateCategoryRequest
{
    [Required] public string EncryptedName { get; set; } = string.Empty;
    public string? EncryptedColor { get; set; }
    public string? EncryptedDescription { get; set; }
}
