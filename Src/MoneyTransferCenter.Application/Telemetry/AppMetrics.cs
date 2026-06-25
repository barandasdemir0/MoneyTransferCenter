using System.Diagnostics.Metrics;

namespace MoneyTransferCenter.Application.Telemetry;

public static class AppMetrics
{
    private static readonly Meter Meter = new("MoneyTransferCenter.Metrics", "1.0.0");

    #region transfer metrikleri 

    // başarılı transfer sayısı
    public static readonly Counter<long> TransferCount =
       Meter.CreateCounter<long>("transfer.count", description: "Toplam başarılı transfer sayısı");

    //başarısız transfer sayısı
    public static readonly Counter<long> TransferFailedCount =
      Meter.CreateCounter<long>("transfer.failed.count", description: "Başarısız transfer sayısı");

    //toplam transfer edilen miktar

    public static readonly Counter<decimal> TransferAmountTotal =
       Meter.CreateCounter<decimal>("transfer.amount.total", "TRY", "Toplam transfer tutarı (TL)");


    #endregion

    #region para yükleme çekme metrikleri 

    // başarılı para yükleme sayısı (Deposit)

    public static readonly Counter<long> DepositCount =
      Meter.CreateCounter<long>("deposit.count", description: "Toplam başarılı para yükleme sayısı");

    // başarısız para yükleme sayısı

    public static readonly Counter<long> DepositFailedCount =
      Meter.CreateCounter<long>("deposit.failed.count", description: "Başarısız para yükleme sayısı");


    // başarılı para çekme sayısı (Withdraw)

    public static readonly Counter<long> WithdrawCount =
      Meter.CreateCounter<long>("withdraw.count", description: "Toplam başarılı para çekme sayısı");


    // başarısız para çekme sayısı

    public static readonly Counter<long> WithdrawFailedCount =
      Meter.CreateCounter<long>("withdraw.failed.count", description: "Başarısız para çekme sayısı");


    #endregion

    #region kimlik doğrulama metrikleri

    //başarılı giriş sayısı
    public static readonly Counter<long> LoginCount =
      Meter.CreateCounter<long>("login.count", description: "Toplam başarılı giriş sayısı");

    //başarısız giriş sayısı
    public static readonly Counter<long> LoginFailedCount =
        Meter.CreateCounter<long>("login.failed.count", description: "Başarısız giriş sayısı");

    //yeni kayıt sayısı
    public static readonly Counter<long> RegisterCount =
        Meter.CreateCounter<long>("register.count", description: "Toplam Başarılı kayıt sayısı");

    //başarısız yeni kayıt sayısı
    public static readonly Counter<long> RegisterFailedCount =
        Meter.CreateCounter<long>("register.failed.count", description: "Toplam Başarırısız kayıt sayısı");

    #endregion


    #region güvenlik metrikleri

    //rate limit'e takılan istek sayısı
    public static readonly Counter<long> RateLimitRejectedCount =
       Meter.CreateCounter<long>("ratelimit.rejected.count", description: "Rate limit'e takılan istek sayısı");

    //banlanan kullanıcı sayısı
    public static readonly Counter<long> IpBannedCount =
       Meter.CreateCounter<long>("ip.banned.count", description: "Banlanan IP sayısı");

    #endregion


    #region dış servis metrikleri tcmb 

    //tcmb fallback (cache) devreye girme sayısı
    public static readonly Counter<long> TcmbFallbackCount =
      Meter.CreateCounter<long>("tcmb.fallback.count", description: "TCMB fallback (cache) devreye girme sayısı");

    //tcmb circuit breaker açılma sayısı şalteri indirme
    public static readonly Counter<long> TcmbCircuitBreakerOpenCount =
       Meter.CreateCounter<long>("tcmb.circuit_breaker.open", description: "Circuit Breaker açılma sayısı");


    #endregion

    #region outbox metrikleri

    //işlenen outbox mesaj sayısı
    public static readonly Counter<long> OutboxProcessedCount =
       Meter.CreateCounter<long>("outbox.processed.count", description: "İşlenen outbox mesaj sayısı");

    //işlenemeyen outbox mesaj sayısı
    public static readonly Counter<long> OutboxFailedCount =
      Meter.CreateCounter<long>("outbox.failed.count", description: "Başarısız outbox mesaj sayısı");

    #endregion



}
