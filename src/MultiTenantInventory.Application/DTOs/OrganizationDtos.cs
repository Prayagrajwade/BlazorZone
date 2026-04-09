using System.ComponentModel.DataAnnotations;

namespace MultiTenantInventory.Application.DTOs;

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public int ProductCount { get; set; }
}

public class CreateOrganizationDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateOrganizationDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
