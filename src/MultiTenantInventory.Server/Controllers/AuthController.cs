namespace MultiTenantInventory.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await authService.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Message));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [HttpPost("set-password")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> SetPassword([FromBody] SetPasswordDto dto)
    {
        try
        {
            var result = await authService.SetPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(ApiResponse<AuthResponseDto>.Fail(result.Message));
            return Ok(ApiResponse<AuthResponseDto>.Ok(result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
        
    }
}
