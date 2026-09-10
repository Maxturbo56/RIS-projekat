using AstralNexus.Web.Data;
using AstralNexus.Web.Extensions;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Controllers;

[Authorize]
public class ShopController(AppDbContext db) : Controller
{
    private const int PackCost = 100;
    private const int CardsPerPack = 6;

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();

        var model = new ShopViewModel
        {
            Coins = await db.Users
                .Where(x => x.Id == userId)
                .Select(x => x.Coins)
                .SingleAsync(),
            PackCost = PackCost,
            RecentOpenings = await db.PackOpenings
                .Where(x => x.UserId == userId)
                .Include(x => x.Cards)
                    .ThenInclude(x => x.Card)
                .OrderByDescending(x => x.OpenedAtUtc)
                .Take(6)
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OpenPack()
    {
        var userId = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == userId);

        if (user.Coins < PackCost)
        {
            TempData["Error"] = $"Nemate dovoljno coina. Paket košta {PackCost}.";
            return RedirectToAction(nameof(Index));
        }

        var availableCards = await db.Cards
            .Where(x => x.IsActive)
            .ToListAsync();

        if (availableCards.Count == 0)
        {
            TempData["Error"] = "Nema aktivnih karata u sistemu.";
            return RedirectToAction(nameof(Index));
        }

        user.Coins -= PackCost;

        var opening = new PackOpening
        {
            UserId = userId,
            Cost = PackCost,
            OpenedAtUtc = DateTime.UtcNow
        };

        // Svi podaci o otvaranju paketa se pripreme u memoriji, pa se zapisuju
        // jednim SaveChangesAsync pozivom. EF Core taj poziv automatski izvršava
        // u transakciji, što je kompatibilno sa EnableRetryOnFailure strategijom.
        db.PackOpenings.Add(opening);

        var owned = await db.UserCards
            .Where(x => x.UserId == userId)
            .ToDictionaryAsync(x => x.CardId);

        var resultCards = new List<PackResultCard>();
        var duplicateCoins = 0;

        for (var position = 0; position < CardsPerPack; position++)
        {
            var card = availableCards[Random.Shared.Next(availableCards.Count)];
            var wasDuplicate = owned.TryGetValue(card.Id, out var userCard) && userCard.Quantity > 0;

            if (wasDuplicate)
            {
                duplicateCoins += 1;
                user.Coins += 1;
            }
            else
            {
                userCard = new UserCard
                {
                    UserId = userId,
                    CardId = card.Id,
                    Quantity = 1,
                    AcquiredAtUtc = DateTime.UtcNow
                };

                db.UserCards.Add(userCard);
                owned[card.Id] = userCard;
            }

            opening.Cards.Add(new PackOpeningCard
            {
                CardId = card.Id,
                Position = position,
                WasDuplicate = wasDuplicate
            });

            resultCards.Add(new PackResultCard(card, wasDuplicate));
        }

        opening.DuplicateCoins = duplicateCoins;

        await db.SaveChangesAsync();

        return View("OpenResult", new PackOpenResultViewModel
        {
            Cards = resultCards,
            Cost = PackCost,
            DuplicateCoins = duplicateCoins,
            RemainingCoins = user.Coins
        });
    }
}
