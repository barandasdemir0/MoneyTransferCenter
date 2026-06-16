using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.WebAPI.Extension;
using Scalar.AspNetCore;
using Serilog;

LoggingExtension.AddConsoleLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddLogConfig();

    builder.Services.AddDatabaseConfig(builder.Configuration);
    builder.Services.AddOpenApi();
    builder.Services.AddApplicationServices();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    app.UseExceptionHandler();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseSerilogRequestLogging();

    app.MapGet("/", () => "Hello World!");

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik hata oluştu!");
}
finally
{
    Log.CloseAndFlush();
}