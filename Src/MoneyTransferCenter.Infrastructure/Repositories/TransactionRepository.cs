using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;

namespace MoneyTransferCenter.Infrastructure.Repositories;

public sealed class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

    public async Task<List<Transaction>> GetAllByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
            .Where(t => t.SenderAccountId == accountId || t.ReceiverAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.SenderAccount).ThenInclude(a => a!.User)
            .Include(t => t.ReceiverAccount).ThenInclude(a => a!.User)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber)
    {
        return await _dbSet
           .Include(t => t.SenderAccount)    // Gönderen bilgisini de Join ile getir
           .Include(t => t.ReceiverAccount)  // Alıcı bilgisini de Join ile getir
           .FirstOrDefaultAsync(t => t.ReferenceNumber == referenceNumber);
    }

    public async Task<List<Transaction>> GetReceivedByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
           .Where(t => t.ReceiverAccountId == accountId)
           .OrderByDescending(t => t.CreatedAt)
           .Skip((page - 1) * pageSize)
           .Take(pageSize)
           .Include(t => t.SenderAccount).ThenInclude(a => a!.User)         // Bana kim para gönderdi? O bilgiyi getir
           .ToListAsync();
    }

    public async Task<List<Transaction>> GetSentByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20)
    {
        return await _dbSet
          .Where(t => t.SenderAccountId == accountId)
          .OrderByDescending(t => t.CreatedAt) // En yeniler en üstte
          .Skip((page - 1) * pageSize)         // Sayfalama (Pagination) atlaması
          .Take(pageSize)                      // Sadece istenen sayfa kadarını getir
          .Include(t => t.ReceiverAccount).ThenInclude(a => a!.User)     // Ben kime para gönderdim? O bilgiyi getir
          .ToListAsync();
    }
}
