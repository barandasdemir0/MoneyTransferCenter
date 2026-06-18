using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces.Repositories;

namespace MoneyTransferCenter.Application.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;
    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogAccountActivatedAsync(Guid userId)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "AccountActivated",
            EntityType = "Account",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = "Hesap aktifleştirildi."
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog: {Action}, UserId: {UserId}", log.Action, userId);
    }

    public async Task LogAccountClosedAsync(Guid userId)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "AccountClosed",
            EntityType = "Account",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = "Active/Passive",
            NewValue = "Hesap başarıyla kapatıldı."
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog: {Action}, UserId: {UserId}", log.Action, userId);
    }

    public async Task LogAccountCreatedAsync(Guid userId, string iban)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "AccountCreated",
            EntityType = "Account",
            EntityId = iban,
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Hesap oluşturuldu. IBAN: {iban}"
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog: {Action}, UserId: {UserId}, IBAN: {IBAN}", log.Action, userId, iban);
    }

    public async Task LogLoginFailedAsync(AppUser user, string reason)
    {
        AuditLog log = new AuditLog
        {
            UserId = user.Id,
            Action = "LoginFailed",
            EntityType = "AppUser",
            EntityId = user.Id.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = reason
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogWarning("AuditLog yazıldı: {Action}, UserId: {UserId}, Sebep: {Reason}", log.Action, user.Id, reason);
    }

    public async Task LogMoneyDepositedAsync(Guid userId, Guid accountId, string iban, decimal amount, decimal newBalance)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "MoneyDeposited",
            EntityType = "Account",
            EntityId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Para yatırıldı. IBAN: {iban}, Tutar: {amount}, Yeni bakiye: {newBalance}"
        };

        await _auditLogRepository.AddAsync(log);

        _logger.LogInformation(
            "AuditLog: {Action}, UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            log.Action,
            userId,
            accountId,
            amount);
    }

    public async Task LogMoneyDepositFailedAsync(Guid userId, decimal amount, string reason)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "MoneyDepositFailed",
            EntityType = "Account",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Para yatırma başarısız. Tutar: {amount}, Sebep: {reason}"
        };

        await _auditLogRepository.AddAsync(log);
    }

    public async Task LogMoneyTransferFailedAsync(Guid userId, string? senderIban, string receiverIban, decimal amount, string reason)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "MoneyTransferFailed",
            EntityType = "Transaction",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Transfer başarısız. Gönderen: {senderIban ?? "Bilinmiyor"}, Alıcı: {receiverIban}, Tutar: {amount}, Sebep: {reason}"
        };

        await _auditLogRepository.AddAsync(log);
    }

    public async Task LogMoneyTransferredAsync(Guid senderUserId, Guid senderAccountId, string senderIban, string receiverIban, decimal amount)
    {
        AuditLog log = new AuditLog
        {
            UserId = senderUserId,
            Action = "MoneyTransferred",
            EntityType = "Transaction",
            EntityId = senderAccountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Gönderen: {senderIban}, Alıcı: {receiverIban}, Tutar: {amount}"
        };

        await _auditLogRepository.AddAsync(log);

        _logger.LogInformation(
            "AuditLog yazıldı: {Action}, UserId: {UserId}",
            log.Action,
            senderUserId);
    }

    public async Task LogMoneyWithdrawFailedAsync(Guid userId, decimal amount, string reason)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "MoneyWithdrawFailed",
            EntityType = "Account",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Para çekme başarısız. Tutar: {amount}, Sebep: {reason}"
        };
        await _auditLogRepository.AddAsync(log);
    }

    public async Task LogMoneyWithdrawnAsync(Guid userId, Guid accountId, string iban, decimal amount, decimal newBalance)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "MoneyWithdrawn",
            EntityType = "Account",
            EntityId = accountId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Para çekildi. IBAN: {iban}, Tutar: {amount}, Yeni bakiye: {newBalance}"
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation(
            "AuditLog: {Action}, UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}",
            log.Action,
            userId,
            accountId,
            amount);
    }

    public async Task LogProfileCompletedAsync(Guid userId)
    {
        AuditLog log = new AuditLog
        {
            UserId = userId,
            Action = "ProfileCompleted",
            EntityType = "AppUser",
            EntityId = userId.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = "Profil bilgileri tamamlandı."
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog: {Action}, UserId: {UserId}", log.Action, userId);
    }

    public async Task LogUserLoggedInAsync(AppUser user)
    {
        AuditLog log = new AuditLog
        {
            UserId = user.Id,
            Action = "UserLoggedIn",
            EntityType = "AppUser",
            EntityId = user.Id.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Başarılı giriş. E-posta: {user.Email}"
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog yazıldı: {Action}, UserId: {UserId}", log.Action, user.Id);
    }

    public async Task LogUserRegisteredAsync(AppUser user)
    {
        AuditLog log = new AuditLog
        {
            UserId = user.Id,
            Action = "UserRegistered",
            EntityType = "AppUser",
            EntityId = user.Id.ToString(),
            Timestamp = DateTimeOffset.UtcNow,
            OldValue = null,
            NewValue = $"Ad: {user.FullName}, E-posta: {user.Email}"
        };
        await _auditLogRepository.AddAsync(log);
        _logger.LogInformation("AuditLog yazıldı: {Action}, UserId: {UserId}", log.Action, user.Id);
    }


   
}
