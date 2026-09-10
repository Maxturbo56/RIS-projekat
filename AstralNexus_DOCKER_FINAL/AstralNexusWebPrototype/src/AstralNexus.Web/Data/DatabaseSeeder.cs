using System.Text.Json;
using AstralNexus.Web.Models.Entities;
using AstralNexus.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace AstralNexus.Web.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        PasswordService passwords,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken = default)
    {
        var seedPath = Path.Combine(environment.ContentRootPath, "Seed", "cards.json");
        if (!File.Exists(seedPath))
            throw new FileNotFoundException("Nedostaje Seed/cards.json.", seedPath);

        var json = await File.ReadAllTextAsync(seedPath, cancellationToken);
        var seedCards = JsonSerializer.Deserialize<List<CardSeedModel>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        var existingKeys = await db.Cards
            .Select(x => x.ExternalKey)
            .ToHashSetAsync(cancellationToken);

        foreach (var item in seedCards.Where(x => !existingKeys.Contains(x.ExternalKey)))
        {
            db.Cards.Add(new Card
            {
                ExternalKey = item.ExternalKey,
                Name = item.Name,
                Type = item.Type,
                BaseValue = item.BaseValue,
                Description = item.Description,
                ImagePath = item.ImagePath,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);


        var boardSeedPath = Path.Combine(environment.ContentRootPath, "Seed", "board-cards.json");
        if (File.Exists(boardSeedPath))
        {
            var boardJson = await File.ReadAllTextAsync(boardSeedPath, cancellationToken);
            var boardSeed = JsonSerializer.Deserialize<List<BoardCardSeedModel>>(
                boardJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            var existingBoardKeys = await db.BoardCards
                .Select(x => x.ExternalKey)
                .ToHashSetAsync(cancellationToken);

            foreach (var item in boardSeed.Where(x => !existingBoardKeys.Contains(x.ExternalKey)))
            {
                db.BoardCards.Add(new BoardCard
                {
                    ExternalKey = item.ExternalKey,
                    Name = item.Name,
                    Description = item.Description,
                    ImagePath = item.ImagePath,
                    IsActive = true
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        var admin = await db.Users.FirstOrDefaultAsync(x => x.Email == "admin@astralnexus.local", cancellationToken);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@astralnexus.local",
                DisplayName = "Astral Administrator",
                Theme = "Dark",
                PasswordHash = passwords.Hash("Admin123!"),
                Role = "Admin",
                Coins = 9999
            };
            db.Users.Add(admin);
        }

        var demo = await db.Users.FirstOrDefaultAsync(x => x.Email == "demo@astralnexus.local", cancellationToken);
        if (demo is null)
        {
            demo = new ApplicationUser
            {
                UserName = "demo",
                Email = "demo@astralnexus.local",
                DisplayName = "Demo Player",
                Theme = "Dark",
                PasswordHash = passwords.Hash("Demo123!"),
                Role = "User",
                Coins = 500
            };
            db.Users.Add(demo);
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!await db.UserCards.AnyAsync(x => x.UserId == demo.Id, cancellationToken))
        {
            var allCards = await db.Cards
                .Where(x => x.IsActive)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            var starterDeck = BuildStarterDeck(allCards);

            var collectionIds = starterDeck.Select(x => x.Id)
                .Concat(allCards.Take(45).Select(x => x.Id))
                .Distinct()
                .Take(55)
                .ToHashSet();

            foreach (var cardId in collectionIds)
            {
                db.UserCards.Add(new UserCard
                {
                    UserId = demo.Id,
                    CardId = cardId,
                    Quantity = 1
                });
            }

            await db.SaveChangesAsync(cancellationToken);

            if (starterDeck.Count == 20)
            {
                var deck = new Deck
                {
                    UserId = demo.Id,
                    Name = "Awakening Starter",
                    IsValid = true
                };

                var order = 0;
                foreach (var card in starterDeck)
                {
                    deck.Cards.Add(new DeckCard
                    {
                        CardId = card.Id,
                        SortOrder = order++
                    });
                }

                db.Decks.Add(deck);
                await db.SaveChangesAsync(cancellationToken);

                db.Matches.AddRange(
                    new MatchRecord
                    {
                        UserId = demo.Id,
                        DeckId = deck.Id,
                        Result = "Win",
                        TurnsPlayed = 8,
                        CoinsAwarded = 20,
                        PlayedAtUtc = DateTime.UtcNow.AddDays(-2)
                    },
                    new MatchRecord
                    {
                        UserId = demo.Id,
                        DeckId = deck.Id,
                        Result = "Loss",
                        TurnsPlayed = 11,
                        CoinsAwarded = 10,
                        PlayedAtUtc = DateTime.UtcNow.AddDays(-1)
                    });
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static List<Card> BuildStarterDeck(IEnumerable<Card> source)
    {
        var chosen = new List<Card>();

        foreach (var card in source.OrderBy(x => x.Name))
        {
            if (chosen.Count >= 20)
                break;

            if (chosen.Any(x => x.Id == card.Id))
                continue;

            if (card.BaseValue == 3 && chosen.Count(x => x.BaseValue == 3) >= 3)
                continue;

            if (card.Type.Contains("mage", StringComparison.OrdinalIgnoreCase)
                && chosen.Count(x => x.Type.Contains("mage", StringComparison.OrdinalIgnoreCase)) >= 8)
                continue;

            if (card.Type.Equals("sentinel", StringComparison.OrdinalIgnoreCase)
                && chosen.Count(x => x.Type.Equals("sentinel", StringComparison.OrdinalIgnoreCase)) >= 1)
                continue;

            if (card.Type.Contains("effect", StringComparison.OrdinalIgnoreCase)
                && chosen.Count(x => x.Type.Contains("effect", StringComparison.OrdinalIgnoreCase)) >= 5)
                continue;

            chosen.Add(card);
        }

        return chosen;
    }


    private sealed class BoardCardSeedModel
    {
        public string ExternalKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }

    private sealed class CardSeedModel
    {
        public string ExternalKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int BaseValue { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
