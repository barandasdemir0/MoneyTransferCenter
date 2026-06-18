using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MoneyTransferCenter.Application.Dtos.Mappings;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Services;

namespace MoneyTransferCenter.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuditService, AuditService>();
     
        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
    
        TypeAdapterConfig.GlobalSettings.Scan(typeof(AccountMapping).Assembly);
        return services;
    }
}
