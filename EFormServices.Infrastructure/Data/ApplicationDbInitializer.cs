// EFormServices.Infrastructure/Data/ApplicationDbInitializer.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFormServices.Infrastructure.Data;

public static class ApplicationDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(context.Organizations))
            {
                await SeedDataAsync(context, logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    private static async Task SeedDataAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding initial data...");

        var defaultOrg = new Organization(
            "Demo Organization",
            "demo",
            OrganizationSettings.Default()
        );

        context.Organizations.Add(defaultOrg);
        await context.SaveChangesAsync();

        var adminRole = new Role(defaultOrg.Id, "Administrator", "System administrator with full access", true);
        var userRole = new Role(defaultOrg.Id, "User", "Standard user with basic access", true);
        
        context.Roles.Add(adminRole);
        context.Roles.Add(userRole);
        await context.SaveChangesAsync();

        var permissions = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(context.Permissions);
        foreach (var permission in permissions)
        {
            context.RolePermissions.Add(new RolePermission(adminRole.Id, permission.Id));
        }

        var basicPermissions = permissions.Where(p => p.Name.In("view_forms", "submit_forms", "create_forms")).ToList();
        foreach (var permission in basicPermissions)
        {
            context.RolePermissions.Add(new RolePermission(userRole.Id, permission.Id));
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Initial data seeded successfully");
    }
}

internal static class StringExtensions
{
    public static bool In(this string value, params string[] values)
    {
        return values.Contains(value);
    }
}