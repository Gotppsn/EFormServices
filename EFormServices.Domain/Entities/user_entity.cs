// Got code 27/05/2025
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Domain.Entities;

public class User : BaseEntity
{
    public int OrganizationId { get; private set; }
    public int? DepartmentId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PasswordHash { get; private set; }
    public string Salt { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? ExternalId { get; private set; }

    public Organization Organization { get; private set; } = null!;
    public Department? Department { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    private readonly List<Form> _createdForms = new();
    private readonly List<FormSubmission> _formSubmissions = new();

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<Form> CreatedForms => _createdForms.AsReadOnly();
    public IReadOnlyCollection<FormSubmission> FormSubmissions => _formSubmissions.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    private User() { }

    public User(int organizationId, string email, string firstName, string lastName, 
                string passwordHash, string salt, int? departmentId = null, string? externalId = null)
    {
        OrganizationId = organizationId;
        DepartmentId = departmentId;
        Email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Salt = salt ?? throw new ArgumentNullException(nameof(salt));
        ExternalId = externalId;
        IsActive = true;
        EmailConfirmed = false;
        UpdateTimestamp();
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        UpdateTimestamp();
    }

    public void UpdateEmail(string email)
    {
        Email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        EmailConfirmed = false;
        UpdateTimestamp();
    }

    public void UpdatePassword(string passwordHash, string salt)
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Salt = salt ?? throw new ArgumentNullException(nameof(salt));
        UpdateTimestamp();
    }

    public void SetDepartment(int? departmentId)
    {
        DepartmentId = departmentId;
        UpdateTimestamp();
    }

    public void ConfirmEmail()
    {
        if (!EmailConfirmed)
        {
            EmailConfirmed = true;
            UpdateTimestamp();
        }
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
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

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur => ur.IsActive && ur.Role.Name == roleName && 
                                   (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow));
    }

    public bool HasPermission(string permissionName)
    {
        return _userRoles.Any(ur => ur.IsActive && 
                                   (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow) &&
                                   ur.Role.HasPermission(permissionName));
    }
}