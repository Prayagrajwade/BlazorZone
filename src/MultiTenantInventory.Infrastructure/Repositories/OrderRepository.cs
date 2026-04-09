
namespace MultiTenantInventory.Infrastructure.Repositories;

public class OrderRepository(AppDbContext db) : Repository<Order>(db), IOrderRepository
{

    public async Task<List<Order>> GetAllWithDetailsAsync()
    {
        return await Query()
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByUserAsync(Guid userId)
    {
        return await Query()
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await Query()
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetRecentAsync(int count)
    {
        return await Query()
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
