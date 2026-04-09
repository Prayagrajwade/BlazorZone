namespace MultiTenantInventory.Infrastructure.Services;

public class AuthService(IUserRepository userRepo, IConfiguration config) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await userRepo.GetByEmailAsync(dto.Email);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "Invalid credentials." };

        if (!user.IsActive)
            return new AuthResponseDto { Success = false, Message = "Account is deactivated." };

        if (!user.Organization.IsActive && user.Role != UserRole.SuperAdmin)
            return new AuthResponseDto { Success = false, Message = "Your organization is deactivated." };

        if (!VerifyPassword(dto.Password, user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "Invalid credentials." };

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Login successful.",
            User = new UserInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization.Name
            }
        };
    }

    public async Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto dto)
    {
        var user = await userRepo.GetByTokenAsync(dto.Token);

        if (user == null)
            return new AuthResponseDto { Success = false, Message = "Invalid or expired token." };

        user.PasswordHash = HashPassword(dto.Password);
        user.SetPasswordToken = null;
        user.SetPasswordTokenExpiry = null;
        user.IsActive = true;

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        return new AuthResponseDto { Success = true, Message = "Password set successfully. You may now log in." };
    }

    public string GenerateSetPasswordToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string stored)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, stored);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false; 
        }
    }

    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "ThisIsASuperSecretKeyForDev12345!"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("OrganizationId", user.OrganizationId.ToString())
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "MultiTenantInventory",
            audience: config["Jwt:Audience"] ?? "MultiTenantInventory",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
