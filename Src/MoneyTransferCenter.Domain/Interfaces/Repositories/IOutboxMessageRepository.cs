using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Domain.Interfaces.Repositories;

public interface IOutboxMessageRepository
{
    Task AddAsync(OutboxMessage message);
    Task<List<OutboxMessage>> GetUnprocessedAsync(int batchSize = 50);
    void Update(OutboxMessage message);
}
