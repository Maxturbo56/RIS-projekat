using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Services;

public class DeckRulesService
{
    public DeckValidationResult Validate(IReadOnlyCollection<Card> cards)
    {
        var errors = new List<string>();

        if (cards.Count != 20)
            errors.Add("Špil mora sadržavati tačno 20 karata.");

        if (cards.Select(x => x.Id).Distinct().Count() != cards.Count)
            errors.Add("Ista karta ne može biti dodana više puta u isti špil.");

        var valueThreeCount = cards.Count(x => x.BaseValue == 3);
        if (valueThreeCount > 3)
            errors.Add("Dozvoljene su najviše 3 karte sa vrijednošću 3.");

        var mageCount = cards.Count(x => x.Type.Contains("mage", StringComparison.OrdinalIgnoreCase));
        if (mageCount > 8)
            errors.Add("Dozvoljeno je najviše 8 Mage karata.");

        var sentinelCount = cards.Count(x => x.Type.Equals("sentinel", StringComparison.OrdinalIgnoreCase));
        if (sentinelCount > 1)
            errors.Add("Dozvoljena je najviše 1 Sentinel karta.");

        var effectCount = cards.Count(x => x.Type.Contains("effect", StringComparison.OrdinalIgnoreCase));
        if (effectCount > 5)
            errors.Add("Dozvoljeno je najviše 5 Effect karata.");

        return new DeckValidationResult(errors.Count == 0, errors);
    }

    public bool CanAdd(Card candidate, IReadOnlyCollection<Card> existing, out string reason)
    {
        var combined = existing.Concat([candidate]).ToList();
        var result = ValidatePartial(combined);
        reason = result.Errors.FirstOrDefault() ?? string.Empty;
        return result.IsValid;
    }

    private DeckValidationResult ValidatePartial(IReadOnlyCollection<Card> cards)
    {
        var errors = new List<string>();

        if (cards.Count > 20)
            errors.Add("Špil ne može imati više od 20 karata.");

        if (cards.Select(x => x.Id).Distinct().Count() != cards.Count)
            errors.Add("Ova karta je već u špilu.");

        if (cards.Count(x => x.BaseValue == 3) > 3)
            errors.Add("Najviše 3 karte sa vrijednošću 3.");

        if (cards.Count(x => x.Type.Contains("mage", StringComparison.OrdinalIgnoreCase)) > 8)
            errors.Add("Najviše 8 Mage karata.");

        if (cards.Count(x => x.Type.Equals("sentinel", StringComparison.OrdinalIgnoreCase)) > 1)
            errors.Add("Najviše 1 Sentinel karta.");

        if (cards.Count(x => x.Type.Contains("effect", StringComparison.OrdinalIgnoreCase)) > 5)
            errors.Add("Najviše 5 Effect karata.");

        return new DeckValidationResult(errors.Count == 0, errors);
    }
}

public record DeckValidationResult(bool IsValid, IReadOnlyList<string> Errors);
