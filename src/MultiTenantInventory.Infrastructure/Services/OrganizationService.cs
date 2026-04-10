using Microsoft.Extensions.Logging;

namespace MultiTenantInventory.Infrastructure.Services;

public class OrganizationService(IOrganizationRepository repo, ITenantService tenant, ILogger<OrganizationService> logger) : IOrganizationService
{
    public async Task<List<OrganizationDto>> GetAllAsync()
    {
        var orgs = await repo.GetAllWithStatsAsync();
        return orgs.Select(o => new OrganizationDto
        {
            Id = o.Id,
            Name = o.Name,
            IsActive = o.IsActive,
            CreatedAt = o.CreatedAt,
            UserCount = o.Users.Count(u => !u.IsDeleted && u.Role == Domain.Enums.UserRole.User),
            ProductCount = o.Products.Count(p => !p.IsDeleted)
        }).ToList();
    }

    public async Task<OrganizationDto?> GetByIdAsync(Guid id)
    {
        var o = await repo.GetByIdWithStatsAsync(id);
        if (o == null) return null;

        return new OrganizationDto
        {
            Id = o.Id,
            Name = o.Name,
            IsActive = o.IsActive,
            CreatedAt = o.CreatedAt,
            UserCount = o.Users.Count(u => !u.IsDeleted && u.Role == Domain.Enums.UserRole.User),
            ProductCount = o.Products.Count(p => !p.IsDeleted)
        };
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto)
    {
        var org = new Organization
        {
            Name = dto.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(org);
        await repo.SaveChangesAsync();

        logger.LogInformation("Organization '{Name}' created by SA {UserId}", dto.Name, tenant.UserId);

        return new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt
        };
    }

    public async Task<OrganizationDto?> UpdateAsync(Guid id, UpdateOrganizationDto dto)
    {
        var orgs = await repo.GetAllWithStatsAsync();
        var org = orgs.FirstOrDefault(o => o.Id == id);
        if (org == null) return null;

        org.Name = dto.Name;
        org.IsActive = dto.IsActive;
        org.UpdatedAt = DateTime.UtcNow;

        repo.Update(org);
        await repo.SaveChangesAsync();

        logger.LogInformation("Organization '{Name}' updated by SA {UserId}", dto.Name, tenant.UserId);

        return new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var org = await repo.GetByIdWithStatsAsync(id);
        if (org == null) return false;

        org.IsDeleted = true;
        org.UpdatedAt = DateTime.UtcNow;
        repo.Update(org);
        await repo.SaveChangesAsync();

        logger.LogInformation("Organization '{Name}' deleted by SA {UserId}", org.Name, tenant.UserId);
        return true;
    }
}
