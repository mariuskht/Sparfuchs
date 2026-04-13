using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Accounts;

public class UpdateAccountRequest
{
    [Required] public string EncryptedName { get; set; } = string.Empty;
    [Required] public string EncryptedBalance { get; set; } = string.Empty;
    [Required] public int Type { get; set; }
}
