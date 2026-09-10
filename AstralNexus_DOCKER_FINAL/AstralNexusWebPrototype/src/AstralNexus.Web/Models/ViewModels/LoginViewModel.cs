using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Unesite email ili korisničko ime.")]
    [Display(Name = "Email ili korisničko ime")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unesite lozinku.")]
    [DataType(DataType.Password)]
    [Display(Name = "Lozinka")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Zapamti me")]
    public bool RememberMe { get; set; }
}
