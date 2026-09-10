using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.Entities;

public class Deck
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public bool IsValid { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DeckCard> Cards { get; set; } = new List<DeckCard>();
    public ICollection<MatchRecord> Matches { get; set; } = new List<MatchRecord>();
}
