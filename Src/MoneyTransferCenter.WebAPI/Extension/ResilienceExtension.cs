using Microsoft.Extensions.Http.Resilience;
using MoneyTransferCenter.Domain.Constants;
using Polly;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class ResilienceExtension
{
    public static IServiceCollection AddResilinceConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(HttpClientNames.TcmbApi, client =>
        {
            var baseUrl = configuration["ExternalServices:Tcmb:BaseUrl"]
                ?? "https://www.tcmb.gov.tr";

            // HttpClient için temel adresi ayarlama
            client.BaseAddress = new Uri(baseUrl);
            //İstek zaman aşımı
            client.Timeout = TimeSpan.FromSeconds(30);

            // TCMB'nin bizi bot sanmaması için User-Agent ekliyoruz
            client.DefaultRequestHeaders.Add("User-Agent", "MoneyTransferCenter/1.0");




        }) // Polly Resilience Pipeline'ını bu HttpClient'a bağlıyoruz
            .AddResilienceHandler("TcmbResiliencePipeline", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    // 3 kez yeniden deneme yapacak şekilde ayarlıyoruz
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2), // ilk denemeden sonra 2 saniye bekle
                    BackoffType = DelayBackoffType.Exponential, // katlanarak artan gecikme süresi 2sn 4 sn 8sn

                    UseJitter = true, // Jitter kullanarak yeniden deneme zamanlarını rastgeleleştiriyoruz
                   
                });

                // Tek bir istek 10 saniyeden uzun sürerse iptal et
                builder.AddTimeout(TimeSpan.FromSeconds(10));


                // Son 30 saniyede gelen isteklerin %50'sinden fazlası başarısızsa
                // ve en az 5 istek yapılmışsa → şalteri indir, 30 saniye boyunca hiç istek atma
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(30), //  Hata oranını ölçme penceresi: 30 saniye
                    FailureRatio = 0.5, // %50 hata oranı → şalter düşer
                    MinimumThroughput = 5, //  En az 5 istek yapılmadan şalter düşmez
                    BreakDuration = TimeSpan.FromSeconds(30), // Şalter düştükten sonra 30 saniye bekle

                   
                });

            });
        return services;
    }
}
