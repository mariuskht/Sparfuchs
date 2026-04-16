using System.Security.Claims;
using System.Security.Cryptography;
using FinanceTracker.Api.DTOs.Auth;
using FinanceTracker.Api.Services;
using FinanceTracker.Data;
using FinanceTracker.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string CookieName = "ft_auth";

    private readonly FinanceTrackerDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly byte[] _hmacKey;

    public AuthController(
        FinanceTrackerDbContext db,
        IPasswordService passwords,
        ITokenService tokens,
        IConfiguration config,
        IWebHostEnvironment env,
        byte[] hmacKey)
    {
        _db = db;
        _passwords = passwords;
        _tokens = tokens;
        _config = config;
        _env = env;
        _hmacKey = hmacKey;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var emailHmac = _passwords.ComputeEmailHmac(req.Email, _hmacKey);

        if (await _db.Users.AnyAsync(u => u.EmailHmac == emailHmac))
            return Conflict(new { message = "An account with this email already exists." });

        var authSalt = _passwords.GenerateSalt();
        var passwordHash = _passwords.HashPassword(req.Password, authSalt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            EmailHmac = emailHmac,
            PasswordHash = passwordHash,
            AuthSalt = Convert.ToBase64String(authSalt),
            EncryptionSalt = req.EncryptionSalt,
            PasswordWrappedKey = req.PasswordWrappedKey,
            RecoverySalt = req.RecoverySalt,
            RecoveryWrappedKey = req.RecoveryWrappedKey,
            RecoveryVerifier = req.RecoveryVerifier,
            EncryptedEmail = req.EncryptedEmail,
            EncryptedUsername = req.EncryptedUsername,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        SetAuthCookie(_tokens.GenerateToken(user.Id));

        return Ok(new LoginResponse
        {
            EncryptionSalt = user.EncryptionSalt,
            PasswordWrappedKey = user.PasswordWrappedKey,
            EncryptedUsername = user.EncryptedUsername,
            EncryptedEmail = user.EncryptedEmail,
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var emailHmac = _passwords.ComputeEmailHmac(req.Email, _hmacKey);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailHmac == emailHmac);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var authSalt = Convert.FromBase64String(user.AuthSalt);
        if (!_passwords.VerifyPassword(req.Password, user.PasswordHash, authSalt))
            return Unauthorized(new { message = "Invalid credentials." });

        SetAuthCookie(_tokens.GenerateToken(user.Id));

        return Ok(new LoginResponse
        {
            EncryptionSalt = user.EncryptionSalt,
            PasswordWrappedKey = user.PasswordWrappedKey,
            EncryptedUsername = user.EncryptedUsername,
            EncryptedEmail = user.EncryptedEmail,
        });
    }

    // Returns encrypted user data for session restore after a page refresh (JWT cookie still valid)
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);

        if (user is null) return Unauthorized();

        return Ok(new LoginResponse
        {
            EncryptionSalt = user.EncryptionSalt,
            PasswordWrappedKey = user.PasswordWrappedKey,
            EncryptedUsername = user.EncryptedUsername,
            EncryptedEmail = user.EncryptedEmail,
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(CookieName);
        return NoContent();
    }

    // Step 1: return recoverySalt for the given email.
    // Always returns 200 even for unknown emails to prevent user enumeration.
    [HttpPost("recover/challenge")]
    public async Task<IActionResult> RecoverChallenge([FromBody] RecoverChallengeRequest req)
    {
        var emailHmac = _passwords.ComputeEmailHmac(req.Email, _hmacKey);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailHmac == emailHmac);
        var recoverySalt = user?.RecoverySalt ?? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return Ok(new { recoverySalt });
    }

    // Step 2: verify PBKDF2(recoveryPhrase, recoverySalt), issue cookie, return recovery-wrapped key.
    [HttpPost("recover/verify")]
    public async Task<IActionResult> RecoverVerify([FromBody] RecoverVerifyRequest req)
    {
        var emailHmac = _passwords.ComputeEmailHmac(req.Email, _hmacKey);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailHmac == emailHmac);

        if (user is null)
            return Unauthorized(new { message = "Invalid recovery credentials." });

        // Timing-safe comparison to prevent timing attacks on the verifier
        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(user.RecoveryVerifier),
                Convert.FromBase64String(req.RecoveryVerifier)))
            return Unauthorized(new { message = "Invalid recovery credentials." });

        SetAuthCookie(_tokens.GenerateToken(user.Id));

        return Ok(new RecoverVerifyResponse
        {
            EncryptionSalt = user.EncryptionSalt,
            PasswordWrappedKey = user.PasswordWrappedKey,
            EncryptedUsername = user.EncryptedUsername,
            EncryptedEmail = user.EncryptedEmail,
            RecoverySalt = user.RecoverySalt,
            RecoveryWrappedKey = user.RecoveryWrappedKey,
        });
    }

    [Authorize]
    [HttpPost("recovery")]
    public async Task<IActionResult> UpdateRecovery([FromBody] UpdateRecoveryRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);

        if (user is null) return Unauthorized();

        user.RecoverySalt = req.RecoverySalt;
        user.RecoveryWrappedKey = req.RecoveryWrappedKey;
        user.RecoveryVerifier = req.RecoveryVerifier;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private void SetAuthCookie(string token)
    {
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;

        Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes),
            Path = "/",
        });
    }
}
