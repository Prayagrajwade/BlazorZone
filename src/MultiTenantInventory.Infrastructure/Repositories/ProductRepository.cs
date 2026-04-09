namespace MultiTenantInventory.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : Repository<Product>(db), IProductRepository
{

    public async Task<List<Product>> GetAllWithCategoryAsync()
    {
        return await Query()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithCategoryAsync(Guid id)
    {
        return await Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
