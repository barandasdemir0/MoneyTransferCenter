using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Domain.Interfaces.Repositories;

public interface IAccountRepository : IGenericRepository<Account>
{
    Task<Account?> GetByIbanAsync(string iban);
    Task<Account?> GetByUserIdAsync(Guid userId);
    Task<bool> IbanExistsAsync(string iban);
}
