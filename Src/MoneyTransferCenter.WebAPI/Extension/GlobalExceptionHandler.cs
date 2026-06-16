using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace MoneyTransferCenter.WebAPI.Extension;

public class GlobalExceptionHandler : IExceptionHandler
{

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1) Hatayı Serilog'a logla (MongoDB SystemLogs'a yazılır)
        _logger.LogError(exception, "İşlenmeyen hata oluştu: {Message}", exception.Message);

        // 2) HTTP durum kodunu 500 (Sunucu Hatası) olarak ayarla
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            StatusCode = httpContext.Response.StatusCode,

            Message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyiniz."
        }, cancellationToken);


        return true;
    }


}
