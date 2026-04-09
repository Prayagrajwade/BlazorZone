using MultiTenantInventory.Domain.Entities;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetAllWithDetailsAsync();
    Task<List<Order>> GetByUserAsync(Guid userId);
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Order>> GetRecentAsync(int count);
}
