// EFormServices.Domain/Entities/Organization.cs
// Got code 30/05/2025
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string TenantKey { get; set; } = string.Empty;
    public OrganizationSettings Settings { get; set; } = new();
    public bool IsActive { get; set; }

    private readonly List<Department> _departments = new();
    private readonly List<User> _users = new();
    private readonly List<Form> _forms = new();
    private readonly List<ApprovalWorkflow> _approvalWorkflows = new();

    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Form> Forms => _forms.AsReadOnly();
    public IReadOnlyCollection<ApprovalWorkflow> ApprovalWorkflows => _approvalWorkflows.AsReadOnly();

    public Organization()
    {
        Settings = OrganizationSettings.Default();
        TenantKey = GenerateTenantKey();
        Name = string.Empty;
        Subdomain = string.Empty;
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