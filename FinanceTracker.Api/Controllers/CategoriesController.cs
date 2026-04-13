using System.Security.Claims;
using FinanceTracker.Api.DTOs.Categories;
using FinanceTracker.Data;
using FinanceTracker.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly FinanceTrackerDbContext _db;

    public CategoriesController(FinanceTrackerDbContext db) => _db = db;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns default (system) categories + the current user's own categories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .Where(c => c.IsDefault || c.UserId == CurrentUserId)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                IsDefault = c.IsDefault,
                Name = c.Name,
                Color = c.Color,
                EncryptedName = c.EncryptedName,
                EncryptedColor = c.EncryptedColor,
                EncryptedDescription = c.EncryptedDescription,
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            IsDefault = false,
            EncryptedName = req.EncryptedName,
            EncryptedColor = req.EncryptedColor,
            EncryptedDescription = req.EncryptedDescription,
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            IsDefault = false,
            EncryptedName = category.EncryptedName,
            EncryptedColor = category.EncryptedColor,
            EncryptedDescription = category.EncryptedDescription,
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest req)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId && !c.IsDefault);

        if (category is null) return NotFound();

        category.EncryptedName = req.EncryptedName;
        category.EncryptedColor = req.EncryptedColor;
        category.EncryptedDescription = req.EncryptedDescription;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId && !c.IsDefault);

        if (category is null) return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
