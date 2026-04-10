namespace MultiTenantInventory.Infrastructure.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected readonly AppDbContext _db = db;
    protected readonly DbSet<T> _dbSet = db.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id)
        => await _dbSet.FindAsync(id);

    public async Task<List<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();

    public IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public IQueryable<T> QueryNoFilters()
        => _dbSet.IgnoreQueryFilters();
}
