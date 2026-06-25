using Microsoft.AspNetCore.Diagnostics;
using MoneyTransferCenter.Domain.Exceptions;
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
        //Hatayı Serilog'a logla (MongoDB SystemLogs'a yazılır)
        _logger.LogError(exception, "İşlenmeyen hata oluştu: {Message}", exception.Message);


        //HTTP durum kodunu 500 (Sunucu Hatası) olarak ayarla
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        string errorCode = "INTERNAL_SERVER_ERROR";

        if (exception is DomainException domainEx)
        {
            statusCode = (int)HttpStatusCode.BadRequest; 
            message = domainEx.Message;                
            errorCode = domainEx.Code;             
        }
        else if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized; // 401 Yetkisiz
            message = "Bu işlem için yetkiniz bulunmamaktadır.";
        }


        else if (exception is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            statusCode = StatusCodes.Status409Conflict; // 409 Çakışma
            message = "Bu hesap üzerinde aynı anda başka bir işlem gerçekleştiği için güvenlik amacıyla işleminiz durduruldu. Lütfen tekrar deneyin.";
            errorCode = "CONCURRENCY_CONFLICT";
        }
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message 
        }, cancellationToken);



        return true;
    }


}
