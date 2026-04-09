using MultiTenantInventory.Domain.Entities;
using MultiTenantInventory.Domain.Enums;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByTokenAsync(string token);
    Task<List<AppUser>> GetByOrgAsync(Guid orgId, UserRole? roleFilter = null);
    Task<AppUser?> GetByIdWithOrgAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}
