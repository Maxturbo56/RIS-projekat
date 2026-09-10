using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class ShopViewModel
{
    public int Coins { get; set; }
    public int PackCost { get; set; } = 100;
    public IReadOnlyList<PackOpening> RecentOpenings { get; set; } = [];
}
