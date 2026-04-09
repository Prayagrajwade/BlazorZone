using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "User")]
public class CategoriesController(ICategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        try
        {
            var categories = await service.GetAllAsync();
            return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CategoryDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = await service.CreateAsync(dto);
            return Ok(ApiResponse<CategoryDto>.Ok(category));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(Guid id, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = await service.UpdateAsync(id, dto);
            if (category == null)
                return NotFound(ApiResponse<CategoryDto>.Fail("Category not found."));
            return Ok(ApiResponse<CategoryDto>.Ok(category));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.Fail(ex.Message));
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
                return NotFound(ApiResponse<bool>.Fail("Category not found."));
            return Ok(ApiResponse<bool>.Ok(true, "Category deleted."));
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

