using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public int Coins { get; set; }
    public int OwnedCards { get; set; }
    public int TotalCardCopies { get; set; }
    public int Decks { get; set; }
    public int ValidDecks { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public IReadOnlyList<MatchRecord> RecentMatches { get; set; } = [];
    public IReadOnlyList<Card> FeaturedCards { get; set; } = [];
}
