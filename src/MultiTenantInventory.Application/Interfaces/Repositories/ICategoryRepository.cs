using MultiTenantInventory.Domain.Entities;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetAllWithProductCountAsync();
}
