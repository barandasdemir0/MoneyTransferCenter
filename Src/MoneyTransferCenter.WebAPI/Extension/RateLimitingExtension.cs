using Microsoft.Extensions.Caching.Memory;
using MoneyTransferCenter.Application.Telemetry;
using MoneyTransferCenter.Domain.Constants;
using MoneyTransferCenter.Domain.Entities;
using System.Threading.RateLimiting;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitConfig(this IServiceCollection services,IConfiguration configuration)
    {

        services.AddMemoryCache(); //memory cache ekliyoruz blaclist için
        services.AddRateLimiter(options =>
        {
            




            //limit aşıldığında dönecek kod
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


            options.AddPolicy(RateLimitPolicies.Standard, context =>
            {

                // Standard policy konfigürasyonunu yakala
                var standardSettings = configuration.GetSection("RateLimitSettings:Standard").Get<RateLimitPolicySettings>();

                //isteği yapan ip adresini al
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                //sliding kullanmamızın sebebi , 1 dk içerisinde 50 istek hakkı var, 1 dk dolmadan 30 sn sonra tekrar istek atarsa 1 dk dolmadan tekrar istek atabilir, sliding window mantığı ile çalışıyor
                return RateLimitPartition.GetSlidingWindowLimiter
                (
                    partitionKey:clientIp,
                    factory:partition => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = standardSettings!.PermitLimit, // bir ıp max 50 istek
                        Window = TimeSpan.FromMinutes(standardSettings.WindowMinutes), // 1 dk içerisinde
                        SegmentsPerWindow = standardSettings.SegmentsPerWindow, // 1 dk'yi 2 parçaya bölüyoruz, her parça 30 sn olacak
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // kuyrukta bekleyen istekler eski olan önce işlenecek
                        QueueLimit = standardSettings.QueueLimit //limit aşılırsa 429 dönecek, kuyrukta bekletmeyecek limit aşılmazsa kullanıcıyı bekletirken loading ekranı dönecek
                    }
                );
            });

            options.AddPolicy(RateLimitPolicies.Strict, context =>
            {

                // Strict policy konfigürasyonunu yakala
                var strictSettings = configuration.GetSection("RateLimitSettings:Strict").Get<RateLimitPolicySettings>();
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter
                (
                    partitionKey: clientIp,
                    factory: partition => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = strictSettings!.PermitLimit, 
                        Window = TimeSpan.FromMinutes(strictSettings.WindowMinutes), // 1 dk içerisinde
                        SegmentsPerWindow = strictSettings.SegmentsPerWindow, // 1 dk'yi 2 parçaya bölüyoruz, her parça 30 sn olacak
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // kuyrukta bekleyen istekler eski olan önce işlenecek
                        QueueLimit = strictSettings.QueueLimit //limit aşılırsa 429 dönecek, kuyrukta bekletmeyecek limit aşılmazsa kullanıcıyı bekletirken loading ekranı dönecek
                    }
                );
            });

            options.OnRejected = async (context, token) =>
            {

                // Dependency Injection üzerinden Logger'ı yakalıyoruz
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                // Dependency Injection üzerinden MemoryCache'i yakalıyoruz
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                //hangi endpoint'e istek atıldığını alıyoruz
                var path = context.HttpContext.Request.Path;

                //kaç kere saldırdı
                var strikeKey = $"StrikeCount_{clientIp}";
                //banlanacak ip
                var banKey = $"Ban_{clientIp}";


                var strikes = cache.GetOrCreate(strikeKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); // 1 saat boyunca strike sayısını tut
                    return 0;
                });

                strikes++;
                cache.Set(strikeKey, strikes);

                // Serilog ile logluyoruz (UYARI seviyesinde)
                logger.LogWarning("Rate Limit Aşıldı! IP: {ClientIp}, Yol: {Path}. Ceza Puanı: {Strikes}/10", clientIp, path, strikes);
                AppMetrics.RateLimitRejectedCount.Add(1);



                // Eğer 10 defa limite takılırsa GÜVENLİK İHLALİ de ve 24 saat BANLA!
                if (strikes >= 10)
                {
                    cache.Set(banKey, true, TimeSpan.FromHours(24));
                    logger.LogCritical("GÜVENLİK İHLALİ! Kötü niyetli IP Adresi 24 saat banlandı: {ClientIp}", clientIp);
                    AppMetrics.IpBannedCount.Add(1);

                }



                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
               

                context.HttpContext.Response.ContentType = "application/json";

                var message = new
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later."
                };
                await context.HttpContext.Response.WriteAsJsonAsync(message, cancellationToken: token);
            };
        });
        return services;
    }
}
