using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Models.ViewModels;
using AstralNexus.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize]
public class DecksController(
    AppDbContext db,
    DeckRulesService deckRules) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var decks = await db.Decks
            .Where(x => x.UserId == userId)
            .Include(x => x.Cards)
                .ThenInclude(x => x.Card)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync();

        return View(decks);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.GetUserId();
        var deck = await db.Decks
            .Where(x => x.Id == id && x.UserId == userId)
            .Include(x => x.Cards)
                .ThenInclude(x => x.Card)
            .FirstOrDefaultAsync();

        return deck is null ? NotFound() : View(deck);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new DeckEditorViewModel();
        await PopulateOwnedCardsAsync(model);
        return View("Editor", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeckEditorViewModel model)
    {
        var userId = User.GetUserId();
        model.SelectedCardIds = model.SelectedCardIds.Distinct().ToList();

        var selectedCards = await LoadOwnedSelectedCardsAsync(userId, model.SelectedCardIds);

        if (selectedCards.Count != model.SelectedCardIds.Count)
            ModelState.AddModelError(string.Empty, "Špil sadrži kartu koju korisnik ne posjeduje.");

        var validation = deckRules.Validate(selectedCards);
        foreach (var error in validation.Errors)
            ModelState.AddModelError(string.Empty, error);

        var normalizedName = model.Name?.Trim() ?? string.Empty;
        if (await db.Decks.AnyAsync(x => x.UserId == userId && x.Name == normalizedName))
            ModelState.AddModelError(nameof(model.Name), "Već imate špil sa ovim nazivom.");

        if (!ModelState.IsValid)
        {
            await PopulateOwnedCardsAsync(model);
            return View("Editor", model);
        }

        var deck = new Deck
        {
            UserId = userId,
            Name = normalizedName,
            IsValid = validation.IsValid
        };

        for (var i = 0; i < selectedCards.Count; i++)
        {
            deck.Cards.Add(new DeckCard
            {
                CardId = selectedCards[i].Id,
                SortOrder = i
            });
        }

        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Špil '{deck.Name}' je spremljen.";
        return RedirectToAction(nameof(Details), new { id = deck.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.GetUserId();
        var deck = await db.Decks
            .Where(x => x.Id == id && x.UserId == userId)
            .Include(x => x.Cards)
            .FirstOrDefaultAsync();

        if (deck is null)
            return NotFound();

        var model = new DeckEditorViewModel
        {
            Id = deck.Id,
            Name = deck.Name,
            SelectedCardIds = deck.Cards
                .OrderBy(x => x.SortOrder)
                .Select(x => x.CardId)
                .ToList()
        };

        await PopulateOwnedCardsAsync(model);
        return View("Editor", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DeckEditorViewModel model)
    {
        var userId = User.GetUserId();
        var deck = await db.Decks
            .Where(x => x.Id == id && x.UserId == userId)
            .Include(x => x.Cards)
            .FirstOrDefaultAsync();

        if (deck is null)
            return NotFound();

        model.Id = id;
        model.SelectedCardIds = model.SelectedCardIds.Distinct().ToList();
        var selectedCards = await LoadOwnedSelectedCardsAsync(userId, model.SelectedCardIds);

        if (selectedCards.Count != model.SelectedCardIds.Count)
            ModelState.AddModelError(string.Empty, "Špil sadrži kartu koju korisnik ne posjeduje.");

        var validation = deckRules.Validate(selectedCards);
        foreach (var error in validation.Errors)
            ModelState.AddModelError(string.Empty, error);

        var normalizedName = model.Name?.Trim() ?? string.Empty;
        if (await db.Decks.AnyAsync(x =>
                x.UserId == userId &&
                x.Id != id &&
                x.Name == normalizedName))
        {
            ModelState.AddModelError(nameof(model.Name), "Već imate špil sa ovim nazivom.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateOwnedCardsAsync(model);
            return View("Editor", model);
        }

        deck.Name = normalizedName;
        deck.IsValid = validation.IsValid;
        deck.UpdatedAtUtc = DateTime.UtcNow;

        db.DeckCards.RemoveRange(deck.Cards);
        deck.Cards.Clear();

        for (var i = 0; i < selectedCards.Count; i++)
        {
            deck.Cards.Add(new DeckCard
            {
                CardId = selectedCards[i].Id,
                SortOrder = i
            });
        }

        await db.SaveChangesAsync();

        TempData["Success"] = $"Špil '{deck.Name}' je ažuriran.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var deck = await db.Decks
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (deck is null)
            return NotFound();

        // SQL Server ne dozvoljava dvije kaskadne putanje User -> Matches
        // i User -> Decks -> Matches. Zbog toga je FK Deck -> Matches NO ACTION,
        // a historiji mečeva ručno uklanjamo referencu na špil prije brisanja.
        var relatedMatches = await db.Matches
            .Where(x => x.DeckId == deck.Id)
            .ToListAsync();

        foreach (var match in relatedMatches)
            match.DeckId = null;

        db.Decks.Remove(deck);
        await db.SaveChangesAsync();

        TempData["Success"] = "Špil je obrisan.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOwnedCardsAsync(DeckEditorViewModel model)
    {
        var userId = User.GetUserId();

        model.OwnedCards = await db.UserCards
            .Where(x => x.UserId == userId && x.Quantity > 0 && x.Card.IsActive)
            .Include(x => x.Card)
            .OrderByDescending(x => x.Card.BaseValue == 2)
            .ThenByDescending(x => x.Card.BaseValue == 3)
            .ThenBy(x => x.Card.Type)
            .ThenBy(x => x.Card.Name)
            .ToListAsync();
    }

    private async Task<List<Card>> LoadOwnedSelectedCardsAsync(int userId, IReadOnlyCollection<int> selectedIds)
    {
        if (selectedIds.Count == 0)
            return [];

        var cards = await db.UserCards
            .Where(x =>
                x.UserId == userId &&
                x.Quantity > 0 &&
                selectedIds.Contains(x.CardId) &&
                x.Card.IsActive)
            .Select(x => x.Card)
            .ToListAsync();

        var order = selectedIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        return cards.OrderBy(x => order[x.Id]).ToList();
    }
}
