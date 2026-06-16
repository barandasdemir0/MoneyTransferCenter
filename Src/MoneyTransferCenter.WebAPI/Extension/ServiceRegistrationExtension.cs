using Microsoft.EntityFrameworkCore;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Infrastructure.Data;
using Serilog;
using Serilog.Events;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class ServiceRegistrationExtension
{
    public static  void AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(configurations =>
        {
            configurations.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, FakeCurrentService>();

        return services;
    }
}
