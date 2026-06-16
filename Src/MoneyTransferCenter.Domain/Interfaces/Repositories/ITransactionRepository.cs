using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Domain.Interfaces.Repositories;

public interface ITransactionRepository : IGenericRepository<Transaction>
{
    Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber);
    Task<List<Transaction>> GetSentByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20);
    Task<List<Transaction>> GetReceivedByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20);
    Task<List<Transaction>> GetAllByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20);
}
