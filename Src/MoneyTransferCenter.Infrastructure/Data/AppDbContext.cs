using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Common;
using MoneyTransferCenter.Domain.Entities;
using System.Reflection.Emit;

namespace MoneyTransferCenter.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        Guid userId;
        if (_currentUserService.UserId != null)
        {
            userId = _currentUserService.UserId.Value;
        }
        else
        {
            userId = Guid.Empty;
        }

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(p => p.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                entry.Property(p => p.CreatedBy).CurrentValue = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                bool isJustDeleted = entry.Property(p => p.IsDeleted).IsModified &&
                    entry.Property(p => p.IsDeleted).CurrentValue == true;

                if (isJustDeleted)
                {
                    entry.Property(p => p.DeletedAt).CurrentValue = DateTimeOffset.UtcNow;
                    entry.Property(p => p.DeletedBy).CurrentValue = userId;
                }
                else
                {
                    entry.Property(p => p.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                    entry.Property(p => p.UpdatedBy).CurrentValue = userId;
                }
            }
            if (entry.State == EntityState.Deleted)
            {
                // Silme işlemini güncellemeye çeviriyoruz
                entry.State = EntityState.Modified;
                entry.Property(p => p.IsDeleted).CurrentValue = true;
                entry.Property(p => p.DeletedAt).CurrentValue = DateTimeOffset.UtcNow;
                entry.Property(p => p.DeletedBy).CurrentValue = userId;
            }
        }


        return await base.SaveChangesAsync(cancellationToken);

    }


}
