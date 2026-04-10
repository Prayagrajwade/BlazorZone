using System.Net;
using System.Net.Mail;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MultiTenantInventory.Infrastructure.Services;

public class EmailService(
    IEmailTemplateRepository templateRepo,
    IOptions<SmtpSettings> smtp,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _smtp = smtp.Value;
    private const string Currency = "Rs";

    public async Task SendSetPasswordEmailAsync(string toEmail, string name, string organizationName, string token)
    {
        var url = $"{_smtp.BaseUrl}/set-password?token={Uri.EscapeDataString(token)}";
        var body = await RenderAsync("SetPassword", new Dictionary<string, string>
        {
            ["Name"] = name,
            ["OrganizationName"] = organizationName,
            ["SetPasswordUrl"] = url,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await SendAsync(toEmail, "Set Your Password – MultiTenant Inventory", body);
    }

    public async Task SendAccountCreatedEmailAsync(string toEmail, string name, string organizationName, string email, string role)
    {
        var body = await RenderAsync("AccountCreated", new Dictionary<string, string>
        {
            ["Name"] = name,
            ["Email"] = email,
            ["OrganizationName"] = organizationName,
            ["Role"] = role,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await SendAsync(toEmail, "Your Account Has Been Created", body);
    }

    public async Task SendOrderApprovedEmailAsync(
        string toEmail, string customerName, string organizationName,
        Guid orderId, DateTime orderDate,
        List<(string ProductName, int Qty, decimal Price)> items,
        decimal total)
    {
        var sb = new StringBuilder();
        foreach (var item in items)
        {
            sb.AppendLine($@"<tr>
              <td>{WebUtility.HtmlEncode(item.ProductName)}</td>
              <td style='text-align:center;'>{item.Qty}</td>
              <td style='text-align:right;'>{Currency}{item.Price:F2}</td>
              <td style='text-align:right;'>{Currency}{(item.Price * item.Qty):F2}</td>
            </tr>");
        }

        var body = await RenderAsync("OrderApproved", new Dictionary<string, string>
        {
            ["OrderId"] = orderId.ToString()[..8].ToUpper(),
            ["OrderDate"] = orderDate.ToString("dd MMM yyyy, HH:mm") + " UTC",
            ["CustomerName"] = customerName,
            ["CustomerEmail"] = toEmail,
            ["OrganizationName"] = organizationName,
            ["OrderItems"] = sb.ToString(),
            ["Currency"] = Currency,
            ["TotalAmount"] = total.ToString("F2"),
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await SendAsync(toEmail, $"Order #{orderId.ToString()[..8].ToUpper()} Approved – Invoice", body);
    }

    public async Task SendOrderRejectedEmailAsync(
        string toEmail, string customerName, string organizationName,
        Guid orderId, DateTime orderDate, decimal total, string reason)
    {
        var body = await RenderAsync("OrderRejected", new Dictionary<string, string>
        {
            ["OrderId"] = orderId.ToString()[..8].ToUpper(),
            ["OrderDate"] = orderDate.ToString("dd MMM yyyy, HH:mm") + " UTC",
            ["CustomerName"] = customerName,
            ["OrganizationName"] = organizationName,
            ["Currency"] = Currency,
            ["TotalAmount"] = total.ToString("F2"),
            ["RejectionReason"] = string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await SendAsync(toEmail, $"Order #{orderId.ToString()[..8].ToUpper()} Rejected", body);
    }

    private async Task<string> RenderAsync(string templateName, Dictionary<string, string> values)
    {
        var template = await templateRepo.GetByNameAsync(templateName);
        var html = template?.HtmlBody ?? LoadEmbeddedTemplate(templateName);

        foreach (var kv in values)
            html = html.Replace($"{{{{{kv.Key}}}}}", kv.Value);

        return html;
    }

    private static string LoadEmbeddedTemplate(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith($"{name}.html", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            return $"<p>Email template '{name}' not found.</p>";

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Host) || string.IsNullOrWhiteSpace(_smtp.Username))
        {
            logger.LogWarning("Email SMTP not configured. Skipping email to {To}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_smtp.FromEmail, _smtp.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send '{Subject}' to {To}", subject, toEmail);
        }
    }
}
