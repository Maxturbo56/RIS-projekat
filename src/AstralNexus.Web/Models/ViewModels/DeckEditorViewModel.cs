using System.ComponentModel.DataAnnotations;
using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class DeckEditorViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Unesite naziv špila.")]
    [StringLength(80)]
    [Display(Name = "Naziv špila")]
    public string Name { get; set; } = string.Empty;

    public List<int> SelectedCardIds { get; set; } = [];
    public IReadOnlyList<UserCard> OwnedCards { get; set; } = [];
    public IReadOnlyList<string> RuleErrors { get; set; } = [];
}
