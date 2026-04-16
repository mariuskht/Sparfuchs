using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Accounts;

public class UpdateAccountRequest
{
    [Required] public string EncryptedName { get; set; } = string.Empty;
    [Required] public string EncryptedBalance { get; set; } = string.Empty;
    [Required]
    [EnumDataType(typeof(AccountType))]
    public AccountType Type { get; set; }
}
