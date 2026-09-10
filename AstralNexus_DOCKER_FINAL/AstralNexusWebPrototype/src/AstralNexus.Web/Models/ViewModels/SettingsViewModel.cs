using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.ViewModels;

public class SettingsViewModel
{
    [Required]
    [Display(Name = "Tema")]
    public string Theme { get; set; } = "Dark";
}
