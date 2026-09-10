using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await db.Users
            .Include(x => x.Collection)
            .Include(x => x.Decks)
            .Include(x => x.Matches)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantCoins(int id, int amount)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        amount = Math.Clamp(amount, -5000, 5000);
        user.Coins = Math.Max(0, user.Coins + amount);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Stanje coina za {user.UserName}: {user.Coins}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(int id)
    {
        var currentUserId = User.GetUserId();
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        if (id == currentUserId)
        {
            TempData["Error"] = "Ne možete promijeniti vlastitu administratorsku ulogu.";
            return RedirectToAction(nameof(Index));
        }

        user.Role = user.Role == "Admin" ? "User" : "Admin";
        await db.SaveChangesAsync();

        TempData["Success"] = $"Uloga korisnika {user.UserName} promijenjena je u {user.Role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.GetUserId();
        if (id == currentUserId)
        {
            TempData["Error"] = "Ne možete obrisati vlastiti administratorski profil.";
            return RedirectToAction(nameof(Index));
        }

        var user = await db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        TempData["Success"] = "Korisnik je obrisan.";
        return RedirectToAction(nameof(Index));
    }
}
