// EFormServices.Infrastructure/DependencyInjection.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Data;
using EFormServices.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFormServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApplicationDbContext, MockApplicationDbContext>();
        services.AddScoped<ICurrentUserService, MockCurrentUserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddHttpContextAccessor();

        return services;
    }
}