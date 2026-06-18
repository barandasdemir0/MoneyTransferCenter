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
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
    {
       
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
   
        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        });
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
