// EFormServices.Infrastructure/DependencyInjection.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Data;
using EFormServices.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFormServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (environment == "Development" && configuration.GetConnectionString("DefaultConnection")?.Contains("(localdb)") == true)
        {
            services.AddScoped<IApplicationDbContext, MockApplicationDbContext>();
            services.AddScoped<ICurrentUserService, MockCurrentUserService>();
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddHttpContextAccessor();

        if (configuration.GetConnectionString("Redis") != null)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });
        }
        else
        {
            services.AddMemoryCache();
        }

        return services;
    }
}