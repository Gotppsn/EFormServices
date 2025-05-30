// EFormServices.Infrastructure/Data/MockApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Infrastructure.Services;

namespace EFormServices.Infrastructure.Data;

public class MockApplicationDbContext : IApplicationDbContext
{
    private readonly MockDbSet<Organization> _organizations;
    private readonly MockDbSet<Department> _departments;
    private readonly MockDbSet<User> _users;
    private readonly MockDbSet<Role> _roles;
    private readonly MockDbSet<Permission> _permissions;
    private readonly MockDbSet<UserRole> _userRoles;
    private readonly MockDbSet<RolePermission> _rolePermissions;
    private readonly MockDbSet<Form> _forms;
    private readonly MockDbSet<FormField> _formFields;
    private readonly MockDbSet<FormFieldOption> _formFieldOptions;
    private readonly MockDbSet<ConditionalLogic> _conditionalLogics;
    private readonly MockDbSet<FormSubmission> _formSubmissions;
    private readonly MockDbSet<SubmissionValue> _submissionValues;
    private readonly MockDbSet<FileAttachment> _fileAttachments;
    private readonly MockDbSet<ApprovalWorkflow> _approvalWorkflows;
    private readonly MockDbSet<ApprovalStep> _approvalSteps;
    private readonly MockDbSet<ApprovalProcess> _approvalProcesses;
    private readonly MockDbSet<ApprovalAction> _approvalActions;

    public IQueryable<Organization> Organizations => _organizations;
    public IQueryable<Department> Departments => _departments;
    public IQueryable<User> Users => _users;
    public IQueryable<Role> Roles => _roles;
    public IQueryable<Permission> Permissions => _permissions;
    public IQueryable<UserRole> UserRoles => _userRoles;
    public IQueryable<RolePermission> RolePermissions => _rolePermissions;
    public IQueryable<Form> Forms => _forms;
    public IQueryable<FormField> FormFields => _formFields;
    public IQueryable<FormFieldOption> FormFieldOptions => _formFieldOptions;
    public IQueryable<ConditionalLogic> ConditionalLogics => _conditionalLogics;
    public IQueryable<FormSubmission> FormSubmissions => _formSubmissions;
    public IQueryable<SubmissionValue> SubmissionValues => _submissionValues;
    public IQueryable<FileAttachment> FileAttachments => _fileAttachments;
    public IQueryable<ApprovalWorkflow> ApprovalWorkflows => _approvalWorkflows;
    public IQueryable<ApprovalStep> ApprovalSteps => _approvalSteps;
    public IQueryable<ApprovalProcess> ApprovalProcesses => _approvalProcesses;
    public IQueryable<ApprovalAction> ApprovalActions => _approvalActions;

    public MockApplicationDbContext()
    {
        _organizations = new MockDbSet<Organization>(MockDataService.GetOrganizations());
        _departments = new MockDbSet<Department>(MockDataService.GetDepartments());
        _users = new MockDbSet<User>(MockDataService.GetUsers());
        _roles = new MockDbSet<Role>(MockDataService.GetRoles());
        _permissions = new MockDbSet<Permission>(MockDataService.GetPermissions());
        _userRoles = new MockDbSet<UserRole>();
        _rolePermissions = new MockDbSet<RolePermission>();
        _forms = new MockDbSet<Form>(MockDataService.GetForms());
        _formFields = new MockDbSet<FormField>();
        _formFieldOptions = new MockDbSet<FormFieldOption>();
        _conditionalLogics = new MockDbSet<ConditionalLogic>();
        _formSubmissions = new MockDbSet<FormSubmission>(MockDataService.GetSubmissions());
        _submissionValues = new MockDbSet<SubmissionValue>();
        _fileAttachments = new MockDbSet<FileAttachment>();
        _approvalWorkflows = new MockDbSet<ApprovalWorkflow>(MockDataService.GetWorkflows());
        _approvalSteps = new MockDbSet<ApprovalStep>();
        _approvalProcesses = new MockDbSet<ApprovalProcess>();
        _approvalActions = new MockDbSet<ApprovalAction>();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public void AddEntity<T>(T entity) where T : class
    {
        switch (entity)
        {
            case Organization org:
                org.Id = _organizations.Count() + 1;
                _organizations.Add(org);
                break;
            case User user:
                user.Id = _users.Count() + 1;
                _users.Add(user);
                break;
            case Form form:
                form.Id = _forms.Count() + 1;
                _forms.Add(form);
                break;
            case FormField field:
                field.Id = _formFields.Count() + 1;
                _formFields.Add(field);
                break;
            case UserRole userRole:
                userRole.Id = _userRoles.Count() + 1;
                _userRoles.Add(userRole);
                break;
        }
    }
}