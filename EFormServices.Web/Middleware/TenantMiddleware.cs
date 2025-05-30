// EFormServices.Web/Middleware/TenantMiddleware.cs
// Got code 30/05/2025
using EFormServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var host = context.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);

        if (!string.IsNullOrEmpty(subdomain))
        {
            var organization = await dbContext.Organizations
                .FirstOrDefaultAsync(o => o.Subdomain == subdomain && o.IsActive);

            if (organization != null)
            {
                context.Items["TenantId"] = organization.Id;
                context.Items["TenantKey"] = organization.TenantKey;
            }
        }

        await _next(context);
    }

    private static string? ExtractSubdomain(string host)
    {
        if (host.Contains("localhost") || host.Contains("127.0.0.1"))
            return null;

        var parts = host.Split('.');
        return parts.Length > 2 ? parts[0] : null;
    }
}