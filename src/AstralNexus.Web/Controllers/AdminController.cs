using AstralNexus.Web.Data;
using AstralNexus.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            Users = await db.Users.CountAsync(),
            Cards = await db.Cards.CountAsync(),
            Decks = await db.Decks.CountAsync(),
            Matches = await db.Matches.CountAsync(),
            PackOpenings = await db.PackOpenings.CountAsync(),
            RecentUsers = await db.Users
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(5)
                .ToListAsync(),
            RecentMatches = await db.Matches
                .Include(x => x.User)
                .Include(x => x.Deck)
                .OrderByDescending(x => x.PlayedAtUtc)
                .Take(8)
                .ToListAsync()
        };

        return View(model);
    }
}
