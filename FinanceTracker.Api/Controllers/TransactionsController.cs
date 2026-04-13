using System.Security.Claims;
using FinanceTracker.Api.DTOs.Transactions;
using FinanceTracker.Data;
using FinanceTracker.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly FinanceTrackerDbContext _db;

    public TransactionsController(FinanceTrackerDbContext db) => _db = db;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? accountId)
    {
        var query = _db.Transactions.Where(t => t.UserId == CurrentUserId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                EncryptedAmount = t.EncryptedAmount,
                EncryptedNote = t.EncryptedNote,
                TransactionDate = t.TransactionDate,
            })
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var t = await _db.Transactions
            .Where(t => t.Id == id && t.UserId == CurrentUserId)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                EncryptedAmount = t.EncryptedAmount,
                EncryptedNote = t.EncryptedNote,
                TransactionDate = t.TransactionDate,
            })
            .FirstOrDefaultAsync();

        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest req)
    {
        // Verify account belongs to current user
        var accountExists = await _db.Accounts
            .AnyAsync(a => a.Id == req.AccountId && a.UserId == CurrentUserId);
        if (!accountExists) return BadRequest(new { message = "Account not found." });

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            AccountId = req.AccountId,
            CategoryId = req.CategoryId,
            EncryptedAmount = req.EncryptedAmount,
            EncryptedNote = req.EncryptedNote,
            TransactionDate = req.TransactionDate,
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, new TransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            CategoryId = transaction.CategoryId,
            EncryptedAmount = transaction.EncryptedAmount,
            EncryptedNote = transaction.EncryptedNote,
            TransactionDate = transaction.TransactionDate,
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest req)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction is null) return NotFound();

        transaction.AccountId = req.AccountId;
        transaction.CategoryId = req.CategoryId;
        transaction.EncryptedAmount = req.EncryptedAmount;
        transaction.EncryptedNote = req.EncryptedNote;
        transaction.TransactionDate = req.TransactionDate;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (transaction is null) return NotFound();

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
