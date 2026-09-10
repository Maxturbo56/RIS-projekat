using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int Users { get; set; }
    public int Cards { get; set; }
    public int Decks { get; set; }
    public int Matches { get; set; }
    public int PackOpenings { get; set; }
    public IReadOnlyList<ApplicationUser> RecentUsers { get; set; } = [];
    public IReadOnlyList<MatchRecord> RecentMatches { get; set; } = [];
}
