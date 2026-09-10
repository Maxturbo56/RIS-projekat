using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize]
public class ProfileController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var model = await BuildProfileAsync(userId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var userId = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == userId);

        if (!ModelState.IsValid)
        {
            var current = await BuildProfileAsync(userId);
            model.UserName = current.UserName;
            model.Email = current.Email;
            model.Coins = current.Coins;
            model.OwnedCards = current.OwnedCards;
            model.Decks = current.Decks;
            model.Wins = current.Wins;
            model.Losses = current.Losses;
            model.CreatedAtUtc = current.CreatedAtUtc;
            return View(model);
        }

        user.DisplayName = model.DisplayName.Trim();
        user.Bio = model.Bio?.Trim() ?? string.Empty;
        await db.SaveChangesAsync();

        TempData["Success"] = "Profil je ažuriran.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var userId = User.GetUserId();
        var theme = await db.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Theme)
            .SingleAsync();

        return View(new SettingsViewModel { Theme = theme });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SettingsViewModel model)
    {
        var allowed = new[] { "Dark", "Light" };
        if (!allowed.Contains(model.Theme))
            ModelState.AddModelError(nameof(model.Theme), "Nepoznata tema.");

        if (!ModelState.IsValid)
            return View(model);

        var userId = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.Theme = model.Theme;
        await db.SaveChangesAsync();

        Response.Cookies.Append(
            "AstralNexus.Theme",
            model.Theme,
            new CookieOptions
            {
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

        TempData["Success"] = "Tema je spremljena.";
        return RedirectToAction(nameof(Settings));
    }

    private async Task<ProfileViewModel> BuildProfileAsync(int userId)
    {
        var user = await db.Users.SingleAsync(x => x.Id == userId);

        return new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName,
            Bio = user.Bio,
            Coins = user.Coins,
            CreatedAtUtc = user.CreatedAtUtc,
            OwnedCards = await db.UserCards.CountAsync(x => x.UserId == userId && x.Quantity > 0),
            Decks = await db.Decks.CountAsync(x => x.UserId == userId),
            Wins = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Win"),
            Losses = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Loss")
        };
    }
}
