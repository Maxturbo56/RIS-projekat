using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class GamePageViewModel
{
    public GameSessionState? State { get; set; }
    public IReadOnlyList<Deck> AvailableDecks { get; set; } = [];
    public IReadOnlyList<Card> HandCards { get; set; } = [];
    public IReadOnlyList<Card> DiscardCards { get; set; } = [];
    public BoardCard? BoardCard { get; set; }
}
