<div align="center">

# 🏦 Money Transfer Center

### Kurumsal Seviyede Finansal İşlem Altyapısı

**Race Condition Koruması · Composite Index Optimizasyonu · Tam Gözlemlenebilirlik (Full Observability)**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-Audit_Log-47A248?logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![Grafana](https://img.shields.io/badge/Grafana-Loki_·_Prometheus-F46800?logo=grafana&logoColor=white)](https://grafana.com/)
[![Jaeger](https://img.shields.io/badge/Jaeger-Distributed_Tracing-66CFE3?logo=jaeger&logoColor=white)](https://www.jaegertracing.io/)

</div>

---

## 📖 Proje Hakkında

**Money Transfer Center**, para yatırma, çekme ve hesaplar arası transfer işlemlerini yöneten, **kurumsal (enterprise) seviyede** .NET backend projesidir. Sistem;

- Aynı anda gelen isteklerde paranın asla eksiye düşmemesini garanti eden **Concurrency kilitleri**,
- Milyonlarca kayıt içinde sorguları milisaniyeler içinde sonuçlandıran **B-Tree Composite Indexleri**,
- Sistemi gerçek zamanlı izleyen **Grafana + Loki + Prometheus + Jaeger** gözlemlenebilirlik kulesi

ile donatılmıştır.

---

## 🏗️ Mimari: Clean Architecture & Katman Yapısı

Proje, bağımlılıkları sıkı bir şekilde yöneten **Clean Architecture** prensibine göre tasarlanmıştır. Her katman yalnızca bir iç katmanı tanır ve dışa olan bağımlılık yoktur.

```mermaid
graph TD
    subgraph Solution ["💼 MoneyTransferCenter Solution"]
        subgraph Tests ["🧪 Tests"]
            UT["UnitTests\n─────────────\nAccountTests\nTransactionTests\nAccountServiceTests\nAuthServiceTests\nTransactionServiceTests"]
            IT["IntegrationTests\n─────────────\nAccountRepositoryTests"]
        end

        subgraph WebAPI ["🌐 WebAPI Katmanı (En Dış)"]
            Controllers["Controllers\n────────────\nAccountController\nAuthController\nTransactionController\nExchangeRateController"]
            Ext["Extensions\n────────────\nRateLimitingExtension\nResilienceExtension\nOpenTelemetryExtension\nLoggingExtension\nGlobalExceptionHandler"]
            MW["Middlewares\n────────────\nIPBlockMiddleware\nReqAndResActivityBodyMiddleware"]
        end

        subgraph App ["⚙️ Application Katmanı"]
            Services["Services\n────────────\nAccountService\nTransactionService\nAuthService\nAuditService"]
            Validators["Validators (FluentValidation)\n────────────\nAccount / Auth / Transaction"]
            DTOs["DTOs\n────────────\nRequest & Response Modelleri"]
            Telemetry["AppMetrics\n(OpenTelemetry Counters)"]
        end

        subgraph Infra ["🔧 Infrastructure Katmanı"]
            Repos["Repositories\n────────────\nGenericRepository\nAccountRepository\nTransactionRepository\nOutboxMessageRepository\nAuditLogRepository"]
            InfraServices["Services\n────────────\nAccountLockService\nOutboxProcessor\nTcmbExchangeRateService\nIbanGenerator\nTokenService"]
            DB["Data\n────────────\nAppDbContext\nEF Core Configurations\nMigrations"]
            Mongo["MongoDB\n────────────\nMongoDbContext\nAuditLog Deposu"]
        end

        subgraph Domain ["🏛️ Domain Katmanı (En İç)"]
            Entities["Entities\n────────────\nAccount\nTransaction\nAppUser\nAuditLog\nOutboxMessage"]
            Interfaces["Interfaces\n────────────\nIUnitOfWork\nIAccountRepository\nITransactionRepository\nIOutboxMessageRepository"]
            Enums["Enums (Strongly Typed)\n────────────\nAccountStatus\nTransactionStatus"]
            Ex["Exceptions\n────────────\nDomainException"]
        end
    end

    WebAPI --> App
    App --> Domain
    Infra --> Domain
    Tests --> App
    Tests --> Domain
    Tests --> Infra
```

---

## 🎯 Tasarım Desenleri (Design Patterns)

| Pattern | Nerede Kullanıldı? | Ne İşe Yarıyor? |
| :--- | :--- | :--- |
| **Repository Pattern** | `GenericRepository<T>` | Veritabanı erişimini servislerden soyutlar |
| **Unit of Work** | `UnitOfWork.cs` | Birden fazla repository değişikliğini tek atomik işlemde taahhüt eder |
| **Outbox Pattern** | `OutboxMessage` + `OutboxProcessor` | SQL işlemi ile MongoDB audit logu arasında *Eventual Consistency* garanti eder |
| **Domain-Driven Design** | `Account.cs`, `Transaction.cs` | İş kuralları (withdraw, deposit) doğrudan Entity içinde, *Rich Domain Model* |
| **Strongly Typed Enums** | `AccountStatus`, `TransactionStatus` | Veritabanına `int`, koda `string` olarak akıllı dönüşüm |
| **Soft Delete** | `BaseEntity.IsDeleted` | Veriler fiziksel olarak silinmez, `IsDeleted = true` ile işaretlenir |
| **Factory Method** | `Transaction.Create()`, `Account.Create()` | Geçersiz nesne oluşumunu domain seviyesinde engeller |
| **Global Exception Handler** | `GlobalExceptionHandler.cs` | Tüm hatalar tek noktadan yakalanır, müşteriye güvenli mesaj döner |

---

## 🔐 Güvenlik & Performans Özellikleri

### Rate Limiting (Hız Sınırlama) — `RateLimitingExtension.cs`
Sistemi aşırı yüklenme ve DDoS saldırılarından koruyan **iki katmanlı** rate limiting mimarisi:

| Politika | Limit | Pencere | Kullanıldığı Endpoint |
| :--- | :--- | :--- | :--- |
| `Standard` | 50 istek | 1 dakika | Genel API endpointleri |
| `Strict` | 5 istek | 1 dakika | Para transferi gibi kritik işlemler |

**Akıllı IP Ban Mekanizması:** Rate limit'i 10 kez aşan bir IP adresi otomatik olarak **24 saat** yasaklanır ve bu olay `Critical` seviyesinde loglanır.

### Polly Resilience (Dayanıklılık) — `ResilienceExtension.cs`
Dış servis (TCMB Döviz API) çağrılarındaki geçici hataları otomatik olarak iyileştiren **üç katmanlı** dayanıklılık pipeline'ı:

- **Retry:** 3 deneme, katlanarak artan süre (2s → 4s → 8s) + Jitter
- **Timeout:** Tek istek 10 saniyeyi geçerse iptal
- **Circuit Breaker:** Son 30 saniyede %50 hata oranı → 30 saniye boyunca şalter indirilir

### IP Blacklist — `IPBlockMiddleware.cs`
Ban yiyen IP'lerin gelen her isteği doğrudan `403 Forbidden` ile reddedilir. İstek uygulama koduna bile ulaşamaz.

### Concurrency (Eşzamanlılık) Koruması — `AccountLockService.cs`
Aynı hesaba aynı anda gelen 10 farklı "100 TL çek" isteğinde paranın eksiye düşmesini (Race Condition) engelleyen **çift katmanlı** kilit mimarisi:

1. **`SemaphoreSlim(1,1)`:** Aynı `AccountId` için sadece 1 istek eş zamanlı çalışır, diğerleri kuyrukta bekler.
2. **EF Core `RowVersion` (Optimistic Lock):** İlk katmanı aşan senaryolarda (çoklu sunucu) veritabanı seviyesinde `HTTP 409 Conflict` döndürür.

---

## 🔭 Gözlemlenebilirlik Kulesi (Observability Stack)

Sistemin her milisaniyesi 4 farklı araç tarafından izlenmektedir:

```mermaid
graph LR
    API[Web API]

    API -- "Serilog Sink\n(Yapısal Loglar)" --> Loki[(Grafana Loki)]
    API -- "OpenTelemetry\n(Request Trace)" --> Jaeger[(Jaeger)]
    API -- "/metrics endpoint\n(Counter, Gauge)" --> Prom[(Prometheus)]

    Loki --> Grafana[📊 Grafana Dashboard]
    Jaeger --> Grafana
    Prom --> Grafana
```

| Araç | Görevi | Adres |
| :--- | :--- | :--- |
| **Prometheus** | Her 15 saniyede `/metrics` endpoint'ini sorgulayarak ölçümleri toplar | `localhost:9090` |
| **Grafana Loki** | Serilog tarafından HTTP üzerinden gönderilen yapısal logları depolar | `localhost:3100` |
| **Jaeger** | OpenTelemetry ile her HTTP isteğini uçtan uca izler (Distributed Tracing) | `localhost:16686` |
| **Grafana** | Tüm kaynakları tek ekranda gösterir; Log, Metric ve Trace aynı anda | `localhost:3000` |

**Özel Metrikler (`AppMetrics.cs`):**
- `rate_limit_rejected_total` — Rate limit'e takılan toplam istek sayısı
- `ip_banned_total` — Banlanan IP sayısı
- `outbox_processed_total` — Başarıyla işlenen outbox mesajları
- `outbox_failed_total` — Hatalı outbox mesajları

---

## 🗄️ Veri Katmanı

### Veritabanı Şeması (SQL Server)
```mermaid
erDiagram
    AppUsers {
        Guid Id PK
        string UserName
        string Email
    }
    Accounts {
        Guid Id PK
        Guid UserId FK
        string IBAN
        decimal Balance
        byte[] RowVersion
    }
    Transactions {
        Guid Id PK
        Guid SenderAccountId FK
        Guid ReceiverAccountId FK
        decimal Amount
        string Status
        string ReferenceNumber
    }
    OutboxMessages {
        Guid Id PK
        string Type
        string Payload
        bool IsProcessed
    }

    AppUsers ||--o{ Accounts : "sahip olur"
    Accounts ||--o{ Transactions : "gönderir"
    Accounts ||--o{ Transactions : "alır"
```

### Performans: B-Tree Composite Index Optimizasyonu
İşlem geçmişi sorgularında (filtreleme + tarihe göre sıralama) pahalı SQL `Sort` operasyonlarını ortadan kaldırmak için bileşik indexler kullanılmıştır:

```
IX_Transactions_SenderAccountId_CreatedAt   → "Gönderdiğim işlemler" sorgusunu 100x hızlandırır
IX_Transactions_ReceiverAccountId_CreatedAt → "Aldığım işlemler" sorgusunu 100x hızlandırır
```

---

## 🧪 Test Altyapısı

Proje hem domain iş kurallarını hem uygulama servislerini hem de altyapıyı doğrulayan kapsamlı bir test suite'ine sahiptir:

```
Tests/
├── MoneyTransferCenter.UnitTests/
│   ├── Domain/
│   │   ├── AccountTests.cs         → Hesap oluşturma, para yatırma/çekme domain kuralları
│   │   └── TransactionTests.cs     → Transfer iş kuralları (negatif tutar, öz transfer vb.)
│   └── Application/
│       ├── AccountServiceTests.cs  → Hesap servis iş akışları (mock ile)
│       ├── AuthServiceTests.cs     → Kimlik doğrulama senaryoları
│       └── TransactionServiceTests.cs → Concurrency ve transfer senaryoları
│
└── MoneyTransferCenter.IntegrationTests/
    └── Infrastructure/
        └── AccountRepositoryTests.cs → Gerçek veritabanı ile CRUD doğrulaması
```

---

## 🚀 Kurulum ve Çalıştırma

**Tek ön koşul:** Bilgisayarınızda **Docker Desktop** kurulu ve çalışıyor olmalıdır.

```bash
# 1. Projeyi klonlayın
git clone <repo-url>

# 2. Proje dizinine gidin
cd MoneyTransferCenter

# 3. Tüm sistemi tek komutla ayağa kaldırın
docker compose up -d --build
```

Sistem başlatıldıktan sonra tüm servisler hazırdır:

| Servis | URL |
| :--- | :--- |
| **API Dokümantasyonu (Scalar)** | `http://localhost:8080/scalar/v1` |
| **Grafana** (admin / admin) | `http://localhost:3000` |
| **Jaeger UI** | `http://localhost:16686` |
| **Prometheus** | `http://localhost:9090` |

> **Not:** Uygulama başlarken `context.Database.Migrate()` ile tüm migration'lar otomatik uygulanır. Manuel `Update-Database` gerekmez.

---

## 📦 Teknoloji Yığını (Tech Stack)

| Kategori | Teknoloji |
| :--- | :--- |
| **Platform** | .NET 10, C# 13 |
| **Web Framework** | ASP.NET Core 10 |
| **ORM** | Entity Framework Core 10 |
| **Primary DB** | Microsoft SQL Server 2022 |
| **Audit / Log DB** | MongoDB |
| **Kimlik Doğrulama** | JWT Bearer Token |
| **Validasyon** | FluentValidation |
| **Loglama** | Serilog (Console + File + MongoDB + Loki sinks) |
| **Dayanıklılık** | Polly (Retry, Timeout, Circuit Breaker) |
| **Tracing** | OpenTelemetry + Jaeger |
| **Metrikler** | Prometheus + Grafana |
| **Merkezi Log** | Grafana Loki |
| **Dokümantasyon** | Scalar (OpenAPI) |
| **Test** | xUnit, Moq, FluentAssertions |
| **Konteyner** | Docker, Docker Compose |
