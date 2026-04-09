using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IOrderService
{
    Task<List<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
    Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto);
}
