using System.ComponentModel.DataAnnotations;
using FinanceTracker.Core.Enums;

namespace FinanceTracker.Api.DTOs.Accounts;

public class CreateAccountRequest
{
    [Required] public string EncryptedName { get; set; } = string.Empty;
    [Required] public string EncryptedBalance { get; set; } = string.Empty;
    [Required] public AccountType Type { get; set; }
}