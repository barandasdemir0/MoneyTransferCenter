using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using System.Text.Json;

namespace MoneyTransferCenter.Infrastructure.Services;

public sealed class OutboxProcessor(IServiceScopeFactory scopeFactory,ILogger<OutboxProcessor> logger) : BackgroundService
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

            // SADECE bekleyen mesaj varsa log at! (Log kirliliğini önler)
            if (pending.Count > 0)
            {
                logger.LogInformation("OutboxProcessor çalıştı. Pending count: {Count}", pending.Count);
            }

            // 2. Mesajları sırayla MongoDB'ye at
            foreach (var msg in pending)
            {
                try
                {
                    string action = msg.Type;
                    string entityType = "Transaction";
                    string entityId = msg.Id.ToString();
                    string newValue = msg.Payload;

                    using JsonDocument doc = JsonDocument.Parse(msg.Payload);
                    JsonElement root = doc.RootElement;

                    if (msg.Type == "TransferCompleted")
                    {
                        action = "MoneyTransferred";
                        entityType = "Transaction";
                        entityId = root.GetProperty("TransactionId").GetString()!;

                        string referenceNumber = root.GetProperty("ReferenceNumber").GetString()!;
                        string senderIban = root.GetProperty("SenderIBAN").GetString()!;
                        string receiverIban = root.GetProperty("ReceiverIBAN").GetString()!;
                        decimal amount = root.GetProperty("Amount").GetDecimal();

                        newValue = $"Transfer başarılı. Referans: {referenceNumber}, Gönderen: {senderIban}, Alıcı: {receiverIban}, Tutar: {amount}";
                    }
                    else if (msg.Type == "DepositCompleted")
                    {
                        action = "MoneyDeposited";
                        entityType = "Account";
                        entityId = root.GetProperty("AccountId").GetString()!;

                        string iban = root.GetProperty("IBAN").GetString()!;
                        decimal amount = root.GetProperty("Amount").GetDecimal();
                        decimal newBalance = root.GetProperty("NewBalance").GetDecimal();

                        newValue = $"Para yatırıldı. IBAN: {iban}, Tutar: {amount}, Yeni bakiye: {newBalance}";
                    }
                    // YENİ EKLENEN PARA ÇEKME (WITHDRAW) OUTBOX EVENTİ
                    else if (msg.Type == "WithdrawCompleted")
                    {
                        action = "MoneyWithdrawn";
                        entityType = "Account";
                        entityId = root.GetProperty("AccountId").GetString()!;

                        string iban = root.GetProperty("IBAN").GetString()!;
                        decimal amount = root.GetProperty("Amount").GetDecimal();
                        decimal newBalance = root.GetProperty("NewBalance").GetDecimal();

                        newValue = $"Para çekildi. IBAN: {iban}, Tutar: {amount}, Yeni bakiye: {newBalance}";
                    }

                    await auditRepo.AddAsync(new AuditLog
                    {
                        Action = action,
                        EntityType = entityType,
                        EntityId = entityId,
                        NewValue = newValue,
                        Timestamp = DateTimeOffset.UtcNow
                    });

                    msg.MarkAsProcessed();
                }
                catch (Exception ex)
                {
                    msg.RecordFailure(ex.Message);
                    logger.LogError(ex, "Outbox işlenemedi. Id: {Id}", msg.Id);
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

