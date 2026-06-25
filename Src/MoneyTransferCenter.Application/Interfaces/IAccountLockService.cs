namespace MoneyTransferCenter.Application.Interfaces;

public interface IAccountLockService
{
    // bu metod, belirli bir hesap kimliği için bir kilit edinmek için kullanılır. Bu, aynı anda birden fazla iş parçacığının aynı hesap üzerinde işlem yapmasını önler.
    Task<IDisposable> AcquireLockAsync(Guid accountId, CancellationToken cancellationToken = default);
}
