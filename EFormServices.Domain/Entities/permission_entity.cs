// EFormServices.Domain/Entities/permission_entity.cs
// Got code 30/05/2025
namespace EFormServices.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsSystemPermission { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() 
    {
        Name = string.Empty;
        Category = string.Empty;
    }

    public Permission(string name, string category, string? description = null, bool isSystemPermission = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Description = description;
        IsSystemPermission = isSystemPermission;
        UpdateTimestamp();
    }

    public void UpdateDetails(string name, string category, string? description = null)
    {
        if (IsSystemPermission)
            throw new InvalidOperationException("Cannot modify system permission details");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Description = description;
        UpdateTimestamp();
    }

    public static class SystemPermissions
    {
        public const string ManageOrganization = "manage_organization";
        public const string ManageUsers = "manage_users";
        public const string ManageRoles = "manage_roles";
        public const string CreateForms = "create_forms";
        public const string EditForms = "edit_forms";
        public const string DeleteForms = "delete_forms";
        public const string ViewForms = "view_forms";
        public const string SubmitForms = "submit_forms";
        public const string ApproveForms = "approve_forms";
        public const string ViewReports = "view_reports";
        public const string ManageWorkflows = "manage_workflows";
        public const string ManageDepartments = "manage_departments";
        public const string ViewAuditLogs = "view_audit_logs";
    }

    public static class Categories
    {
        public const string Organization = "Organization";
        public const string Users = "Users";
        public const string Forms = "Forms";
        public const string Approvals = "Approvals";
        public const string Reports = "Reports";
        public const string System = "System";
    }
}