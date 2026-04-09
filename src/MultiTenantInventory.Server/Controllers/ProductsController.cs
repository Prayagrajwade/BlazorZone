using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "User")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetAll()
    {
        try
        {
            var products = await service.GetAllAsync();
            return Ok(ApiResponse<List<ProductDto>>.Ok(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ProductDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id)
    {
        try
        {
            var product = await service.GetByIdAsync(id);
            if (product == null)
                return NotFound(ApiResponse<ProductDto>.Fail("Product not found."));
            return Ok(ApiResponse<ProductDto>.Ok(product));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await service.CreateAsync(dto);
            return Ok(ApiResponse<ProductDto>.Ok(product));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = await service.UpdateAsync(id, dto);
            if (product == null)
                return NotFound(ApiResponse<ProductDto>.Fail("Product not found."));
            return Ok(ApiResponse<ProductDto>.Ok(product));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Product not found."));
            return Ok(ApiResponse<bool>.Ok(true, "Product deleted."));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
    }
}
