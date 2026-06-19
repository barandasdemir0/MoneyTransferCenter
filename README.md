# Money Transfer Center
Money Transfer Center, finansal işlemleri (para yatırma, para transferi, para çekme) yönetmek amacıyla geliştirilmiş, yüksek ölçeklenebilirliğe ve modern yazılım mimarisi prensiplerine sahip kurumsal seviyede bir arka uç (backend) projesidir.
## 🏛 Mimari ve Tasarım Desenleri
Bu proje, kodun sürdürülebilirliğini ve test edilebilirliğini artırmak için endüstri standartlarındaki en iyi pratikler kullanılarak tasarlanmıştır:
- **Clean Architecture:** Uygulama; Domain, Application, Infrastructure ve WebAPI olmak üzere birbirine sıkı sıkıya bağlı olmayan (loosely coupled) katmanlara ayrılmıştır.
- **Domain-Driven Design (DDD):** İş kuralları servislerin içine değil, doğrudan Domain Entity'lerinin (örneğin `Account.Withdraw()`) içerisine yerleştirilerek zengin bir model (Rich Domain Model) oluşturulmuştur.
- **Repository & Unit of Work Pattern:** Veritabanı erişim mantığı soyutlanarak Repository sınıfları üzerinden yürütülmüş, veri tutarlılığını (transaction yönetimi) garanti altına almak ve değişiklikleri tek bir noktadan (atomik olarak) veritabanına yansıtmak için Unit of Work deseni entegre edilmiştir.
- **Outbox Pattern:** Veritabanı işlemleri ile loglama işlemleri birbirinden ayrılmıştır. İşlemler MSSQL'e kaydedilirken, oluşan olaylar (events) Outbox tablosuna yazılır ve bir Background Service aracılığıyla MongoDB'ye (Audit Logs) aktarılır. Bu sayede **Eventual Consistency** (Nihai Tutarlılık) sağlanmıştır.
- **Soft Delete Pattern:** EF Core üzerindeki `SaveChangesAsync` metodu ezilerek (override), silinen verilerin fiziksel olarak yok edilmesi engellenmiş ve `IsDeleted` bayrağı ile işaretlenmesi sağlanmıştır.
- **Dağıtık İzlenebilirlik (Distributed Tracing):** Özel Middleware'ler ve OpenTelemetry kullanılarak gelen HTTP istekleri, atılan veritabanı sorguları ve arkaplan işleri (background services) Jaeger üzerinden uçtan uca izlenebilir hale getirilmiştir.
## 🛠 Kullanılan Teknolojiler
- **Platform:** .NET 10 (C#)
- **Veritabanları:** Microsoft SQL Server (Primary DB), MongoDB (Audit Log DB)
- **ORM:** Entity Framework Core
- **Gözlemlenebilirlik (Observability):** OpenTelemetry, Jaeger
- **Konteynerleştirme (Containerization):** Docker, Docker Compose
- **Loglama:** Serilog
- **Dokümantasyon:** Scalar

## 🚀 Projeyi Ayağa Kaldırma (Başlatma)

Proje, tüm bağımlılıkları (Veritabanları ve Jaeger dahil) tek bir komutla ayağa kaldırılabilecek şekilde Docker Compose ile yapılandırılmıştır.

### Ön Koşullar
Bilgisayarınızda **Docker Desktop** (veya Docker CLI) kurulu ve çalışır durumda olmalıdır.

### Adım Adım Kurulum

1. Proje dizinini (Terminal/PowerShell veya Komut İstemcisi ile) açın.
2. Aşağıdaki komutu çalıştırarak tüm altyapıyı ayağa kaldırın:
   ```bash
   docker-compose up -d --build
