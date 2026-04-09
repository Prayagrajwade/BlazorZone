using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetAdminDashboardAsync();
    Task<SaDashboardDto> GetSaDashboardAsync();
}
