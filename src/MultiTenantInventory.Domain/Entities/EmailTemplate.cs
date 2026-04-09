using System.ComponentModel.DataAnnotations;

namespace MultiTenantInventory.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string HtmlBody { get; set; } = string.Empty;
}
