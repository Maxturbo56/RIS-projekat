using AstralNexus.Web.Data;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Models.ViewModels;
using AstralNexus.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCardsController(
    AppDbContext db,
    CardImageService images) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Cards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Type.Contains(term));
        }

        ViewBag.Search = search ?? string.Empty;
        return View(await query
            .OrderByDescending(x => x.IsActive)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync());
    }

    [HttpGet]
    public IActionResult Create() => View("Editor", new CardEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CardEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View("Editor", model);

        try
        {
            var imagePath = await images.SaveAsync(model.ImageFile, cancellationToken);

            var card = new Card
            {
                ExternalKey = $"admin-{Guid.NewGuid():N}",
                Name = model.Name.Trim(),
                Type = model.Type.Trim(),
                BaseValue = model.BaseValue,
                Description = model.Description?.Trim() ?? string.Empty,
                ImagePath = imagePath ?? "/images/branding/card-back.png",
                IsActive = model.IsActive
            };

            db.Cards.Add(card);
            await db.SaveChangesAsync(cancellationToken);

            TempData["Success"] = $"Karta '{card.Name}' je kreirana.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            return View("Editor", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var card = await db.Cards.FindAsync(id);
        if (card is null)
            return NotFound();

        return View("Editor", new CardEditViewModel
        {
            Id = card.Id,
            Name = card.Name,
            Type = card.Type,
            BaseValue = card.BaseValue,
            Description = card.Description,
            IsActive = card.IsActive,
            ExistingImagePath = card.ImagePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CardEditViewModel model, CancellationToken cancellationToken)
    {
        var card = await db.Cards.FindAsync(new object[] { id }, cancellationToken);
        if (card is null)
            return NotFound();

        model.Id = id;
        model.ExistingImagePath = card.ImagePath;

        if (!ModelState.IsValid)
            return View("Editor", model);

        try
        {
            var newImage = await images.SaveAsync(model.ImageFile, cancellationToken);
            if (newImage is not null)
            {
                images.DeleteUploadedFile(card.ImagePath);
                card.ImagePath = newImage;
            }

            card.Name = model.Name.Trim();
            card.Type = model.Type.Trim();
            card.BaseValue = model.BaseValue;
            card.Description = model.Description?.Trim() ?? string.Empty;
            card.IsActive = model.IsActive;

            await db.SaveChangesAsync(cancellationToken);
            TempData["Success"] = $"Karta '{card.Name}' je ažurirana.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.ImageFile), ex.Message);
            return View("Editor", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await db.Cards.FindAsync(id);
        if (card is null)
            return NotFound();

        var inUse =
            await db.UserCards.AnyAsync(x => x.CardId == id) ||
            await db.DeckCards.AnyAsync(x => x.CardId == id) ||
            await db.PackOpeningCards.AnyAsync(x => x.CardId == id);

        if (inUse)
        {
            card.IsActive = false;
            await db.SaveChangesAsync();
            TempData["Error"] =
                "Karta je već korištena u historijskim podacima, pa je deaktivirana umjesto fizičkog brisanja.";
        }
        else
        {
            db.Cards.Remove(card);
            await db.SaveChangesAsync();
            images.DeleteUploadedFile(card.ImagePath);
            TempData["Success"] = "Karta je obrisana.";
        }

        return RedirectToAction(nameof(Index));
    }
}
