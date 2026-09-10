using System.ComponentModel.DataAnnotations;

namespace AstralNexus.Web.Models.Entities;

public class BoardCard
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string ExternalKey { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(3000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(400)]
    public string ImagePath { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
