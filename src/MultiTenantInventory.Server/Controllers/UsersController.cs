using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
    {
        try
        {
            var users = await service.GetAllAsync();
            return Ok(ApiResponse<List<UserDto>>.Ok(users));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserDto>>.Fail(ex.Message));
        }
    }

    [HttpGet("by-org/{orgId}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetByOrg(Guid orgId)
    {
        try
        {
            var users = await service.GetByOrgAsync(orgId);
            return Ok(ApiResponse<List<UserDto>>.Ok(users));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<UserDto>>.Fail(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrSA")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), null,
                ApiResponse<UserDto>.Ok(user, "User created. Set-password email sent."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/resend-invite")]
    [Authorize(Policy = "AdminOrSA")]
    public async Task<ActionResult<ApiResponse<bool>>> ResendInvite(Guid id)
    {
        try
        {
            var result = await service.ResendInviteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("User not found."));
            return Ok(ApiResponse<bool>.Ok(true, "Invitation email resent."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Policy = "AdminOrSA")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleActive(Guid id)
    {
        try
        {
            var result = await service.ToggleActiveAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("User not found."));
            return Ok(ApiResponse<bool>.Ok(true));
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

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOrSA")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await service.DeleteAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("User not found."));
            return Ok(ApiResponse<bool>.Ok(true, "User deleted."));
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
