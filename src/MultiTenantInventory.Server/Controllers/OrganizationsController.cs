using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdmin")]
public class OrganizationsController(IOrganizationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrganizationDto>>>> GetAll()
    {
        try
        {
            var orgs = await service.GetAllAsync();
            return Ok(ApiResponse<List<OrganizationDto>>.Ok(orgs));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<OrganizationDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> GetById(Guid id)
    {
        try
        {
            var org = await service.GetByIdAsync(id);
            if (org == null)
                return NotFound(ApiResponse<OrganizationDto>.Fail("Organization not found."));
            return Ok(ApiResponse<OrganizationDto>.Ok(org));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<OrganizationDto>.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> Create([FromBody] CreateOrganizationDto dto)
    {
        try
        {
            var org = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = org.Id }, ApiResponse<OrganizationDto>.Ok(org));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrganizationDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationDto>>> Update(Guid id, [FromBody] UpdateOrganizationDto dto)
    {
        try
        {
            var org = await service.UpdateAsync(id, dto);
            if (org == null)
                return NotFound(ApiResponse<OrganizationDto>.Fail("Organization not found."));
            return Ok(ApiResponse<OrganizationDto>.Ok(org));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrganizationDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Organization not found."));
            return Ok(ApiResponse<bool>.Ok(true, "Organization deleted."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
    }
}
