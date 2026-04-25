using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Data.Encryption;

/// <summary>
/// Encrypts sensitive user data with AES-256-GCM (authenticated encryption).
/// Keys are derived from the user's password with PBKDF2-SHA512 and never stored.
/// </summary>
public class AesGcmEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;       // 96-bit GCM nonce
    private const int TagSize = 16;         // 128-bit authentication tag
    private const int KeySize = 32;         // 256-bit AES key
    private const int Pbkdf2Iterations = 600_000; // OWASP recommendation for PBKDF2-SHA512

    public string Encrypt(string plaintext, byte[] key)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Layout: nonce(12) | tag(16) | ciphertext(n)
        var result = new byte[NonceSize + TagSize + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, NonceSize);
        ciphertext.CopyTo(result, NonceSize + TagSize);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertextEncoded, byte[] key)
    {
        ArgumentException.ThrowIfNullOrEmpty(ciphertextEncoded);

        var data = Convert.FromBase64String(ciphertextEncoded);

        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid ciphertext: too short.");

        var nonce = data[..NonceSize];
        var tag = data[NonceSize..(NonceSize + TagSize)];
        var ciphertext = data[(NonceSize + TagSize)..];
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    public byte[] DeriveEncryptionKey(string password, byte[] salt)
        => Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA512, KeySize);

    public byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(32);

    public string HashPassword(string password, byte[] salt)
        => Convert.ToBase64String(
            Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA512, 64));

    public bool VerifyPassword(string password, string hash, byte[] salt)
    {
        var computed = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(hash));
    }

    public string ComputeEmailHmac(string email, byte[] serverHmacKey)
    {
        using var hmac = new HMACSHA256(serverHmacKey);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToBase64String(hashBytes);
    }
}
