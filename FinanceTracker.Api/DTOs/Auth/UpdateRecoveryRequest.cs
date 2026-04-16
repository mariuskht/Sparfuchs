using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs.Auth;

public class UpdateRecoveryRequest
{
    [Required] public string RecoverySalt      { get; set; } = string.Empty;
    [Required] public string RecoveryWrappedKey { get; set; } = string.Empty;
    [Required] public string RecoveryVerifier  { get; set; } = string.Empty;
}
