// EFormServices.Infrastructure/Services/MockDataService.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Infrastructure.Services;

public class MockDataService
{
    private static List<Organization> _organizations = new();
    private static List<User> _users = new();
    private static List<Role> _roles = new();
    private static List<Permission> _permissions = new();
    private static List<Department> _departments = new();
    private static List<Form> _forms = new();
    private static List<FormSubmission> _submissions = new();
    private static List<ApprovalWorkflow> _workflows = new();

    static MockDataService()
    {
        InitializeMockData();
    }

    private static void InitializeMockData()
    {
        // Create permissions
        var permissions = new[]
        {
            new Permission("manage_organization", "Organization", "Manage organization settings", true) { Id = 1 },
            new Permission("manage_users", "Users", "Create and manage users", true) { Id = 2 },
            new Permission("manage_roles", "Users", "Create and manage roles", true) { Id = 3 },
            new Permission("create_forms", "Forms", "Create new forms", true) { Id = 4 },
            new Permission("edit_forms", "Forms", "Edit existing forms", true) { Id = 5 },
            new Permission("delete_forms", "Forms", "Delete forms", true) { Id = 6 },
            new Permission("view_forms", "Forms", "View forms", true) { Id = 7 },
            new Permission("submit_forms", "Forms", "Submit form responses", true) { Id = 8 },
            new Permission("approve_forms", "Approvals", "Approve form submissions", true) { Id = 9 },
            new Permission("view_reports", "Reports", "View form reports and analytics", true) { Id = 10 }
        };
        _permissions.AddRange(permissions);

        // Create organization
        var organization = new Organization("Demo Corporation", "demo", OrganizationSettings.Default()) { Id = 1 };
        _organizations.Add(organization);

        // Create departments
        var departments = new[]
        {
            new Department(1, "Human Resources", "HR", "Human Resources Department") { Id = 1 },
            new Department(1, "Information Technology", "IT", "IT Department") { Id = 2 },
            new Department(1, "Finance", "FIN", "Finance Department") { Id = 3 }
        };
        _departments.AddRange(departments);

        // Create roles
        var adminRole = new Role(1, "Administrator", "System administrator with full access", true) { Id = 1 };
        var managerRole = new Role(1, "Manager", "Department manager with approval rights", false) { Id = 2 };
        var userRole = new Role(1, "User", "Standard user with basic access", false) { Id = 3 };
        _roles.AddRange(new[] { adminRole, managerRole, userRole });

        // Create users
        var adminUser = new User(1, "admin@demo.com", "John", "Admin", "hashedpassword", "salt123") { Id = 1 };
        var managerUser = new User(1, "manager@demo.com", "Jane", "Manager", "hashedpassword", "salt123", 1) { Id = 2 };
        var regularUser = new User(1, "user@demo.com", "Bob", "User", "hashedpassword", "salt123", 2) { Id = 3 };
        _users.AddRange(new[] { adminUser, managerUser, regularUser });

        // Create workflows
        var approvalWorkflow = new ApprovalWorkflow(1, "Standard Approval", WorkflowType.Sequential, "Standard approval process") { Id = 1 };
        _workflows.Add(approvalWorkflow);

        // Create sample forms
        var leaveForm = new Form(1, 1, "Leave Request Form", FormType.Request, "Submit your leave requests through this form", 1, 1) { Id = 1 };
        var feedbackForm = new Form(1, 1, "Employee Feedback", FormType.Feedback, "Share your feedback about the organization", null, null) { Id = 2 };
        var expenseForm = new Form(1, 2, "Expense Report", FormType.Report, "Submit your expense reports", 3, 1) { Id = 3 };
        
        leaveForm.Publish();
        feedbackForm.Publish();
        expenseForm.Publish();
        
        _forms.AddRange(new[] { leaveForm, feedbackForm, expenseForm });

        // Create sample submissions
        var submission1 = new FormSubmission(1, 3, "192.168.1.100", "Mozilla/5.0...") { Id = 1 };
        var submission2 = new FormSubmission(2, 2, "192.168.1.101", "Mozilla/5.0...") { Id = 2 };
        var submission3 = new FormSubmission(3, 3, "192.168.1.102", "Mozilla/5.0...") { Id = 3 };
        
        submission1.UpdateStatus(SubmissionStatus.PendingApproval);
        submission2.UpdateStatus(SubmissionStatus.Approved);
        submission3.UpdateStatus(SubmissionStatus.Submitted);
        
        _submissions.AddRange(new[] { submission1, submission2, submission3 });
    }

    public static List<Organization> GetOrganizations() => _organizations;
    public static List<User> GetUsers() => _users;
    public static List<Role> GetRoles() => _roles;
    public static List<Permission> GetPermissions() => _permissions;
    public static List<Department> GetDepartments() => _departments;
    public static List<Form> GetForms() => _forms;
    public static List<FormSubmission> GetSubmissions() => _submissions;
    public static List<ApprovalWorkflow> GetWorkflows() => _workflows;

    public static Organization? GetOrganizationBySubdomain(string subdomain)
        => _organizations.FirstOrDefault(o => o.Subdomain == subdomain);

    public static User? GetUserByEmail(string email)
        => _users.FirstOrDefault(u => u.Email == email.ToLowerInvariant());

    public static User? GetUserById(int id)
        => _users.FirstOrDefault(u => u.Id == id);
}