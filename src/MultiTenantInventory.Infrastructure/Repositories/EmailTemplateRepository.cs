namespace MultiTenantInventory.Infrastructure.Repositories;

public class EmailTemplateRepository(AppDbContext db) : Repository<EmailTemplate>(db), IEmailTemplateRepository
{

    public async Task<EmailTemplate?> GetByNameAsync(string name)
        => await QueryNoFilters().FirstOrDefaultAsync(t => t.Name == name);

    public async Task<bool> ExistsByNameAsync(string name)
        => await QueryNoFilters().AnyAsync(t => t.Name == name);
}
