namespace FinanceTracker.Api.DTOs.Auth;

/// <summary>
/// Extended response for the recovery flow.
/// Includes the recovery-wrapped master key so the client can unwrap it with the recovery phrase.
/// </summary>
public class RecoverVerifyResponse : LoginResponse
{
    public string RecoverySalt      { get; set; } = string.Empty;
    public string RecoveryWrappedKey { get; set; } = string.Empty;
}
