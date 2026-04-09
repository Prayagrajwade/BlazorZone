using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IOrganizationService
{
    Task<List<OrganizationDto>> GetAllAsync();
    Task<OrganizationDto?> GetByIdAsync(Guid id);
    Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto);
    Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationDto dto);
    Task<bool> DeleteAsync(Guid id);
}
