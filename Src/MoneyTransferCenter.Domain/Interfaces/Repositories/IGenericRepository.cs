using MoneyTransferCenter.Domain.Common;

namespace MoneyTransferCenter.Domain.Interfaces.Repositories;

public interface IGenericRepository<T> where T:BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity); // Soft delete (BaseEntity.IsDeleted = true yapacak)
}
