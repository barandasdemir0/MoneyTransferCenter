using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MoneyTransferCenter.Application.Dtos.ExchangeRate;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Constants;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MoneyTransferCenter.Infrastructure.Services;

public sealed class TcmbExchangeRateService : IExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TcmbExchangeRateService> _logger;


    // son bilinen döviz kurlarını önbelleğe almak için kullanılacak anahtar
    private const string CacheKey = "TcmbExchangeRates_LastKnown";
    public TcmbExchangeRateService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<TcmbExchangeRateService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ExchangeRateDto>> GetCurrentRatesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.TcmbApi);
            _logger.LogInformation("TCMB'den güncel döviz kurları çekiliyor...");
            var xmlContent = await client.GetStringAsync("/kurlar/today.xml");
            var rates = ParseTcmbXml(xmlContent);
            //    1 saat boyunca cache'te tut
            _cache.Set(CacheKey, rates, TimeSpan.FromHours(1));

            _logger.LogInformation("TCMB kurları başarıyla çekildi. {Count} adet döviz bulundu.", rates.Count);
            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TCMB servisine ulaşılamadı. Fallback devreye giriyor...");

            // Veriyi doğrudan Cache'den çek
            var cachedRates = _cache.Get<List<ExchangeRateDto>>(CacheKey);
            // Eğer null değilse
            if (cachedRates != null)
            {
                _logger.LogInformation("Fallback başarılı! Cache'teki son kurlar döndürülüyor.");
                return cachedRates;
            }
            // Cache'te de veri yoksa
            _logger.LogError("TCMB servisine ulaşılamadı ve cache'te yedek veri bulunamadı!");
            throw new Exception("Döviz kurları şu anda alınamıyor. Lütfen daha sonra tekrar deneyin.");
        }
    }

    private static List<ExchangeRateDto> ParseTcmbXml(string xmlContent)
    {
        var rates = new List<ExchangeRateDto>();
        // bizim yerimize XML'i objeye çevirsin
        var serializer = new XmlSerializer(typeof(TcmbKurlar));
        using var reader = new StringReader(xmlContent);
        var tcmbData = (TcmbKurlar)serializer.Deserialize(reader)!;
        // objenin içinde dönüp kendi DTO'muza atalım
        foreach (var currency in tcmbData.Currencies)
        {
            // Sadece USD, EUR ve GBP
            if (currency.Kod == "USD" || currency.Kod == "EUR" || currency.Kod == "GBP")
            {
                // Fiyatları ondalıklı sayıya çevir
                var buying = decimal.Parse(currency.ForexBuying, CultureInfo.InvariantCulture);
                var selling = decimal.Parse(currency.ForexSelling, CultureInfo.InvariantCulture);
                // Listeye ekle
                rates.Add(new ExchangeRateDto(currency.Kod, currency.Isim, buying, selling));
            }
        }
        return rates;
    }

}
