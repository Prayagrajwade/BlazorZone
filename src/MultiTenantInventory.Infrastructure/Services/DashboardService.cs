namespace MultiTenantInventory.Infrastructure.Services;

public class DashboardService(
    IProductRepository productRepo,
    IOrderRepository orderRepo,
    IUserRepository userRepo,
    ICategoryRepository categoryRepo,
    IOrganizationRepository orgRepo) : IDashboardService
{
    public async Task<DashboardDto> GetAdminDashboardAsync()
    {
        var products = await productRepo.GetAllAsync();
        var orders = await orderRepo.GetAllWithDetailsAsync();
        var users = await userRepo.GetAllAsync();
        var categories = await categoryRepo.GetAllAsync();
        var recentOrders = await orderRepo.GetRecentAsync(5);

        return new DashboardDto
        {
            TotalProducts = products.Count,
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            TotalUsers = users.Count,
            TotalCategories = categories.Count,
            LowStockProducts = products.Count(p => p.StockQuantity < 10),
            RecentOrders = recentOrders.Select(o => new RecentOrderDto
            {
                Id = o.Id,
                UserName = o.User?.Name ?? "",
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            }).ToList()
        };
    }

    public async Task<SaDashboardDto> GetSaDashboardAsync()
    {
        var orgs = await orgRepo.GetAllWithStatsAsync();
        var allUsers = orgs.SelectMany(o => o.Users).Where(u => !u.IsDeleted).ToList();

        return new SaDashboardDto
        {
            TotalOrganizations = orgs.Count,
            ActiveOrganizations = orgs.Count(o => o.IsActive),
            TotalUsers = allUsers.Count
        };
    }
}
