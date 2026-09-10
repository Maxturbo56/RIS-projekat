namespace AstralNexus.Web.Models.Entities;

public class UserCard
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int CardId { get; set; }
    public Card Card { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public DateTime AcquiredAtUtc { get; set; } = DateTime.UtcNow;
}
