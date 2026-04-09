using Microsoft.Extensions.Logging;

namespace MultiTenantInventory.Infrastructure.Services;

public class UserService(
    IUserRepository userRepo,
    IOrganizationRepository orgRepo,
    ITenantService tenant,
    IAuthService authService,
    IEmailService emailService,
    ILogger<UserService> logger) : IUserService
{

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await userRepo.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetByOrgAsync(Guid orgId)
    {
        var users = await userRepo.GetByOrgAsync(orgId, UserRole.Admin);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        Guid targetOrgId;

        if (tenant.IsSuperAdmin)
        {
            if (dto.OrganizationId == null || dto.OrganizationId == Guid.Empty)
                throw new InvalidOperationException("OrganizationId is required.");
            targetOrgId = dto.OrganizationId.Value;
            if (dto.Role != "Admin")
                throw new InvalidOperationException("Super Admin can only create Admin users.");
        }
        else
        {
            targetOrgId = tenant.OrganizationId;
            if (dto.Role == "SuperAdmin" || dto.Role == "Admin")
                throw new InvalidOperationException("Admins can only create User-role accounts.");
        }

        if (await userRepo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already in use.");

        var org = await orgRepo.GetByIdWithStatsAsync(targetOrgId);
        if (org == null)
            throw new InvalidOperationException("Organization not found.");
        if (!org.IsActive)
            throw new InvalidOperationException("Organization is deactivated.");

        var token = authService.GenerateSetPasswordToken();

        var user = new AppUser
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = Enum.Parse<UserRole>(dto.Role),
            OrganizationId = targetOrgId,
            IsActive = false,
            PasswordHash = "",
            SetPasswordToken = token,
            SetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        await emailService.SendSetPasswordEmailAsync(dto.Email, dto.Name, org.Name, token);
        await emailService.SendAccountCreatedEmailAsync(dto.Email, dto.Name, org.Name, dto.Email, dto.Role);

        logger.LogInformation("[Org:{OrgId}] User '{Email}' (role: {Role}) created by {Creator}",
            targetOrgId, dto.Email, dto.Role, tenant.UserId);

        return MapToDto(user);
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var user = await ResolveUserAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        userRepo.Update(user);
        await userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await ResolveUserAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        userRepo.Update(user);
        await userRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendInviteAsync(Guid id)
    {
        var user = await ResolveUserAsync(id);
        if (user == null) return false;

        if (!string.IsNullOrEmpty(user.PasswordHash))
            throw new InvalidOperationException("User has already set their password.");

        var token = authService.GenerateSetPasswordToken();
        user.SetPasswordToken = token;
        user.SetPasswordTokenExpiry = DateTime.UtcNow.AddHours(24);
        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        await emailService.SendSetPasswordEmailAsync(user.Email, user.Name, user.Organization?.Name ?? "", token);
        return true;
    }

    private async Task<AppUser?> ResolveUserAsync(Guid id)
    {
        if (tenant.IsSuperAdmin)
        {
            var users = await userRepo.GetByOrgAsync(Guid.Empty);
            var user = await userRepo.GetByIdWithOrgAsync(id);
            if (user != null && user.Role != UserRole.Admin) return null;
            return user;
        }

        var u = await userRepo.GetByIdAsync(id);
        if (u != null && u.OrganizationId != tenant.OrganizationId)
        {
            logger.LogWarning("Unauthorized access attempt by {UserId} on user {TargetId}", tenant.UserId, id);
            throw new UnauthorizedAccessException("Access denied.");
        }
        return u;
    }

    private static UserDto MapToDto(AppUser u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive,
        OrganizationId = u.OrganizationId,
        CreatedAt = u.CreatedAt
    };
}
