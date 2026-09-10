namespace AstralNexus.Web.Models.Entities;

public class PackOpening
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int Cost { get; set; }
    public int DuplicateCoins { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PackOpeningCard> Cards { get; set; } = new List<PackOpeningCard>();
}
