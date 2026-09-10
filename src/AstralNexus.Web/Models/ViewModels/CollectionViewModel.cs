using AstralNexus.Web.Models.Entities;

namespace AstralNexus.Web.Models.ViewModels;

public class CollectionViewModel
{
    public IReadOnlyList<UserCard> Cards { get; set; } = [];
    public IReadOnlyList<string> Types { get; set; } = [];
    public IReadOnlyList<int> Values { get; set; } = [];
    public string Search { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? Value { get; set; }
    public int Coins { get; set; }
}
