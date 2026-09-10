using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.Api;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/decks")]
public class DecksApiController(
    AppDbContext db,
    DeckRulesService rules) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();

        var decks = await db.Decks
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.IsValid,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                CardIds = x.Cards.OrderBy(c => c.SortOrder).Select(c => c.CardId).ToList()
            })
            .ToListAsync();

        return Ok(decks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var userId = User.GetUserId();

        var deck = await db.Decks
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.IsValid,
                Cards = x.Cards
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new
                    {
                        c.CardId,
                        c.Card.Name,
                        c.Card.Type,
                        c.Card.BaseValue,
                        c.Card.ImagePath
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return deck is null ? NotFound() : Ok(deck);
    }

    [HttpPost]
    public async Task<IActionResult> Create(DeckApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { errors = new[] { "Naziv špila je obavezan." } });

        var userId = User.GetUserId();
        var ids = request.CardIds.Distinct().ToList();

        var cards = await LoadOwnedCardsAsync(userId, ids);
        var validation = rules.Validate(cards);

        if (cards.Count != ids.Count)
            return BadRequest(new { errors = new[] { "Jedna ili više karata nisu u korisničkoj kolekciji." } });

        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        if (await db.Decks.AnyAsync(x => x.UserId == userId && x.Name == request.Name))
            return Conflict(new { error = "Špil sa tim nazivom već postoji." });

        var deck = new Deck
        {
            UserId = userId,
            Name = request.Name.Trim(),
            IsValid = true
        };

        for (var i = 0; i < cards.Count; i++)
            deck.Cards.Add(new DeckCard { CardId = cards[i].Id, SortOrder = i });

        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = deck.Id }, new { deck.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, DeckApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { errors = new[] { "Naziv špila je obavezan." } });

        var userId = User.GetUserId();
        var deck = await db.Decks
            .Where(x => x.Id == id && x.UserId == userId)
            .Include(x => x.Cards)
            .FirstOrDefaultAsync();

        if (deck is null)
            return NotFound();

        var ids = request.CardIds.Distinct().ToList();
        var cards = await LoadOwnedCardsAsync(userId, ids);
        var validation = rules.Validate(cards);

        if (cards.Count != ids.Count)
            return BadRequest(new { errors = new[] { "Jedna ili više karata nisu u korisničkoj kolekciji." } });

        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors });

        if (await db.Decks.AnyAsync(x => x.UserId == userId && x.Id != id && x.Name == request.Name))
            return Conflict(new { error = "Špil sa tim nazivom već postoji." });

        deck.Name = request.Name.Trim();
        deck.IsValid = true;
        deck.UpdatedAtUtc = DateTime.UtcNow;

        db.DeckCards.RemoveRange(deck.Cards);
        deck.Cards.Clear();

        for (var i = 0; i < cards.Count; i++)
            deck.Cards.Add(new DeckCard { CardId = cards[i].Id, SortOrder = i });

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var deck = await db.Decks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (deck is null)
            return NotFound();

        // Match historija ostaje sačuvana i nakon brisanja špila.
        var relatedMatches = await db.Matches
            .Where(x => x.DeckId == deck.Id)
            .ToListAsync();

        foreach (var match in relatedMatches)
            match.DeckId = null;

        db.Decks.Remove(deck);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<List<Card>> LoadOwnedCardsAsync(int userId, IReadOnlyCollection<int> ids)
    {
        var cards = await db.UserCards
            .Where(x => x.UserId == userId && x.Quantity > 0 && ids.Contains(x.CardId) && x.Card.IsActive)
            .Select(x => x.Card)
            .ToListAsync();

        var order = ids.Select((id, index) => new { id, index }).ToDictionary(x => x.id, x => x.index);
        return cards.OrderBy(x => order[x.Id]).ToList();
    }
}
