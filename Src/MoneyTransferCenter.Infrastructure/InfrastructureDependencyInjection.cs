using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.Infrastructure.MongoDB;
using MoneyTransferCenter.Infrastructure.Repositories;
using MoneyTransferCenter.Infrastructure.Services;


namespace MoneyTransferCenter.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
   
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });
        services.AddSingleton<MongoDbContext>();


        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IExchangeRateService, TcmbExchangeRateService>();



        services.AddScoped<IIbanGenerator, IbanGenerator>(); 
        services.AddScoped<ITokenService, TokenService>();   
        services.AddHostedService<OutboxProcessor>();  
        
        return services;
    }


}
