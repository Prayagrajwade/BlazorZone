using Microsoft.AspNetCore.Http;

namespace MultiTenantInventory.Infrastructure.Services;

public class TenantService(IHttpContextAccessor _httpContextAccessor) : ITenantService
{

    public Guid OrganizationId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("OrganizationId");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }

    public string Role
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }

    public bool IsSuperAdmin => Role == "SuperAdmin";
}
