// EFormServices.Domain/Entities/organization_entity.cs
// Got code 30/05/2025
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; private set; }
    public string Subdomain { get; private set; }
    public string TenantKey { get; private set; }
    public OrganizationSettings Settings { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Department> _departments = new();
    private readonly List<User> _users = new();
    private readonly List<Form> _forms = new();

    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Form> Forms => _forms.AsReadOnly();

    private Organization()
    {
        Name = string.Empty;
        Subdomain = string.Empty;
        TenantKey = string.Empty;
        Settings = OrganizationSettings.Default();
    }

    public Organization(string name, string subdomain, OrganizationSettings settings)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Subdomain = subdomain?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(subdomain));
        TenantKey = GenerateTenantKey();
        Settings = settings ?? OrganizationSettings.Default();
        IsActive = true;
        UpdateTimestamp();
    }

    public void UpdateSettings(OrganizationSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdateTimestamp();
    }

    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }

    private static string GenerateTenantKey()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }
}