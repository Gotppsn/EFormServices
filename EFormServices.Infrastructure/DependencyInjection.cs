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
        var useInMemoryDb = configuration.GetValue<bool>("UseInMemoryDatabase");
        
        if (useInMemoryDb)
        {
            services.AddDbContext<InMemoryApplicationDbContext>(options =>
                options.UseInMemoryDatabase("EFormServicesTestDb"));
            
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<InMemoryApplicationDbContext>());
            
            services.AddScoped<ICurrentUserService, MockCurrentUserService>();
        }
        else
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<ApplicationDbContext>());
            
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}