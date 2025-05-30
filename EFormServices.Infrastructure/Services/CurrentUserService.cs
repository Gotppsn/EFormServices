// EFormServices.Infrastructure/Services/CurrentUserService.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EFormServices.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => GetClaimValue<int?>(ClaimTypes.NameIdentifier);
    public int? OrganizationId => GetClaimValue<int?>("OrganizationId");
    public string? Email => GetClaimValue<string>(ClaimTypes.Email);
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission)
    {
        return _httpContextAccessor.HttpContext?.User?.HasClaim("Permission", permission) ?? false;
    }

    public bool HasRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    private T? GetClaimValue<T>(string claimType)
    {
        var claimValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
        
        if (string.IsNullOrEmpty(claimValue))
            return default;

        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        try
        {
            return (T)Convert.ChangeType(claimValue, targetType);
        }
        catch
        {
            return default;
        }
    }
}