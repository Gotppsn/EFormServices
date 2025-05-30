// EFormServices.Infrastructure/Data/MockApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<Organization> Organizations => _organizations;
    public DbSet<Department> Departments => _departments;
    public DbSet<User> Users => _users;
    public DbSet<Role> Roles => _roles;
    public DbSet<Permission> Permissions => _permissions;
    public DbSet<UserRole> UserRoles => _userRoles;
    public DbSet<RolePermission> RolePermissions => _rolePermissions;
    public DbSet<Form> Forms => _forms;
    public DbSet<FormField> FormFields => _formFields;
    public DbSet<FormFieldOption> FormFieldOptions => _formFieldOptions;
    public DbSet<ConditionalLogic> ConditionalLogics => _conditionalLogics;
    public DbSet<FormSubmission> FormSubmissions => _formSubmissions;
    public DbSet<SubmissionValue> SubmissionValues => _submissionValues;
    public DbSet<FileAttachment> FileAttachments => _fileAttachments;
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => _approvalWorkflows;
    public DbSet<ApprovalStep> ApprovalSteps => _approvalSteps;
    public DbSet<ApprovalProcess> ApprovalProcesses => _approvalProcesses;
    public DbSet<ApprovalAction> ApprovalActions => _approvalActions;

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
}