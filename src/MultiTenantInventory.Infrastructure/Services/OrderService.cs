using Microsoft.Extensions.Logging;

namespace MultiTenantInventory.Infrastructure.Services;

public class OrderService(
    IOrderRepository _orderRepo,
    IProductRepository _productRepo,
    IUserRepository _userRepo,
    ITenantService _tenant,
    IEmailService _emailService,
    ILogger<OrderService> _logger) : IOrderService
{
    public async Task<List<OrderDto>> GetAllAsync()
    {
        List<Order> orders;

        if (_tenant.Role == "User")
            orders = await _orderRepo.GetByUserAsync(_tenant.UserId);
        else
            orders = await _orderRepo.GetAllWithDetailsAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id)
    {
        var order = await _orderRepo.GetByIdWithDetailsAsync(id);
        if (order == null) return null;

        if (_tenant.Role == "User" && order.UserId != _tenant.UserId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToDto(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("Order must have at least one item.");

        var order = new Order
        {
            UserId = _tenant.UserId,
            OrganizationId = _tenant.OrganizationId,
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };

        foreach (var item in dto.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found.");

            if (product.OrganizationId != _tenant.OrganizationId)
                throw new UnauthorizedAccessException("Access denied.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = product.Price,
                OrganizationId = _tenant.OrganizationId
            });

            order.TotalAmount += product.Price * item.Quantity;
        }

        await _orderRepo.AddAsync(order);
        await _orderRepo.SaveChangesAsync();

        _logger.LogInformation("[Org:{OrgId}] Order {OrderId} placed by User {UserId} — ₹{Amount} — awaiting admin approval",
            _tenant.OrganizationId, order.Id, _tenant.UserId, order.TotalAmount);

        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }

    /// <summary>
    /// Admin updates order status.
    /// On Approved  → stock deducted + invoice email sent.
    /// On Rejected  → rejection email sent, no stock change.
    /// </summary>
    public async Task<bool> UpdateStatusAsync(Guid id, UpdateOrderStatusDto dto)
    {
        if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
            throw new InvalidOperationException($"Invalid order status '{dto.Status}'.");

        var order = await _orderRepo.GetByIdWithDetailsAsync(id);
        if (order == null) return false;

        if (order.OrganizationId != _tenant.OrganizationId)
            throw new UnauthorizedAccessException("Access denied.");

        // Guard: can only approve/reject Pending orders
        if (newStatus == OrderStatus.Approved || newStatus == OrderStatus.Rejected)
        {
            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException($"Can only approve/reject orders in Pending status. Current: {order.Status}.");
        }

        if (newStatus == OrderStatus.Approved)
            await HandleApprovalAsync(order);
        else if (newStatus == OrderStatus.Rejected)
            await HandleRejectionAsync(order, dto.RejectionReason);
        else
        {
            order.Status = newStatus;
            _orderRepo.Update(order);
            await _orderRepo.SaveChangesAsync();
        }

        _logger.LogInformation("[Org:{OrgId}] Order {OrderId} → {Status} by Admin {UserId}",
            _tenant.OrganizationId, id, dto.Status, _tenant.UserId);

        return true;
    }

    // ─── private helpers ────────────────────────────────────────────────────

    private async Task HandleApprovalAsync(Order order)
    {
        // Deduct stock for each item
        foreach (var item in order.Items)
        {
            var product = await _productRepo.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found during approval.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}' at approval time. Please reject the order.");

            product.StockQuantity -= item.Quantity;
            _productRepo.Update(product);
        }

        order.Status = OrderStatus.Approved;
        order.ReviewedAt = DateTime.UtcNow;
        _orderRepo.Update(order);
        await _orderRepo.SaveChangesAsync();

        // Send invoice email
        var buyer = await _userRepo.GetByIdWithOrgAsync(order.UserId);
        if (buyer != null)
        {
            var lineItems = order.Items.Select(i => (
                i.Product?.Name ?? "Product",
                i.Quantity,
                i.Price
            )).ToList();

            await _emailService.SendOrderApprovedEmailAsync(
                buyer.Email,
                buyer.Name,
                buyer.Organization?.Name ?? "",
                order.Id,
                order.CreatedAt,
                lineItems,
                order.TotalAmount);
        }
    }

    private async Task HandleRejectionAsync(Order order, string? reason)
    {
        order.Status = OrderStatus.Rejected;
        order.RejectionReason = reason;
        order.ReviewedAt = DateTime.UtcNow;
        _orderRepo.Update(order);
        await _orderRepo.SaveChangesAsync();

        // Send rejection email
        var buyer = await _userRepo.GetByIdWithOrgAsync(order.UserId);
        if (buyer != null)
        {
            await _emailService.SendOrderRejectedEmailAsync(
                buyer.Email,
                buyer.Name,
                buyer.Organization?.Name ?? "",
                order.Id,
                order.CreatedAt,
                order.TotalAmount,
                reason ?? "No reason provided.");
        }
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        UserName = o.User?.Name ?? "",
        TotalAmount = o.TotalAmount,
        Status = o.Status.ToString(),
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? "",
            Quantity = i.Quantity,
            Price = i.Price
        }).ToList()
    };
}
