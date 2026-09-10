using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class PackOpenResultViewModel
{
    public IReadOnlyList<PackResultCard> Cards { get; set; } = [];
    public int Cost { get; set; }
    public int DuplicateCoins { get; set; }
    public int RemainingCoins { get; set; }
}

public record PackResultCard(Card Card, bool WasDuplicate);
