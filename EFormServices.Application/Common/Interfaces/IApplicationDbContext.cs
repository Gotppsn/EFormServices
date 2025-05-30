// EFormServices.Application/Common/Interfaces/IApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<Department> Departments { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<Form> Forms { get; }
    DbSet<FormField> FormFields { get; }
    DbSet<FormFieldOption> FormFieldOptions { get; }
    DbSet<ConditionalLogic> ConditionalLogics { get; }
    DbSet<FormSubmission> FormSubmissions { get; }
    DbSet<SubmissionValue> SubmissionValues { get; }
    DbSet<FileAttachment> FileAttachments { get; }
    DbSet<ApprovalWorkflow> ApprovalWorkflows { get; }
    DbSet<ApprovalStep> ApprovalSteps { get; }
    DbSet<ApprovalProcess> ApprovalProcesses { get; }
    DbSet<ApprovalAction> ApprovalActions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}