using Microsoft.Extensions.Configuration;
using MoneyTransferCenter.Domain.Entities;
using MongoDB.Driver;

namespace MoneyTransferCenter.Infrastructure.MongoDB;

public class MongoDbContext
{
    private readonly IMongoDatabase _mongoDatabase;

    public MongoDbContext(IConfiguration configuration)
    {

        string connectionString = configuration.GetSection("MongoDB:ConnectionString").Value!;
        if (string.IsNullOrEmpty(connectionString) == true)
        {
            // Geliştirme ortamında hata almamak için varsayılan yerel MongoDB adresini atıyoruz.
            connectionString = "mongodb://localhost:27017";
        }
        // 2. Adım: Yapılandırma dosyasından hedef veritabanı adını (DatabaseName) okuyoruz.
        string databaseName = configuration.GetSection("MongoDB:DatabaseName").Value!;
        // Eğer yapılandırma dosyasında veritabanı ismi belirtilmediyse (boş veya null ise):
        if (string.IsNullOrEmpty(databaseName) == true)
        {
            // Varsayılan olarak projemizin ismini taşıyan veritabanını hedef alıyoruz.
            databaseName = "MoneyTransferCenterDb";
        }
        // 3. Adım: Elde ettiğimiz bağlantı adresiyle fiziksel bağlantıyı yönetecek olan MongoClient nesnesini oluşturuyoruz.
        MongoClient client = new MongoClient(connectionString);
        // 4. Adım: İstemci (client) üzerinden ilgili veritabanına bağlanıp, bağlantıyı sınıfımızın _database alanına kaydediyoruz.
        _mongoDatabase = client.GetDatabase(databaseName);
    }

    public IMongoCollection<AuditLog> AuditLogs
    {
        get
        {
            // _database bağlantısı üzerinden tipi AuditLog olan "AuditLogs" koleksiyonunu çekiyoruz.
            IMongoCollection<AuditLog> collection = _mongoDatabase.GetCollection<AuditLog>("AuditLogs");

            // Elde ettiğimiz koleksiyonu geri döndürüyoruz.
            return collection;
        }
    }
}
