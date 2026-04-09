using MultiTenantInventory.Domain.Entities;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    Task<EmailTemplate?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
