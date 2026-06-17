using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;

namespace MoneyTransferCenter.Infrastructure.Services;

public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. İşlenmemiş mesajları getir
        while (!stoppingToken.IsCancellationRequested)
        {
            // 2. Servis scope'u oluştur
            using var scope = scopeFactory.CreateScope();
            // 3. Repository ve UnitOfWork'leri al
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();

            // 4. AuditLog repository'sini al
            var auditRepo = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
            // 5. UnitOfWork'ü al
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            // 1. İşlenmemiş mesajları getir
            var pending = await outboxRepo.GetUnprocessedAsync();
            // 2. Mesajları sırayla MongoDB'ye at
            foreach (var msg in pending)
            {
                try
                {
                    await auditRepo.AddAsync(new AuditLog
                    {
                        Action = msg.Type,
                        EntityType = "Transaction",
                        EntityId = msg.Id.ToString(),
                        NewValue = msg.Payload,
                        Timestamp = DateTimeOffset.UtcNow
                    });

                    msg.MarkAsProcessed(); // EF Core değişikliği otomatik algılar
                }
                catch (Exception ex)
                {
                    msg.RecordFailure(ex.Message); // EF Core değişikliği otomatik algılar
                }
            }
            // 3. MSSQL'e değişiklikleri kaydet ve 10 saniye bekle
            if (pending.Any())
            {
                await unitOfWork.SaveChangesAsync(stoppingToken);
            }
            await Task.Delay(10000, stoppingToken);
        }
    }
}

