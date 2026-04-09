namespace MultiTenantInventory.Infrastructure.Repositories;

public class OrganizationRepository(AppDbContext db) : Repository<Organization>(db), IOrganizationRepository
{

    public async Task<List<Organization>> GetAllWithStatsAsync()
    {
        return await QueryNoFilters()
            .Where(o => !o.IsDeleted)
            .Include(o => o.Users)
            .Include(o => o.Products)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Organization?> GetByIdWithStatsAsync(Guid id)
    {
        return await QueryNoFilters()
            .Where(o => o.Id == id && !o.IsDeleted)
            .Include(o => o.Users)
            .Include(o => o.Products)
            .FirstOrDefaultAsync();
    }
}
