using MoneyTransferCenter.Domain.Entities;

namespace MoneyTransferCenter.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task AddManyAsync(IEnumerable<AuditLog> auditLogs);
    Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId);
}
