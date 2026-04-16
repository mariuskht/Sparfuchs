using System.Security.Claims;
using FinanceTracker.Api.DTOs.Accounts;
using FinanceTracker.Data;
using FinanceTracker.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly FinanceTrackerDbContext _db;

    public AccountsController(FinanceTrackerDbContext db) => _db = db;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _db.Accounts
            .Where(a => a.UserId == CurrentUserId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                EncryptedName = a.EncryptedName,
                EncryptedBalance = a.EncryptedBalance,
                Type = (int)a.Type,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _db.Accounts
            .Where(a => a.Id == id && a.UserId == CurrentUserId)
            .Select(a => new AccountDto
            {
                Id = a.Id,
                EncryptedName = a.EncryptedName,
                EncryptedBalance = a.EncryptedBalance,
                Type = (int)a.Type,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
            })
            .FirstOrDefaultAsync();

        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest req)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            EncryptedName = req.EncryptedName,
            EncryptedBalance = req.EncryptedBalance,
            Type = req.Type,
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, new AccountDto
        {
            Id = account.Id,
            EncryptedName = account.EncryptedName,
            EncryptedBalance = account.EncryptedBalance,
            Type = (int)account.Type,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest req)
    {
        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (account is null) return NotFound();

        account.EncryptedName = req.EncryptedName;
        account.EncryptedBalance = req.EncryptedBalance;
        account.Type = (FinanceTracker.Core.Enums.AccountType)req.Type;
        account.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var account = await _db.Accounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (account is null) return NotFound();

        _db.Accounts.Remove(account);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}