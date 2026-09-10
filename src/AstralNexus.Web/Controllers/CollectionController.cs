using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize]
public class CollectionController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search, string? type, int? value)
    {
        var userId = User.GetUserId();

        var query = db.UserCards
            .Where(x => x.UserId == userId && x.Quantity > 0 && x.Card.IsActive)
            .Include(x => x.Card)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Card.Name.Contains(term) ||
                x.Card.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(x => x.Card.Type == type);

        if (value.HasValue)
            query = query.Where(x => x.Card.BaseValue == value.Value);

        var model = new CollectionViewModel
        {
            Cards = await query
                .OrderBy(x => x.Card.Type)
                .ThenByDescending(x => x.Card.BaseValue)
                .ThenBy(x => x.Card.Name)
                .ToListAsync(),
            Types = await db.UserCards
                .Where(x => x.UserId == userId && x.Quantity > 0 && x.Card.IsActive)
                .Select(x => x.Card.Type)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),
            Values = await db.UserCards
                .Where(x => x.UserId == userId && x.Quantity > 0 && x.Card.IsActive)
                .Select(x => x.Card.BaseValue)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),
            Search = search ?? string.Empty,
            Type = type ?? string.Empty,
            Value = value,
            Coins = await db.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Coins)
                .SingleAsync()
        };

        return View(model);
    }
}
