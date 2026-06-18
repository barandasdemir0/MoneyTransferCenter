using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class OpenTelemetryExtension
{
    public static IServiceCollection AddJaegerOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var jaegerEndpoint = configuration["Jaeger:Endpoint"] ?? "http://localhost:4317";

       

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("MoneyTransferCenter.API"))
            .WithTracing(tracing =>
            {
                tracing
                    // API İstekleri
                    .AddAspNetCoreInstrumentation()

                    //  HTTP Çağrıları 
                    .AddHttpClientInstrumentation()

                     // EF Core İzlemesi
                     .AddEntityFrameworkCoreInstrumentation(options =>
                     {
                         options.Filter = (provider, command) => true;

                         options.EnrichWithIDbCommand = (activity, command) =>
                         {
                             activity.SetTag("db.statement", command.CommandText);
                         };
                     })
                    .AddSqlClientInstrumentation()
                    // MONGODB İzlemesi
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")

                    // JAEGER'A GÖNDERİM AYARI (OTLP Protokolü ile)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(jaegerEndpoint);
                    });
            });



        return services;
    }
}
