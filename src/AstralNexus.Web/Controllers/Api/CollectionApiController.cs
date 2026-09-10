using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/collection")]
public class CollectionApiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();

        var cards = await db.UserCards
            .Where(x => x.UserId == userId && x.Quantity > 0 && x.Card.IsActive)
            .OrderBy(x => x.Card.Name)
            .Select(x => new
            {
                x.CardId,
                x.Card.Name,
                x.Card.Type,
                x.Card.BaseValue,
                x.Card.ImagePath,
                x.Quantity,
                x.AcquiredAtUtc
            })
            .ToListAsync();

        return Ok(cards);
    }
}
