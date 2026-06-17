using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;

namespace MoneyTransferCenter.Infrastructure.Repositories;

public sealed class OutboxMessageRepository : GenericRepository<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

    public async Task<List<OutboxMessage>> GetUnprocessedAsync(int batchSize = 50)
    {
        return await _dbSet
             .Where(o => !o.IsProcessed && o.RetryCount < 3)  // 3'ten fazla hata alırsa bırak
             .OrderBy(o => o.CreatedAt)                        // Eskiden yeniye işle
             .Take(batchSize)
             .ToListAsync();
    }
}
