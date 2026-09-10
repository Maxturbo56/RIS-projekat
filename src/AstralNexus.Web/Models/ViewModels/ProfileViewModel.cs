using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.ViewModels;

public class ProfileViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Ime za prikaz")]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "O meni")]
    public string Bio { get; set; } = string.Empty;

    public int Coins { get; set; }
    public int OwnedCards { get; set; }
    public int Decks { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
