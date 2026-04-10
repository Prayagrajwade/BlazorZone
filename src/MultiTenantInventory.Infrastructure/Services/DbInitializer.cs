using System.Reflection;
using Microsoft.Extensions.Logging;

namespace MultiTenantInventory.Infrastructure.Services;

public class DbInitializer(
    AppDbContext db,
    IOrganizationRepository orgRepo,
    IUserRepository userRepo,
    IEmailTemplateRepository templateRepo,
    IAuthService authService,
    ILogger<DbInitializer> logger)
{
    public async Task SeedAsync()
    {
        await db.Database.MigrateAsync();

        await SeedSuperAdminAsync();
        await SeedEmailTemplatesAsync();
    }


    private async Task SeedSuperAdminAsync()
    {
        var saExists = await userRepo.EmailExistsAsync("sa@system.com");
        if (saExists)
        {
            logger.LogInformation("Seeder Super Admin already exists. Skipping.");
            return;
        }

        logger.LogInformation("Seeder Seeding Super Admin...");

        var systemOrg = new Organization
        {
            Name = "System Administration",
            IsActive = true
        };

        await orgRepo.AddAsync(systemOrg);
        await orgRepo.SaveChangesAsync();

        var sa = new AppUser
        {
            Name = "Super Admin",
            Email = "sa@system.com",
            PasswordHash = authService.HashPassword("SuperAdmin@123"),
            Role = UserRole.SuperAdmin,
            OrganizationId = systemOrg.Id,
            IsActive = true
        };

        await userRepo.AddAsync(sa);
        await userRepo.SaveChangesAsync();

        logger.LogInformation("Super Admin seeded.");
        logger.LogInformation("sa@system.com");
        logger.LogInformation("SuperAdmin@123");
        logger.LogInformation("Change password before going to production!");
    }

    private static readonly (string Name, string Subject)[] TemplateDefinitions =
    [
        ("SetPassword",     "Set Your Password – MultiTenant Inventory"),
        ("AccountCreated",  "Your Account Has Been Created"),
        ("OrderApproved",   "Your Order Has Been Approved"),
        ("OrderRejected",   "Your Order Has Been Rejected")
    ];

    private async Task SeedEmailTemplatesAsync()
    {
        logger.LogInformation("Checking email templates...");
        var seeded = 0;

        foreach (var (name, subject) in TemplateDefinitions)
        {
            if (await templateRepo.ExistsByNameAsync(name))
            {
                continue;
            }

            var html = LoadEmbeddedHtml(name);
            if (html == null)
            {
                continue;
            }

            await templateRepo.AddAsync(new EmailTemplate
            {
                Name    = name,
                Subject = subject,
                HtmlBody = html
            });

            seeded++;
            logger.LogInformation("Email template '{Name}' seeded.", name);
        }

        if (seeded > 0)
            await templateRepo.SaveChangesAsync();
        else
            logger.LogInformation("All email templates already up to date.");
    }

    private static string? LoadEmbeddedHtml(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith($"{templateName}.html", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null) return null;

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
