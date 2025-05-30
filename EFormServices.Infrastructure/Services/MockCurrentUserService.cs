// EFormServices.Infrastructure/Services/MockCurrentUserService.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;

namespace EFormServices.Infrastructure.Services;

public class MockCurrentUserService : ICurrentUserService
{
    public int? UserId => 1;
    public int? OrganizationId => 1;
    public string? Email => "admin@demo.com";
    public bool IsAuthenticated => true;

    private readonly List<string> _permissions = new()
    {
        "manage_organization", "manage_users", "manage_roles", "create_forms",
        "edit_forms", "delete_forms", "view_forms", "submit_forms",
        "approve_forms", "view_reports", "publish_forms", "edit_published_forms",
        "view_all_forms", "approve_form_publishing"
    };

    private readonly List<string> _roles = new() { "Administrator", "Manager" };

    public bool HasPermission(string permission)
    {
        return _permissions.Contains(permission);
    }

    public bool HasRole(string role)
    {
        return _roles.Contains(role);
    }
}