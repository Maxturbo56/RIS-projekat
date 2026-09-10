using AstralNexus.Web.Data;
using AstralNexus.Web.Models.Api;
using AstralNexus.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers.Api;

[ApiController]
[Route("api/cards")]
public class CardsApiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var query = db.Cards.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Type.Contains(search));

        var cards = await query
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Type,
                x.BaseValue,
                x.Description,
                x.ImagePath
            })
            .ToListAsync();

        return Ok(cards);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var card = await db.Cards
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ExternalKey,
                x.Name,
                x.Type,
                x.BaseValue,
                x.Description,
                x.ImagePath,
                x.IsActive
            })
            .FirstOrDefaultAsync();

        return card is null ? NotFound() : Ok(card);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CardApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Type))
            return BadRequest(new { error = "Name i Type su obavezni." });

        var card = new Card
        {
            ExternalKey = $"api-{Guid.NewGuid():N}",
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            BaseValue = request.BaseValue,
            Description = request.Description?.Trim() ?? string.Empty,
            ImagePath = string.IsNullOrWhiteSpace(request.ImagePath)
                ? "/images/branding/card-back.png"
                : request.ImagePath,
            IsActive = request.IsActive
        };

        db.Cards.Add(card);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = card.Id }, new { card.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CardApiRequest request)
    {
        var card = await db.Cards.FindAsync(id);
        if (card is null)
            return NotFound();

        card.Name = request.Name.Trim();
        card.Type = request.Type.Trim();
        card.BaseValue = request.BaseValue;
        card.Description = request.Description?.Trim() ?? string.Empty;
        card.ImagePath = string.IsNullOrWhiteSpace(request.ImagePath)
            ? card.ImagePath
            : request.ImagePath;
        card.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
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
            return Conflict(new { error = "Karta je korištena u sistemu; deaktivirajte je umjesto brisanja." });

        db.Cards.Remove(card);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
