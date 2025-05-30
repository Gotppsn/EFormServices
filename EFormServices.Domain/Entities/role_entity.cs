// EFormServices.Domain/Entities/role_entity.cs
// Got code 30/05/2025
namespace EFormServices.Domain.Entities;

public class Role : BaseEntity
{
    public int OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }

    public Organization Organization { get; private set; } = null!;

    private readonly List<UserRole> _userRoles = new();
    private readonly List<RolePermission> _rolePermissions = new();

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() 
    {
        Name = string.Empty;
    }

    public Role(int organizationId, string name, string? description = null, bool isSystemRole = false)
    {
        OrganizationId = organizationId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        IsSystemRole = isSystemRole;
        UpdateTimestamp();
    }

    public void UpdateDetails(string name, string? description = null)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role details");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        UpdateTimestamp();
    }

    public void AddPermission(Permission permission)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role permissions");

        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        _rolePermissions.Add(new RolePermission(Id, permission.Id));
        UpdateTimestamp();
    }

    public void RemovePermission(int permissionId)
    {
        if (IsSystemRole)
            throw new InvalidOperationException("Cannot modify system role permissions");

        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
            UpdateTimestamp();
        }
    }

    public bool HasPermission(string permissionName)
    {
        return _rolePermissions.Any(rp => rp.Permission.Name == permissionName);
    }

    public IEnumerable<Permission> GetPermissions()
    {
        return _rolePermissions.Select(rp => rp.Permission);
    }
}