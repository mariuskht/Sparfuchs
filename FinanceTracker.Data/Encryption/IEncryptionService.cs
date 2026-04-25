namespace FinanceTracker.Data.Encryption;

public interface IEncryptionService
{
    /// <summary>Encrypts plaintext with AES-256-GCM using the given key. Returns Base64-encoded nonce+tag+ciphertext.</summary>
    string Encrypt(string plaintext, byte[] key);

    /// <summary>Decrypts AES-256-GCM ciphertext. Throws if tampered or wrong key.</summary>
    string Decrypt(string ciphertext, byte[] key);

    /// <summary>Derives a 256-bit encryption key from a password using PBKDF2-SHA512.</summary>
    byte[] DeriveEncryptionKey(string password, byte[] salt);

    /// <summary>Generates a cryptographically random 32-byte salt.</summary>
    byte[] GenerateSalt();

    /// <summary>Hashes a password with PBKDF2-SHA512 for authentication.</summary>
    string HashPassword(string password, byte[] salt);

    /// <summary>Verifies a password against its stored hash in constant time.</summary>
    bool VerifyPassword(string password, string hash, byte[] salt);

    /// <summary>
    /// Computes an HMAC-SHA256 of the email (lowercased) using a server-side secret.
    /// Used to look up users by email without storing the email in plaintext.
    /// </summary>
    string ComputeEmailHmac(string email, byte[] serverHmacKey);
}
