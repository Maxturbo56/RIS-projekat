using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AstralNexus.Web.Models.ViewModels;

public class CardEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Naziv")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    [Display(Name = "Tip")]
    public string Type { get; set; } = string.Empty;

    [Display(Name = "Bazna vrijednost")]
    public int BaseValue { get; set; }

    [StringLength(3000)]
    [Display(Name = "Opis")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Aktivna")]
    public bool IsActive { get; set; } = true;

    public string ExistingImagePath { get; set; } = string.Empty;

    [Display(Name = "Slika karte")]
    public IFormFile? ImageFile { get; set; }
}
