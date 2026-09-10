using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.Entities;

public class MatchRecord
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int? DeckId { get; set; }
    public Deck? Deck { get; set; }

    [Required, MaxLength(20)]
    public string Result { get; set; } = "Loss";

    public int TurnsPlayed { get; set; }
    public int CoinsAwarded { get; set; }
    public DateTime PlayedAtUtc { get; set; } = DateTime.UtcNow;
}
