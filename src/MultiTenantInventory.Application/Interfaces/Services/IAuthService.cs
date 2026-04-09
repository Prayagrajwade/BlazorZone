using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto dto);
    string GenerateSetPasswordToken();
    string HashPassword(string password);
}
