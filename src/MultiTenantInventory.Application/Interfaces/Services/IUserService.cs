using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<List<UserDto>> GetByOrgAsync(Guid orgId);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<bool> ToggleActiveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ResendInviteAsync(Guid id);
}
