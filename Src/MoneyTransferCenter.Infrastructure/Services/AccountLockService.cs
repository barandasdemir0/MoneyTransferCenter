using MoneyTransferCenter.Application.Interfaces;
using System.Collections.Concurrent;

namespace MoneyTransferCenter.Infrastructure.Services;

public sealed class AccountLockService : IAccountLockService
{

    // Bu sınıf, belirli bir hesap kimliği için kilit edinmek ve serbest bırakmak için kullanılan bir hizmettir. Bu, aynı anda birden fazla iş parçacığının aynı hesap üzerinde işlem yapmasını önler.

    //semaphoreSlim sınıfı, birden fazla iş parçacığının aynı anda belirli bir kaynağa erişmesini sınırlamak için kullanılan bir senkronizasyon ilkesidir. Bu sınıf, belirli bir sayıda iş parçacığının aynı anda belirli bir kod bloğunu çalıştırmasına izin verir.
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _accountLocks = new();

    public async Task<IDisposable> AcquireLockAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var accountLock = GetOrCreateLock(accountId); //kilit oluştur veya getir
        await accountLock.WaitAsync(cancellationToken); //SemaphoreSlim nesnesinin WaitAsync metodu çağrılır, bu da iş parçacığının kilidi almasını sağlar. Eğer kilit zaten alınmışsa, iş parçacığı bekler.
        return new LockRelease(accountLock);//LockRelease nesnesi oluşturulur ve döndürülür. Bu nesne, kilidi serbest bırakmak için kullanılır.


    }


    //  kilidi oluşturur veya getirir.
    private SemaphoreSlim GetOrCreateLock(Guid accountId)
    {
        return _accountLocks.GetOrAdd(
            accountId, // accountId için kilit oluştur veya getir
            _ => new SemaphoreSlim(1, 1));  // _ ile yeni SemaphoreSlim oluşturulur ve 1 iş parçacığına izin verilir, maksimum 1 iş parçacığı aynı anda kilidi alabilir.
    }


    // Kilit yönetimi, eşzamanlı erişim kontrolü.
    private sealed class LockRelease : IDisposable
    {
        private readonly SemaphoreSlim _accountLock; // 5 kelime : SemaphoreSlim nesnesi, kilidi temsil eder ve iş parçacıklarının bu kilidi almasını ve serbest bırakmasını sağlar.

        private bool _isReleased; //bu alan, kilidin serbest bırakılıp bırakılmadığını takip eder. Eğer kilit zaten serbest bırakılmışsa, Dispose metodu tekrar çağrıldığında hiçbir işlem yapılmaz.

        public LockRelease(SemaphoreSlim accountLock)
        {
            _accountLock = accountLock;
        }

        public void Dispose()
        {
            if (_isReleased)
                return;

            _isReleased = true; //kilit serbest bırakıldı olarak işaretlenir
            _accountLock.Release(); //SemaphoreSlim nesnesinin Release metodu çağrılır, bu da kilidi serbest bırakır ve diğer iş parçacıklarının bu kilidi almasına izin verir.
        }
    }
}
