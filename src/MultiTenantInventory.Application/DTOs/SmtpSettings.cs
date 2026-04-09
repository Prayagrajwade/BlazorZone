namespace MultiTenantInventory.Application.DTOs;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "MultiTenant Inventory";
    public bool EnableSsl { get; set; } = true;
    public string BaseUrl { get; set; } = "https://localhost:5001";
}
