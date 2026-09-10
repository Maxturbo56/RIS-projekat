using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.Entities;

public class ApplicationUser
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(120), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Bio { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "User";

    public int Coins { get; set; } = 500;

    [Required, MaxLength(20)]
    public string Theme { get; set; } = "Dark";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<UserCard> Collection { get; set; } = new List<UserCard>();
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();
    public ICollection<MatchRecord> Matches { get; set; } = new List<MatchRecord>();
    public ICollection<PackOpening> PackOpenings { get; set; } = new List<PackOpening>();
}
