// Got code 27/05/2025
namespace EFormServices.Domain.Entities;

public class Department : BaseEntity
{
    public int OrganizationId { get; private set; }
    public int? ParentDepartmentId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public Organization Organization { get; private set; } = null!;
    public Department? ParentDepartment { get; private set; }

    private readonly List<Department> _childDepartments = new();
    private readonly List<User> _users = new();
    private readonly List<Form> _forms = new();

    public IReadOnlyCollection<Department> ChildDepartments => _childDepartments.AsReadOnly();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();
    public IReadOnlyCollection<Form> Forms => _forms.AsReadOnly();

    private Department() { }

    public Department(int organizationId, string name, string code, string? description = null, int? parentDepartmentId = null)
    {
        OrganizationId = organizationId;
        ParentDepartmentId = parentDepartmentId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Code = code?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(code));
        Description = description;
        IsActive = true;
        UpdateTimestamp();
    }

    public void UpdateDetails(string name, string code, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Code = code?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(code));
        Description = description;
        UpdateTimestamp();
    }

    public void SetParentDepartment(int? parentDepartmentId)
    {
        if (parentDepartmentId == Id)
            throw new InvalidOperationException("Department cannot be its own parent");

        ParentDepartmentId = parentDepartmentId;
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

    public bool IsChildOf(int departmentId)
    {
        var current = ParentDepartment;
        while (current != null)
        {
            if (current.Id == departmentId)
                return true;
            current = current.ParentDepartment;
        }
        return false;
    }

    public IEnumerable<Department> GetAllChildren()
    {
        foreach (var child in _childDepartments)
        {
            yield return child;
            foreach (var grandchild in child.GetAllChildren())
            {
                yield return grandchild;
            }
        }
    }
}