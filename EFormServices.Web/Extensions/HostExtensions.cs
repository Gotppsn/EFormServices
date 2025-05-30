// EFormServices.Web/Extensions/HostExtensions.cs
// Got code 30/05/2025
using EFormServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Extensions;

public static class HostExtensions
{
    public static async Task<IHost> MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }

        return host;
    }
}