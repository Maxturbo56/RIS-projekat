namespace AstralNexus.Web.Models.Entities;

public class PackOpeningCard
{
    public int PackOpeningId { get; set; }
    public PackOpening PackOpening { get; set; } = null!;

    public int CardId { get; set; }
    public Card Card { get; set; } = null!;

    public int Position { get; set; }
    public bool WasDuplicate { get; set; }
}
