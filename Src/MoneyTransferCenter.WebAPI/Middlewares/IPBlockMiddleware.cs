using Microsoft.Extensions.Caching.Memory;

namespace MoneyTransferCenter.WebAPI.Middlewares;

public class IPBlockMiddleware
{
    private readonly RequestDelegate _next; 
    private readonly ILogger<IPBlockMiddleware> _logger;

    public IPBlockMiddleware(RequestDelegate next, ILogger<IPBlockMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context,IMemoryCache memoryCache)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var banKey = $"Ban_{clientIp}";
        // IP Cache'te yasaklı listesinde mi kontrol et
        if (memoryCache.TryGetValue(banKey, out bool isBanned) && isBanned)
        {
            _logger.LogWarning("Banlanmış IP sisteme erişmeye çalıştı ve engellendi: {ClientIp}", clientIp);

            // 403 Forbidden dön ve isteği burada kes (Controller'a asla gitmez)
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Güvenlik ihlali sebebiyle IP adresiniz 24 saat süreyle sistemden engellenmiştir." });
            return;
        }
        // Banlı değilse, yola devam et
        await _next(context);
    }

}
