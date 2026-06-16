using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Domain.Common;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;

namespace MoneyTransferCenter.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _appDbContext;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
        _dbSet = appDbContext.Set<T>();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}
