using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.WebAPI.Extension;
using Scalar.AspNetCore;
using Serilog;

// Serilog yapılandırması
LoggingExtension.AddConsoleLogger(); 

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddLogConfig();

    builder.Services.AddDatabaseConfig(builder.Configuration);
    builder.Services.AddIdentityConfig(builder.Configuration);
  
    builder.Services.AddApplicationServices(); 
    builder.Services.AddOpenApi();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

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