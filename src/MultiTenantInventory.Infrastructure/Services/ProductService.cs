using Microsoft.Extensions.Logging;
namespace MultiTenantInventory.Infrastructure.Services;

public class ProductService(
    IProductRepository productRepo,
    ICategoryRepository categoryRepo,
    ITenantService tenant,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await productRepo.GetAllWithCategoryAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await productRepo.GetByIdWithCategoryAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var category = await categoryRepo.GetByIdAsync(dto.CategoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found.");

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            OrganizationId = tenant.OrganizationId
        };

        await productRepo.AddAsync(product);
        await productRepo.SaveChangesAsync();

        logger.LogInformation("[Org:{OrgId}] Product '{Name}' created by {UserId}",
            tenant.OrganizationId, dto.Name, tenant.UserId);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto)
    {
        var product = await productRepo.GetByIdAsync(id);
        if (product == null) return null;

        if (product.OrganizationId != tenant.OrganizationId)
            throw new UnauthorizedAccessException("Access denied.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;

        productRepo.Update(product);
        await productRepo.SaveChangesAsync();

        var category = await categoryRepo.GetByIdAsync(product.CategoryId);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? ""
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await productRepo.GetByIdAsync(id);
        if (product == null) return false;

        if (product.OrganizationId != tenant.OrganizationId)
            throw new UnauthorizedAccessException("Access denied.");

        productRepo.Remove(product);
        await productRepo.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        ImageUrl = p.ImageUrl,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? ""
    };
}
