namespace MultiTenantInventory.Infrastructure.Services;

public class CategoryService(ICategoryRepository repo, ITenantService tenant) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await repo.GetAllWithProductCountAsync();
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            ProductCount = c.Products.Count(p => !p.IsDeleted)
        }).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            OrganizationId = tenant.OrganizationId
        };

        await repo.AddAsync(category);
        await repo.SaveChangesAsync();

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, CreateCategoryDto dto)
    {
        var category = await repo.GetByIdAsync(id);
        if (category == null) return null;

        if (category.OrganizationId != tenant.OrganizationId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        category.Name = dto.Name;
        repo.Update(category);
        await repo.SaveChangesAsync();

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await repo.GetByIdAsync(id);
        if (category == null) return false;

        if (category.OrganizationId != tenant.OrganizationId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }
            
        repo.Remove(category);
        await repo.SaveChangesAsync();
        return true;
    }
}
