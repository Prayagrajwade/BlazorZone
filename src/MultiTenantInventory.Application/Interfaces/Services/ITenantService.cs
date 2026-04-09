namespace MultiTenantInventory.Application.Interfaces.Services;

public interface ITenantService
{
    Guid OrganizationId { get; }
    Guid UserId { get; }
    string Role { get; }
    bool IsSuperAdmin { get; }
}
