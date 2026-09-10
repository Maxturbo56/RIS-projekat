using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize]
public class DashboardController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == userId);

        var recentMatches = await db.Matches
            .Where(x => x.UserId == userId)
            .Include(x => x.Deck)
            .OrderByDescending(x => x.PlayedAtUtc)
            .Take(5)
            .ToListAsync();

        var featured = await db.UserCards
            .Where(x => x.UserId == userId && x.Card.IsActive)
            .OrderByDescending(x => x.AcquiredAtUtc)
            .Select(x => x.Card)
            .Take(6)
            .ToListAsync();

        var model = new DashboardViewModel
        {
            UserName = user.UserName,
            Coins = user.Coins,
            OwnedCards = await db.UserCards.CountAsync(x => x.UserId == userId && x.Quantity > 0),
            TotalCardCopies = await db.UserCards
                .Where(x => x.UserId == userId)
                .SumAsync(x => (int?)x.Quantity) ?? 0,
            Decks = await db.Decks.CountAsync(x => x.UserId == userId),
            ValidDecks = await db.Decks.CountAsync(x => x.UserId == userId && x.IsValid),
            Wins = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Win"),
            Losses = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Loss"),
            RecentMatches = recentMatches,
            FeaturedCards = featured
        };

        return View(model);
    }
}
