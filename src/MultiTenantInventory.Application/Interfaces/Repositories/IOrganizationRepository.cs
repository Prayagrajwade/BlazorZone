using MultiTenantInventory.Domain.Entities;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByIdWithStatsAsync(Guid id);
    Task<List<Organization>> GetAllWithStatsAsync();
}
