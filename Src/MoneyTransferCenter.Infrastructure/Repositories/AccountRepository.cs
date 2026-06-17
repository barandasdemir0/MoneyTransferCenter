using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;

namespace MoneyTransferCenter.Infrastructure.Repositories;

public sealed class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext appDbContext) : base(appDbContext)
    {
    }

    public override async Task<List<Account>> GetAllAsync()
    {
        return await _dbSet
       .Include(a => a.User)
       .ToListAsync();
    }
    public override async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _dbSet
           .Include(a => a.User)
           .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByIbanAsync(string iban)
    {
        return await _dbSet
          .Include(a => a.User)
          .FirstOrDefaultAsync(a => a.IBAN == iban);
    }

    public async Task<Account?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
           .Include(a => a.User)
           .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<bool> IbanExistsAsync(string iban)
    {
        return await _dbSet.AnyAsync(a => a.IBAN == iban);
    }
}
