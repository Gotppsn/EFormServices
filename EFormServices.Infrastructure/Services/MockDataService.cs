// EFormServices.Infrastructure/Services/MockDataService.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Infrastructure.Services;

public class MockDataService
{
    private static readonly List<Organization> _organizations = new();
    private static readonly List<User> _users = new();
    private static readonly List<Role> _roles = new();
    private static readonly List<Permission> _permissions = new();
    private static readonly List<Department> _departments = new();
    private static readonly List<Form> _forms = new();
    private static readonly List<FormField> _formFields = new();
    private static readonly List<FormSubmission> _submissions = new();
    private static readonly List<ApprovalWorkflow> _workflows = new();

    static MockDataService()
    {
        InitializeMockData();
    }

    private static void InitializeMockData()
    {
        var permissions = new[]
        {
            CreatePermission(1, "manage_organization", "Organization", "Manage organization settings"),
            CreatePermission(2, "manage_users", "Users", "Create and manage users"),
            CreatePermission(3, "create_forms", "Forms", "Create new forms"),
            CreatePermission(4, "edit_forms", "Forms", "Edit existing forms"),
            CreatePermission(5, "view_forms", "Forms", "View forms"),
            CreatePermission(6, "approve_forms", "Approvals", "Approve form submissions"),
            CreatePermission(7, "view_reports", "Reports", "View reports"),
            CreatePermission(8, "publish_forms", "Forms", "Publish forms"),
            CreatePermission(9, "edit_published_forms", "Forms", "Edit published forms"),
            CreatePermission(10, "approve_form_publishing", "Forms", "Approve form publishing"),
            CreatePermission(11, "view_all_forms", "Forms", "View all organization forms")
        };
        _permissions.AddRange(permissions);

        var organization = CreateOrganization(1, "Demo Corporation", "demo");
        _organizations.Add(organization);

        var departments = new[]
        {
            CreateDepartment(1, 1, "Human Resources", "HR"),
            CreateDepartment(2, 1, "Information Technology", "IT"),
            CreateDepartment(3, 1, "Finance", "FIN")
        };
        _departments.AddRange(departments);

        var roles = new[]
        {
            CreateRole(1, 1, "Administrator", "System administrator"),
            CreateRole(2, 1, "Manager", "Department manager"),
            CreateRole(3, 1, "User", "Standard user")
        };
        _roles.AddRange(roles);

        var users = new[]
        {
            CreateUser(1, 1, "admin@demo.com", "John", "Admin", "password123"),
            CreateUser(2, 1, "manager@demo.com", "Jane", "Manager", "password123", 1),
            CreateUser(3, 1, "user@demo.com", "Bob", "User", "password123", 2),
            CreateUser(4, 1, "alice@demo.com", "Alice", "Smith", "password123", 3)
        };
        _users.AddRange(users);

        var workflow = CreateWorkflow(1, 1, "Standard Approval");
        _workflows.Add(workflow);

        var forms = new[]
        {
            CreateForm(1, 1, 1, "Leave Request Form", FormType.Request),
            CreateForm(2, 1, 2, "Employee Feedback", FormType.Feedback),
            CreateForm(3, 1, 3, "Expense Report", FormType.Report),
            CreateForm(4, 1, 1, "IT Support Request", FormType.Request),
            CreateForm(5, 1, 2, "Performance Review", FormType.Assessment)
        };
        _forms.AddRange(forms);

        var formFields = new[]
        {
            CreateFormField(1, 1, FieldType.Text, "Full Name", "fullName", 1, true),
            CreateFormField(2, 1, FieldType.Date, "Start Date", "startDate", 2, true),
            CreateFormField(3, 1, FieldType.Date, "End Date", "endDate", 3, true),
            CreateFormField(4, 1, FieldType.TextArea, "Reason", "reason", 4, true),
            CreateFormField(5, 2, FieldType.Rating, "Overall Satisfaction", "satisfaction", 1, true),
            CreateFormField(6, 2, FieldType.TextArea, "Comments", "comments", 2, false)
        };
        _formFields.AddRange(formFields);

        var submissions = new[]
        {
            CreateSubmission(1, 1, 3),
            CreateSubmission(2, 2, 2),
            CreateSubmission(3, 3, 3),
            CreateSubmission(4, 1, 4),
            CreateSubmission(5, 4, 3)
        };
        _submissions.AddRange(submissions);
    }

    private static Permission CreatePermission(int id, string name, string category, string description) => 
        new(name, category, description, true) { Id = id };

    private static Organization CreateOrganization(int id, string name, string subdomain) => 
        new(name, subdomain, OrganizationSettings.Default()) { Id = id };

    private static Department CreateDepartment(int id, int orgId, string name, string code) => 
        new(orgId, name, code) { Id = id };

    private static Role CreateRole(int id, int orgId, string name, string description) => 
        new(orgId, name, description) { Id = id };

    private static User CreateUser(int id, int orgId, string email, string firstName, string lastName, string password, int? deptId = null) => 
        new(orgId, email, firstName, lastName, password, "salt123", deptId) { Id = id };

    private static ApprovalWorkflow CreateWorkflow(int id, int orgId, string name) => 
        new(orgId, name, WorkflowType.Sequential) { Id = id };

    private static Form CreateForm(int id, int orgId, int userId, string title, FormType type)
    {
        var form = new Form(orgId, userId, title, type) { Id = id };
        form.Publish();
        return form;
    }

    private static FormField CreateFormField(int id, int formId, FieldType fieldType, string label, string name, int sortOrder, bool isRequired) => 
        new(formId, fieldType, label, name, sortOrder, isRequired) { Id = id };

    private static FormSubmission CreateSubmission(int id, int formId, int userId) => 
        new(formId, userId) { Id = id };

    public static List<Organization> GetOrganizations() => _organizations;
    public static List<User> GetUsers() => _users;
    public static List<Role> GetRoles() => _roles;
    public static List<Permission> GetPermissions() => _permissions;
    public static List<Department> GetDepartments() => _departments;
    public static List<Form> GetForms() => _forms;
    public static List<FormField> GetFormFields() => _formFields;
    public static List<FormSubmission> GetSubmissions() => _submissions;
    public static List<ApprovalWorkflow> GetWorkflows() => _workflows;

    public static Organization? GetOrganizationBySubdomain(string subdomain) =>
        _organizations.FirstOrDefault(o => o.Subdomain == subdomain);

    public static User? GetUserByEmail(string email) =>
        _users.FirstOrDefault(u => u.Email == email.ToLowerInvariant());

    public static User? GetUserById(int id) =>
        _users.FirstOrDefault(u => u.Id == id);

    public static void AddForm(Form form)
    {
        var maxId = _forms.Any() ? _forms.Max(f => f.Id) : 0;
        form.Id = maxId + 1;
        _forms.Add(form);
    }

    public static void UpdateForm(Form form)
    {
        var existingForm = _forms.FirstOrDefault(f => f.Id == form.Id);
        if (existingForm != null)
        {
            var index = _forms.IndexOf(existingForm);
            _forms[index] = form;
        }
    }

    public static void DeleteForm(int formId)
    {
        var form = _forms.FirstOrDefault(f => f.Id == formId);
        if (form != null)
        {
            _forms.Remove(form);
        }
    }
}