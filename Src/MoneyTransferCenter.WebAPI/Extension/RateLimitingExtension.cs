using Microsoft.Extensions.Caching.Memory;
using MoneyTransferCenter.Application.Telemetry;
using MoneyTransferCenter.Domain.Constants;
using System.Threading.RateLimiting;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class RateLimitingExtension
{
    public static IServiceCollection AddRateLimitConfig(this IServiceCollection services)
    {

        services.AddMemoryCache(); //memory cache ekliyoruz blaclist için
        services.AddRateLimiter(options =>
        {
            




            //limit aşıldığında dönecek kod
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


            options.AddPolicy(RateLimitPolicies.Standard, context =>
            {
                //isteği yapan ip adresini al
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter
                (
                    partitionKey:clientIp,
                    factory:partition => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 50, // bir ıp max 50 istek
                        Window = TimeSpan.FromMinutes(1), // 1 dk içerisinde
                        SegmentsPerWindow = 2, // 1 dk'yi 2 parçaya bölüyoruz, her parça 30 sn olacak
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // kuyrukta bekleyen istekler eski olan önce işlenecek
                        QueueLimit = 0 //limit aşılırsa 429 dönecek, kuyrukta bekletmeyecek
                    }
                );
            });

            options.AddPolicy(RateLimitPolicies.Strict, context =>
            {
                var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter
                (
                    partitionKey: clientIp,
                    factory: partition => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5, 
                        Window = TimeSpan.FromMinutes(1), // 1 dk içerisinde
                        SegmentsPerWindow = 2, // 1 dk'yi 2 parçaya bölüyoruz, her parça 30 sn olacak
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst, // kuyrukta bekleyen istekler eski olan önce işlenecek
                        QueueLimit = 0 //limit aşılırsa 429 dönecek, kuyrukta bekletmeyecek
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
