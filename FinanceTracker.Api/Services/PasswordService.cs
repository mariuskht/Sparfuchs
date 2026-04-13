using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Api.Services;

/// <summary>
/// Server-side password hashing only.
/// Encryption of user data happens exclusively in the Angular client (Web Crypto API).
/// The server never derives or stores an encryption key.
/// </summary>
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

    /// <summary>
    /// HMAC-SHA256 of lowercased email using a server secret.
    /// Allows user lookup without storing the email in plaintext.
    /// </summary>
    public string ComputeEmailHmac(string email, byte[] hmacKey)
    {
        using var hmac = new HMACSHA256(hmacKey);
        return Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant())));
    }
}
