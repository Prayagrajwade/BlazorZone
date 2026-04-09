namespace MultiTenantInventory.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasQueryFilter(o => !o.IsDeleted);
            e.HasIndex(o => o.Name);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.OrganizationId);

            e.HasOne(u => u.Organization)
             .WithMany(o => o.Users)
             .HasForeignKey(u => u.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(u => !u.IsDeleted &&
                (_tenantService.IsSuperAdmin || u.OrganizationId == _tenantService.OrganizationId));
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.OrganizationId);

            e.HasOne(c => c.Organization)
             .WithMany(o => o.Categories)
             .HasForeignKey(c => c.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(c => !c.IsDeleted &&
                (_tenantService.IsSuperAdmin || c.OrganizationId == _tenantService.OrganizationId));
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.OrganizationId);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Organization)
             .WithMany(o => o.Products)
             .HasForeignKey(p => p.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(p => !p.IsDeleted &&
                (_tenantService.IsSuperAdmin || p.OrganizationId == _tenantService.OrganizationId));
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasIndex(o => o.OrganizationId);
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Organization)
             .WithMany(org => org.Orders)
             .HasForeignKey(o => o.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(o => !o.IsDeleted &&
                (_tenantService.IsSuperAdmin || o.OrganizationId == _tenantService.OrganizationId));
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.HasIndex(oi => oi.OrganizationId);
            e.Property(oi => oi.Price).HasColumnType("decimal(18,2)");

            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Product)
             .WithMany()
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(oi => oi.Organization)
             .WithMany()
             .HasForeignKey(oi => oi.OrganizationId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(oi => !oi.IsDeleted &&
                (_tenantService.IsSuperAdmin || oi.OrganizationId == _tenantService.OrganizationId));
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _tenantService.UserId.ToString();
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _tenantService.UserId.ToString();
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.OrganizationId == Guid.Empty)
            {
                entry.Entity.OrganizationId = _tenantService.OrganizationId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
