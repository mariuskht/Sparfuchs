using FinanceTracker.Api.DTOs.Auth;
using FinanceTracker.Api.Services;
using FinanceTracker.Data;
using FinanceTracker.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FinanceTrackerDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly ITokenService _tokens;
    private readonly byte[] _hmacKey;

    public AuthController(
        FinanceTrackerDbContext db,
        IPasswordService passwords,
        ITokenService tokens,
        byte[] hmacKey)
    {
        _db = db;
        _passwords = passwords;
        _tokens = tokens;
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
            EncryptedEmail = req.EncryptedEmail,
            EncryptedUsername = req.EncryptedUsername,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { token = _tokens.GenerateToken(user.Id) });
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

        return Ok(new LoginResponse
        {
            Token = _tokens.GenerateToken(user.Id),
            EncryptionSalt = user.EncryptionSalt,
            EncryptedUsername = user.EncryptedUsername,
            EncryptedEmail = user.EncryptedEmail,
        });
    }
}
