namespace MultiTenantInventory.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendSetPasswordEmailAsync(string toEmail, string name, string organizationName, string token);
    Task SendAccountCreatedEmailAsync(string toEmail, string name, string organizationName, string email, string role);
    Task SendOrderApprovedEmailAsync(string toEmail, string customerName, string organizationName, Guid orderId, DateTime orderDate, List<(string ProductName, int Qty, decimal Price)> items, decimal total);
    Task SendOrderRejectedEmailAsync(string toEmail, string customerName, string organizationName, Guid orderId, DateTime orderDate, decimal total, string reason);
}
