using System.Security.Claims;
using AstralNexus.Web.Data;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Models.ViewModels;
using AstralNexus.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

public class AccountController(
    AppDbContext db,
    PasswordService passwords) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var login = model.Login.Trim();
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Email == login || x.UserName == login);

        if (user is null || !passwords.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Pogrešan email/korisničko ime ili lozinka.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var userName = model.UserName.Trim();

        if (await db.Users.AnyAsync(x => x.Email == email))
            ModelState.AddModelError(nameof(model.Email), "Korisnik sa ovim emailom već postoji.");

        if (await db.Users.AnyAsync(x => x.UserName == userName))
            ModelState.AddModelError(nameof(model.UserName), "Korisničko ime je zauzeto.");

        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            DisplayName = userName,
            Theme = "Dark",
            PasswordHash = passwords.Hash(model.Password),
            Role = "User",
            Coins = 500
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var starterCards = await db.Cards
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .Take(55)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var cardId in starterCards)
        {
            db.UserCards.Add(new UserCard
            {
                UserId = user.Id,
                CardId = cardId,
                Quantity = 1
            });
        }

        await db.SaveChangesAsync();
        await SignInAsync(user, rememberMe: false);

        TempData["Success"] = "Profil je kreiran. Dobili ste 500 coina i početnu kolekciju.";
        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(ApplicationUser user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            });

        Response.Cookies.Append(
            "AstralNexus.Theme",
            user.Theme,
            new CookieOptions
            {
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
    }
}
