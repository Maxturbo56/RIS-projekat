using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(50, MinimumLength = 3)]
    [Display(Name = "Korisničko ime")]
    public string UserName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Lozinka")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Lozinke se ne podudaraju.")]
    [Display(Name = "Potvrda lozinke")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
