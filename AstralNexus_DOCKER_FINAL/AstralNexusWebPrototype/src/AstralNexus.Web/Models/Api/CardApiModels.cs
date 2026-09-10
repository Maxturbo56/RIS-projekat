namespace AstralNexus.Web.Models.Api;

public record CardApiRequest(
    string Name,
    string Type,
    int BaseValue,
    string Description,
    string? ImagePath,
    bool IsActive = true);

public record DeckApiRequest(string Name, IReadOnlyList<int> CardIds);
