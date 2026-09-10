using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.Entities;

public class Card
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string ExternalKey { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Type { get; set; } = string.Empty;

    public int BaseValue { get; set; }

    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(400)]
    public string ImagePath { get; set; } = "/images/branding/card-back.png";

    public bool IsActive { get; set; } = true;

    public ICollection<UserCard> Owners { get; set; } = new List<UserCard>();
    public ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();
    public ICollection<PackOpeningCard> PackOpeningCards { get; set; } = new List<PackOpeningCard>();
}
