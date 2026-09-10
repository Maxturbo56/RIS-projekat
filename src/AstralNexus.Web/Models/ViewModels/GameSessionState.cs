namespace AstralNexus.Web.Models.ViewModels;

public class GameSessionState
{
    public int DeckId { get; set; }
    public string DeckName { get; set; } = string.Empty;
    public int? BoardCardId { get; set; }
    public List<int> DrawPile { get; set; } = [];
    public List<int> Hand { get; set; } = [];
    public List<int> Discard { get; set; } = [];
    public int PlayerHp { get; set; } = 10;
    public int OpponentHp { get; set; } = 10;
    public int Turn { get; set; } = 1;
    public int TurnsPlayed { get; set; }
    public int? LastDice { get; set; }
    public string? LastCoin { get; set; }
    public string Message { get; set; } = string.Empty;
}
