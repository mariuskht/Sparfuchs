namespace FinanceTracker.Api.Services;

public interface IPasswordService
{
    byte[] GenerateSalt();
    string HashPassword(string password, byte[] salt);
    bool VerifyPassword(string password, string hash, byte[] salt);
    string ComputeEmailHmac(string email, byte[] hmacKey);
}
