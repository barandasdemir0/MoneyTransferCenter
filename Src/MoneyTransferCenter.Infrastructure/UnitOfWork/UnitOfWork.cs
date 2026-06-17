using Microsoft.EntityFrameworkCore.Storage;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Infrastructure.Data;

namespace MoneyTransferCenter.Infrastructure.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction; // Aktif veritabanı transaction'ını tutar
    private bool _disposed; // Dispose işleminin birden fazla kez çalışmasını engeller

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }



    //bu method, transaction başlatmak için kullanılır. Eğer zaten bir transaction varsa, yeni bir transaction başlatılmaz.
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    // Bu method, transaction'ı commit etmek için kullanılır. Eğer bir hata oluşursa, transaction rollback yapılır.
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Önce değişiklikleri kaydet (ChangeTracker çalışır)
            await SaveChangesAsync(cancellationToken);
            // Sonra transaction'ı commit et
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            // Herhangi bir hata olursa otomatik rollback
            await RollbackTransactionAsync(cancellationToken);
            throw; // Hatayı yukarıya fırlat (Application katmanı yakalasın)
        }
        finally
        {
            DisposeTransaction();
        }
    }


    // Dispose pattern'ini uygular. Bu method, UnitOfWork nesnesi kullanıldıktan sonra kaynakları serbest bırakmak için çağrılır.
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeTransaction();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    // Bu method, aktif transaction içindeki işlemleri geri alır.
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            DisposeTransaction();
        }
    }

    // Bu method, aktif transaction'ı dispose eder ve null yapar. Böylece bir sonraki transaction başlatılabilir.
    private void DisposeTransaction()
    {
        if (_currentTransaction != null)
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    // Bu method, değişiklikleri veritabanına kaydetmek için kullanılır. Eğer bir hata oluşursa, exception fırlatılır.
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
