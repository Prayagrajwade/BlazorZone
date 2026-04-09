using MultiTenantInventory.Domain.Enums;

namespace MultiTenantInventory.Domain.Entities;

public class Order : TenantEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
