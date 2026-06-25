<div align="center">

# 🏦 Money Transfer Center (Enterprise Backend)

**Yüksek Erişilebilirlik (High Availability), Dağıtık İzlenebilirlik (Distributed Tracing) ve İleri Düzey Eşzamanlılık (Concurrency) Yönetimine Sahip Yeni Nesil Finansal İşlem Altyapısı.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-Primary_DB-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-Audit_Logs-47A248?logo=mongodb)](https://www.mongodb.com/)
[![Grafana](https://img.shields.io/badge/Grafana-Observability-F46800?logo=grafana)](https://grafana.com/)

</div>

---

## 🚀 Proje Hakkında

**Money Transfer Center**, para yatırma, çekme ve transfer işlemlerini yöneten, kurumsal (enterprise) seviyede bir .NET mikro-mimari projesidir. Sistem, finansal verilerin tutarlılığını garanti altına almak ve olası veri yarışlarını (Race Conditions) engellemek amacıyla **Concurrency Kilitleri** ve **Optimistic Locking** ile donatılmıştır. 

Ayrıca sistemin her bir milisaniyesi, **Grafana, Prometheus, Loki ve Jaeger**'dan oluşan tam teşekküllü bir Observability (Gözlemlenebilirlik) kulesi üzerinden anlık olarak izlenmektedir.

---

## 🏗️ Sistem Mimarisi & Veri Akışı

Projenin genel altyapısı, veritabanı iletişimi ve gözlemlenebilirlik araçlarının birbirleriyle olan etkileşimi aşağıdaki mimari şemada gösterilmiştir:

```mermaid
graph TD
    %% Tanımlamalar
    Client([İstemci / Scalar / Postman])
    API[Web API Katmanı]
    
    subgraph Veri_Katmani [Veri Katmani]
        SQL[(SQL Server <br/> Transaction DB)]
        Processor[[Outbox Processor <br/> Background Service]]
        Mongo[(MongoDB <br/> Audit Logs)]
    end

    subgraph Observability_Kulesi [Observability Kulesi]
        Loki[(Grafana Loki <br/> Centralized Logs)]
        Prom[(Prometheus <br/> Metrics Scraper)]
        Jaeger[(Jaeger <br/> Distributed Tracing)]
        Grafana[Grafana Dashboard]
    end

    %% İlişkiler
    Client == HTTP İstekleri ==> API
    API == Güvenli İşlem & Composite Index ==> SQL
    SQL -- Eventual Consistency --> Processor
    Processor == Log Kaydı ==> Mongo
    
    %% Gözlemlenebilirlik İlişkileri
    API -. Serilog Sink .-> Loki
    API -. /metrics Ping .-> Prom
    API -. OpenTelemetry .-> Jaeger
    
    Loki --> Grafana
    Prom --> Grafana
    Jaeger --> Grafana

    %% Stil
    style Client fill:#2d3436,stroke:#dfe6e9,color:#fff
    style API fill:#0984e3,stroke:#74b9ff,color:#fff
    style SQL fill:#d63031,stroke:#ff7675,color:#fff
    style Mongo fill:#00b894,stroke:#55efc4,color:#fff
    style Grafana fill:#e17055,stroke:#fab1a0,color:#fff
```

---

## 💎 Temel Mimari Prensipler (Architecture & Design Patterns)

Projede, sürdürülebilirliği maksimumda tutmak için endüstri standardı pattern'ler kullanılmıştır:

* **Clean Architecture:** Domain, Application, Infrastructure ve WebAPI katmanlarıyla "Loosely Coupled" (Gevşek Bağlı) bir yapı.
* **Domain-Driven Design (DDD):** İş kurallarının servislere saçılması yerine doğrudan `Entity` içine gömüldüğü *Rich Domain Model*.
* **Outbox Pattern:** İşlemlerin MSSQL'e yazılmasıyla MongoDB'ye log atılması arasındaki kopukluğu önleyen *Eventual Consistency* garantisi.
* **Concurrency Management (Eşzamanlılık):** Aynı anda gelen transfer isteklerinde paranın eksiye düşmesini (Race Condition) engelleyen `SemaphoreSlim` kilit mimarisi ve EF Core `RowVersion` (Optimistic Locking).
* **B-Tree Composite Indexing:** Milyonlarca satır veride SQL Server'ın yavaş Sort operasyonlarını engelleyen, Tarih ve ID bazlı *Bileşik Index* optimizasyonları.
* **Resilience & Fault Tolerance:** Hatalı API çağrılarında sistemin çökmesini engelleyen **Polly** (Retry, Circuit Breaker) entegrasyonu.

---

## 🛠️ Kullanılan Teknolojiler (Tech Stack)

| Kategori | Teknoloji / Araç |
| :--- | :--- |
| **Platform** | .NET 10 (C#) |
| **Veritabanları** | Microsoft SQL Server (Primary DB), MongoDB (Audit Log DB) |
| **ORM & Veri Erişimi** | Entity Framework Core, Repository & Unit of Work Pattern |
| **Gözlemlenebilirlik (Observability)** | OpenTelemetry, Prometheus, Jaeger, Grafana, Loki |
| **Loglama** | Serilog (Structured Logging with MongoDB & Loki Sinks) |
| **Güvenlik & Performans** | Rate Limiting, IP Blocking Middleware, Global Exception Handler |
| **Konteynerleştirme** | Docker, Docker Compose |

---

## 🚦 Kurulum ve Çalıştırma (Quick Start)

Mükemmel bir **Developer Experience (DX)** sunmak adına, tüm veritabanları, arka plan servisleri ve izleme araçları tek bir komutla ayağa kalkacak şekilde Dockerize edilmiştir.

### Ön Koşullar
* Bilgisayarınızda **Docker Desktop** kurulu ve çalışır durumda olmalıdır.

### Adım Adım Başlatma
1. Proje dizininde (Powershell / Terminal) komut satırını açın.
2. Sadece aşağıdaki komutu çalıştırın:
   ```bash
   docker compose up -d --build
   ```
3. Konteynerler ayağa kalktıktan sonra aşağıdaki bağlantıları kullanarak sistemi inceleyebilirsiniz:

| Servis | Bağlantı (URL) |
| :--- | :--- |
| **API & Scalar Dokümantasyonu** | `http://localhost:8080/scalar/v1` veya `/Scalar` |
| **Grafana (Metrikler & Loglar)** | `http://localhost:3000` *(Kullanıcı: admin, Şifre: admin)* |
| **Jaeger (Trace & Request İzleme)** | `http://localhost:16686` |

---

## 🛡️ Hata Yönetimi & Güvenlik
Proje, hataları kullanıcıya şifreleyerek (`HTTP 409 Conflict`, `HTTP 429 Too Many Requests` vb.) dönerken, arka planda tüm kritik detayları (Stack Trace, Hata satırı) **Loki** ve **MongoDB**'ye detaylı olarak yazar. Müşteri asla teknik detay görmez, sistem yöneticisi ise asla kör kalmaz.
