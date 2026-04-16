using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Api.Services;

public class PasswordService : IPasswordService
{
    private const int Iterations = 600_000;

    public byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(32);

    public string HashPassword(string password, byte[] salt)
        => Convert.ToBase64String(
            Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, 64));

    public bool VerifyPassword(string password, string hash, byte[] salt)
    {
        var computed = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(hash));
    }

    public string ComputeEmailHmac(string email, byte[] hmacKey)
    {
        using var hmac = new HMACSHA256(hmacKey);
        return Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant())));
    }
}