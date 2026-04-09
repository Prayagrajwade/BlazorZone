namespace MultiTenantInventory.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext db) : Repository<Category>(db), ICategoryRepository
{

    public async Task<List<Category>> GetAllWithProductCountAsync()
    {
        return await Query()
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
