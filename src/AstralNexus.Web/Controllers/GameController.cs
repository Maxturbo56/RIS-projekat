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
public class GameController(
    AppDbContext db,
    DeckRulesService deckRules) : Controller
{
    private const string SessionKey = "AstralNexus.ActiveGame";

    public async Task<IActionResult> Index()
    {
        var state = HttpContext.Session.GetJson<GameSessionState>(SessionKey);
        return View(await BuildPageModelAsync(state));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int deckId)
    {
        var userId = User.GetUserId();

        var deck = await db.Decks
            .Where(x => x.Id == deckId && x.UserId == userId)
            .Include(x => x.Cards)
                .ThenInclude(x => x.Card)
            .FirstOrDefaultAsync();

        if (deck is null)
            return NotFound();

        var orderedCards = deck.Cards
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Card)
            .Where(x => x.IsActive)
            .ToList();

        var validation = deckRules.Validate(orderedCards);
        if (!validation.IsValid)
        {
            TempData["Error"] = "Ovaj špil više ne zadovoljava pravila: " +
                                string.Join(" ", validation.Errors);
            return RedirectToAction(nameof(Index));
        }

        var pile = orderedCards.Select(x => x.Id).ToList();
        Shuffle(pile);

        var boardIds = await db.BoardCards
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        var boardCardId = boardIds.Count == 0
            ? (int?)null
            : boardIds[Random.Shared.Next(boardIds.Count)];

        var state = new GameSessionState
        {
            DeckId = deck.Id,
            DeckName = deck.Name,
            BoardCardId = boardCardId,
            DrawPile = pile,
            PlayerHp = 10,
            OpponentHp = 10,
            Turn = 1,
            TurnsPlayed = 0,
            Message = "Meč je pokrenut. Izvučeno je početnih 6 karata."
        };

        for (var i = 0; i < 6 && state.DrawPile.Count > 0; i++)
            DrawOne(state);

        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Draw()
    {
        var state = GetRequiredState();

        if (state.Hand.Count >= 6)
            state.Message = "Ruka je puna. Maksimalno 6 karata.";
        else if (state.DrawPile.Count == 0)
            state.Message = "U špilu više nema karata.";
        else
        {
            DrawOne(state);
            state.Message = "Izvučena je jedna karta.";
        }

        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Discard(int cardId)
    {
        var state = GetRequiredState();

        if (state.Hand.Remove(cardId))
        {
            state.Discard.Add(cardId);
            state.Message = "Karta je prebačena u discard pile.";
        }

        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangeHealth(string target, int delta)
    {
        var state = GetRequiredState();
        delta = Math.Clamp(delta, -1, 1);

        if (target.Equals("player", StringComparison.OrdinalIgnoreCase))
            state.PlayerHp += delta;
        else if (target.Equals("opponent", StringComparison.OrdinalIgnoreCase))
            state.OpponentHp += delta;

        state.Message = state.PlayerHp <= 0 || state.OpponentHp <= 0
            ? "Jedan igrač je na 0 HP. Završite meč kao pobjedu ili poraz."
            : "HP je ažuriran.";

        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EndTurn()
    {
        var state = GetRequiredState();
        state.Turn = state.Turn == 1 ? 2 : 1;
        state.TurnsPlayed++;
        state.Message = $"Potez predan. Na redu je igrač {state.Turn}.";
        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RollDice()
    {
        var state = GetRequiredState();
        state.LastDice = Random.Shared.Next(1, 7);
        state.Message = $"Kocka: {state.LastDice}.";
        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FlipCoin()
    {
        var state = GetRequiredState();
        state.LastCoin = Random.Shared.Next(2) == 0 ? "Glava" : "Pismo";
        state.Message = $"Novčić: {state.LastCoin}.";
        SaveState(state);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(string result)
    {
        var state = GetRequiredState();
        var normalized = result.Equals("Win", StringComparison.OrdinalIgnoreCase)
            ? "Win"
            : result.Equals("Loss", StringComparison.OrdinalIgnoreCase)
                ? "Loss"
                : string.Empty;

        if (string.IsNullOrEmpty(normalized))
            return BadRequest("Rezultat mora biti Win ili Loss.");

        var turns = Math.Max(1, state.TurnsPlayed);
        var reward = normalized == "Win"
            ? 20
            : 5 + (turns / 2);

        var userId = User.GetUserId();
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.Coins += reward;

        db.Matches.Add(new MatchRecord
        {
            UserId = userId,
            DeckId = state.DeckId,
            Result = normalized,
            TurnsPlayed = turns,
            CoinsAwarded = reward,
            PlayedAtUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        HttpContext.Session.Remove(SessionKey);

        TempData["Success"] = normalized == "Win"
            ? $"Pobjeda! Osvojili ste {reward} coina."
            : $"Meč je završen. Osvojili ste {reward} coina.";

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        HttpContext.Session.Remove(SessionKey);
        TempData["Success"] = "Aktivni meč je resetovan bez upisa rezultata.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<GamePageViewModel> BuildPageModelAsync(GameSessionState? state)
    {
        var userId = User.GetUserId();

        if (state is null)
        {
            return new GamePageViewModel
            {
                AvailableDecks = await db.Decks
                    .Where(x => x.UserId == userId && x.IsValid)
                    .Include(x => x.Cards)
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            };
        }

        var ids = state.Hand.Concat(state.Discard).Distinct().ToList();
        var cards = await db.Cards
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id);

        BoardCard? boardCard = null;
        if (state.BoardCardId.HasValue)
            boardCard = await db.BoardCards.FindAsync(state.BoardCardId.Value);

        return new GamePageViewModel
        {
            State = state,
            HandCards = state.Hand
                .Where(cards.ContainsKey)
                .Select(id => cards[id])
                .ToList(),
            DiscardCards = state.Discard
                .Where(cards.ContainsKey)
                .Select(id => cards[id])
                .ToList(),
            BoardCard = boardCard
        };
    }

    private GameSessionState GetRequiredState()
        => HttpContext.Session.GetJson<GameSessionState>(SessionKey)
           ?? throw new InvalidOperationException("Nema aktivnog meča.");

    private void SaveState(GameSessionState state)
        => HttpContext.Session.SetJson(SessionKey, state);

    private static void DrawOne(GameSessionState state)
    {
        var lastIndex = state.DrawPile.Count - 1;
        var cardId = state.DrawPile[lastIndex];
        state.DrawPile.RemoveAt(lastIndex);
        state.Hand.Add(cardId);
    }

    private static void Shuffle(List<int> values)
    {
        for (var i = values.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }
}
