// EFormServices.Application/Common/Interfaces/IApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;

namespace EFormServices.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Organization> Organizations { get; }
    IQueryable<Department> Departments { get; }
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<RolePermission> RolePermissions { get; }
    IQueryable<Form> Forms { get; }
    IQueryable<FormField> FormFields { get; }
    IQueryable<FormFieldOption> FormFieldOptions { get; }
    IQueryable<ConditionalLogic> ConditionalLogics { get; }
    IQueryable<FormSubmission> FormSubmissions { get; }
    IQueryable<SubmissionValue> SubmissionValues { get; }
    IQueryable<FileAttachment> FileAttachments { get; }
    IQueryable<ApprovalWorkflow> ApprovalWorkflows { get; }
    IQueryable<ApprovalStep> ApprovalSteps { get; }
    IQueryable<ApprovalProcess> ApprovalProcesses { get; }
    IQueryable<ApprovalAction> ApprovalActions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}