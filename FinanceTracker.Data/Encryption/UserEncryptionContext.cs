using System.Security.Cryptography;

namespace FinanceTracker.Data.Encryption;

/// <summary>
/// Holds the current user's in-memory encryption key for the duration of a session.
/// Registered as Scoped — the key lives only in RAM, is never written to disk or logs,
/// and is zeroed when the session ends.
/// </summary>
public sealed class UserEncryptionContext : IDisposable
{
    private byte[]? _encryptionKey;
    private bool _disposed;

    public bool IsInitialized => _encryptionKey != null;

    /// <summary>Returns the current user's encryption key. Throws if not yet initialized (user not logged in).</summary>
    public byte[] EncryptionKey
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _encryptionKey
                ?? throw new InvalidOperationException(
                    "Encryption context not initialized. The user must be authenticated first.");
        }
    }

    /// <summary>Called after successful login: stores the derived key in memory.</summary>
    public void Initialize(byte[] key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Clear();
        _encryptionKey = (byte[])key.Clone();
    }

    /// <summary>Zeroes the key in memory (e.g. on logout).</summary>
    public void Clear()
    {
        if (_encryptionKey != null)
        {
            CryptographicOperations.ZeroMemory(_encryptionKey);
            _encryptionKey = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
    }
}
