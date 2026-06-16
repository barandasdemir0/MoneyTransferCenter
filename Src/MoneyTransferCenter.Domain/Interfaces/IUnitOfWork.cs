using MoneyTransferCenter.Domain.Interfaces.Repositories;

namespace MoneyTransferCenter.Domain.Interfaces;

public interface IUnitOfWork:IDisposable
{
    IAccountRepository Accounts { get; }
    ITransactionRepository Transactions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
