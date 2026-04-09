using MultiTenantInventory.Domain.Entities;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> GetAllWithCategoryAsync();
    Task<Product?> GetByIdWithCategoryAsync(Guid id);
}
