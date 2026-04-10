using System.Linq.Expressions;

namespace MultiTenantInventory.Application.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task SaveChangesAsync();
    IQueryable<T> Query();
    IQueryable<T> QueryNoFilters();
}
