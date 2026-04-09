namespace MultiTenantInventory.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : Repository<AppUser>(db), IUserRepository
{

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await QueryNoFilters()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<AppUser?> GetByTokenAsync(string token)
    {
        return await QueryNoFilters()
            .FirstOrDefaultAsync(u =>
                u.SetPasswordToken == token &&
                u.SetPasswordTokenExpiry > DateTime.UtcNow &&
                !u.IsDeleted);
    }

    public async Task<List<AppUser>> GetByOrgAsync(Guid orgId, UserRole? roleFilter = null)
    {
        var query = QueryNoFilters()
            .Where(u => u.OrganizationId == orgId && !u.IsDeleted);

        if (roleFilter.HasValue)
            query = query.Where(u => u.Role == roleFilter.Value);

        return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<AppUser?> GetByIdWithOrgAsync(Guid id)
    {
        return await QueryNoFilters()
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await QueryNoFilters()
            .AnyAsync(u => u.Email == email && !u.IsDeleted);
    }
}
