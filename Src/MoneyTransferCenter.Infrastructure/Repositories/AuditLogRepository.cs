using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace MoneyTransferCenter.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly MongoDbContext _mongoDbContext;

    public AuditLogRepository(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _mongoDbContext.AuditLogs.InsertOneAsync(auditLog);
    }//Tek bir audit kaydı ekler.


    //birden fazla audit kaydını toplu olarak ekler. Eğer liste boşsa, hiçbir işlem yapmaz.
    public async Task AddManyAsync(IEnumerable<AuditLog> auditLogs)
    {

        List<AuditLog> list = auditLogs.ToList();
        if (list.Count > 0)
        {
            await _mongoDbContext.AuditLogs.InsertManyAsync(list);
        }
    }


    //Belirli bir varlık türü ve varlık kimliği için audit loglarını getirir. Sonuçlar, zaman damgasına göre azalan sırada sıralanır.
    public async Task<List<AuditLog>> GetByEntityAsync(string entityType, string entityId)
    {
        //bu alt satır ile filtreleme yapıyoruz. entityType ve entityId eşleşen kayıtları getiriyoruz.
        FilterDefinition<AuditLog> filter = Builders<AuditLog>.Filter.And(
            Builders<AuditLog>.Filter.Eq(log => log.EntityType, entityType),
            Builders<AuditLog>.Filter.Eq(log => log.EntityId, entityId)
        );
        return await _mongoDbContext.AuditLogs
            .Find(filter)
            .SortByDescending(log => log.Timestamp)
            .ToListAsync();

    }

    //Belirli bir kullanıcı kimliği için audit loglarını getirir. Sonuçlar, zaman damgasına göre azalan sırada sıralanır ve sayfalama yapılır.
    public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        return await _mongoDbContext.AuditLogs
           .Find(log => log.UserId == userId)
           .SortByDescending(log => log.Timestamp)
           .Skip((page - 1) * pageSize)
           .Limit(pageSize)
           .ToListAsync();
    }
}
