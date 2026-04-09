using System.ComponentModel.DataAnnotations;
using MultiTenantInventory.Domain.Enums;

namespace MultiTenantInventory.Domain.Entities;

public class AppUser : TenantEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Token for email-based password setup flow.
    /// </summary>
    public string? SetPasswordToken { get; set; }

    public DateTime? SetPasswordTokenExpiry { get; set; }

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
