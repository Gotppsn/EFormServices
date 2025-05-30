// EFormServices.Infrastructure/DependencyInjection.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EFormServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}

public interface ICurrentUserService
{
    int? UserId { get; }
    int? OrganizationId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
}

public class CurrentUserService : ICurrentUserService
{
    public int? UserId => 1;
    public int? OrganizationId => 1;
    public string? Email => "admin@demo.com";
    public bool IsAuthenticated => true;
    public bool HasPermission(string permission) => true;
    public bool HasRole(string role) => true;
}