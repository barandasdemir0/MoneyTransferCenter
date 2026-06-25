using Serilog;
using Serilog.Sinks.Grafana.Loki;

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
            .MinimumLevel.Override("Microsoft.Extensions.Http.Resilience", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Error)
            .MinimumLevel.Override("Polly", Serilog.Events.LogEventLevel.Error)
            .WriteTo.Console()
            .WriteTo.File("Logs/SystemLog-.txt", rollingInterval: RollingInterval.Day)// logları günlük olarak döndüren bir dosya hedefi ekler, böylece loglar "Logs" klasöründe "SystemLog-2024-06-01.txt" gibi dosyalara yazılır.
            .WriteTo.MongoDB(databaseUrl:mongoFullUrl,
            collectionName:"SystemLogs")
             .WriteTo.GrafanaLoki("http://loki:3100")
            .Enrich.FromLogContext() // loglara context bilgisi ekler, böylece logların hangi istek veya işlemle ilişkili olduğunu görebiliriz.
            .CreateLogger();
        applicationBuilder.Host.UseSerilog();
        return applicationBuilder;
    }
}