// EFormServices.Infrastructure/Data/ApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Organization> OrganizationSet => Set<Organization>();
    public DbSet<Department> DepartmentSet => Set<Department>();
    public DbSet<User> UserSet => Set<User>();
    public DbSet<Role> RoleSet => Set<Role>();
    public DbSet<Permission> PermissionSet => Set<Permission>();
    public DbSet<UserRole> UserRoleSet => Set<UserRole>();
    public DbSet<RolePermission> RolePermissionSet => Set<RolePermission>();
    public DbSet<Form> FormSet => Set<Form>();
    public DbSet<FormField> FormFieldSet => Set<FormField>();
    public DbSet<FormFieldOption> FormFieldOptionSet => Set<FormFieldOption>();
    public DbSet<ConditionalLogic> ConditionalLogicSet => Set<ConditionalLogic>();
    public DbSet<FormSubmission> FormSubmissionSet => Set<FormSubmission>();
    public DbSet<SubmissionValue> SubmissionValueSet => Set<SubmissionValue>();
    public DbSet<FileAttachment> FileAttachmentSet => Set<FileAttachment>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflowSet => Set<ApprovalWorkflow>();
    public DbSet<ApprovalStep> ApprovalStepSet => Set<ApprovalStep>();
    public DbSet<ApprovalProcess> ApprovalProcessSet => Set<ApprovalProcess>();
    public DbSet<ApprovalAction> ApprovalActionSet => Set<ApprovalAction>();

    public IQueryable<Organization> Organizations => OrganizationSet;
    public IQueryable<Department> Departments => DepartmentSet;
    public IQueryable<User> Users => UserSet;
    public IQueryable<Role> Roles => RoleSet;
    public IQueryable<Permission> Permissions => PermissionSet;
    public IQueryable<UserRole> UserRoles => UserRoleSet;
    public IQueryable<RolePermission> RolePermissions => RolePermissionSet;
    public IQueryable<Form> Forms => FormSet;
    public IQueryable<FormField> FormFields => FormFieldSet;
    public IQueryable<FormFieldOption> FormFieldOptions => FormFieldOptionSet;
    public IQueryable<ConditionalLogic> ConditionalLogics => ConditionalLogicSet;
    public IQueryable<FormSubmission> FormSubmissions => FormSubmissionSet;
    public IQueryable<SubmissionValue> SubmissionValues => SubmissionValueSet;
    public IQueryable<FileAttachment> FileAttachments => FileAttachmentSet;
    public IQueryable<ApprovalWorkflow> ApprovalWorkflows => ApprovalWorkflowSet;
    public IQueryable<ApprovalStep> ApprovalSteps => ApprovalStepSet;
    public IQueryable<ApprovalProcess> ApprovalProcesses => ApprovalProcessSet;
    public IQueryable<ApprovalAction> ApprovalActions => ApprovalActionSet;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new FormConfiguration());
        modelBuilder.ApplyConfiguration(new FormFieldConfiguration());
        modelBuilder.ApplyConfiguration(new FormFieldOptionConfiguration());
        modelBuilder.ApplyConfiguration(new ConditionalLogicConfiguration());
        modelBuilder.ApplyConfiguration(new FormSubmissionConfiguration());
        modelBuilder.ApplyConfiguration(new SubmissionValueConfiguration());
        modelBuilder.ApplyConfiguration(new FileAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalWorkflowConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalStepConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalProcessConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalActionConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}