using Microsoft.AspNetCore.Authorization;

namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> Get()
    {
        try
        {
            var dashboard = await service.GetAdminDashboardAsync();
            return Ok(ApiResponse<DashboardDto>.Ok(dashboard));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<DashboardDto>.Fail(ex.Message));
        }
    }

    [HttpGet("sa")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<ActionResult<ApiResponse<SaDashboardDto>>> GetSaDashboard()
    {
        try
        {
            var dashboard = await service.GetSaDashboardAsync();
            return Ok(ApiResponse<SaDashboardDto>.Ok(dashboard));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<SaDashboardDto>.Fail(ex.Message));
        }
    }
}
