using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers.Api;

[ApiController]
[Authorize]
[Route("api/stats")]
public class StatsApiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();

        return Ok(new
        {
            Coins = await db.Users.Where(x => x.Id == userId).Select(x => x.Coins).SingleAsync(),
            OwnedCards = await db.UserCards.CountAsync(x => x.UserId == userId && x.Quantity > 0),
            Decks = await db.Decks.CountAsync(x => x.UserId == userId),
            Wins = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Win"),
            Losses = await db.Matches.CountAsync(x => x.UserId == userId && x.Result == "Loss"),
            PackOpenings = await db.PackOpenings.CountAsync(x => x.UserId == userId)
        });
    }
}
