using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoneyTransferCenter.Application.Dtos.Mappings;
using MoneyTransferCenter.Application.Interfaces;
using MoneyTransferCenter.Application.Services;
using MoneyTransferCenter.Application.Validators.Transaction;
using MoneyTransferCenter.Domain.Entities;
using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using MoneyTransferCenter.Infrastructure.Data;
using MoneyTransferCenter.Infrastructure.MongoDB;
using MoneyTransferCenter.Infrastructure.Repositories;
using MoneyTransferCenter.Infrastructure.Services;
using MoneyTransferCenter.Infrastructure.UnitOfWork;
using MoneyTransferCenter.WebAPI.Services;
using Serilog;
using Serilog.Events;
using System.Text;

namespace MoneyTransferCenter.WebAPI.Extension;

public static class ServiceRegistrationExtension
{
    public static  void AddDatabaseConfig(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<AppDbContext>(configurations =>
        {
            configurations.UseSqlServer(configuration.GetConnectionString("SqlServer"));
        });

        services.AddSingleton<MongoDbContext>();
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IIbanGenerator, IbanGenerator>();
        services.AddScoped<ITransactionService, TransactionService>();




        services.AddValidatorsFromAssemblyContaining<IAuthService>();
        // Controllers + ValidationFilter
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });

        TypeAdapterConfig.GlobalSettings.Scan(typeof(AccountMapping).Assembly);

        return services;
    }

 

    public static void AddIdentityConfig(this IServiceCollection services, IConfiguration configuration)
    {
        // Identity
        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
        // JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
            };
        });
    }
}
