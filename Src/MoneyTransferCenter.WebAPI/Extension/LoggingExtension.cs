using Serilog;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class LoggingExtension
{
    public static void AddConsoleLogger()
    {
        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Information()
           .WriteTo.Console()
           .CreateLogger();
    }

    public static WebApplicationBuilder AddLogConfig(this WebApplicationBuilder applicationBuilder)
    {
        //// sadece alttaki iki satır eklenerek MongoDB bağlantısı sağlanır ve loglar MongoDB'ye yazılır.
        string mongoConnectionString = applicationBuilder.Configuration["MongoDB:ConnectionString"]!;
        string mongoDatabaseName = applicationBuilder.Configuration["MongoDB:DatabaseName"]!;
        string mongoFullUrl = $"{mongoConnectionString}/{mongoDatabaseName}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)// Microsoft'un log seviyesini Warning olarak ayarlıyoruz, böylece sadece Warning ve üzeri seviyedeki loglar yazılır.
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File("Logs/SystemLog-.txt", rollingInterval: RollingInterval.Day)// logları günlük olarak döndüren bir dosya hedefi ekler, böylece loglar "Logs" klasöründe "SystemLog-2024-06-01.txt" gibi dosyalara yazılır.
            .WriteTo.MongoDB(databaseUrl:mongoFullUrl,
            collectionName:"SystemLogs")
            .Enrich.FromLogContext() // loglara context bilgisi ekler, böylece logların hangi istek veya işlemle ilişkili olduğunu görebiliriz.
            .CreateLogger();
        applicationBuilder.Host.UseSerilog();
        return applicationBuilder;
    }
}

