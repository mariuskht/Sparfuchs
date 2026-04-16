namespace FinanceTracker.Api.Services;

public interface ITokenService
{
    string GenerateToken(Guid userId);
}
