// EFormServices.Infrastructure/Data/InMemoryApplicationDbContext.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Infrastructure.Data;

public class InMemoryApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Form> Forms { get; set; } = null!;
    public DbSet<FormField> FormFields { get; set; } = null!;
    public DbSet<FormFieldOption> FormFieldOptions { get; set; } = null!;
    public DbSet<ConditionalLogic> ConditionalLogics { get; set; } = null!;
    public DbSet<FormSubmission> FormSubmissions { get; set; } = null!;
    public DbSet<SubmissionValue> SubmissionValues { get; set; } = null!;
    public DbSet<FileAttachment> FileAttachments { get; set; } = null!;
    public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; } = null!;
    public DbSet<ApprovalStep> ApprovalSteps { get; set; } = null!;
    public DbSet<ApprovalProcess> ApprovalProcesses { get; set; } = null!;
    public DbSet<ApprovalAction> ApprovalActions { get; set; } = null!;

    public InMemoryApplicationDbContext(DbContextOptions<InMemoryApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(50);
            entity.Ignore(e => e.Settings);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Form>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Ignore(e => e.Settings);
            entity.Ignore(e => e.Metadata);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var organizations = MockDataService.GetOrganizations();
        var users = MockDataService.GetUsers();
        var roles = MockDataService.GetRoles();
        var permissions = MockDataService.GetPermissions();
        var forms = MockDataService.GetForms();

        modelBuilder.Entity<Organization>().HasData(organizations);
        modelBuilder.Entity<User>().HasData(users);
        modelBuilder.Entity<Role>().HasData(roles);
        modelBuilder.Entity<Permission>().HasData(permissions);
        modelBuilder.Entity<Form>().HasData(forms);
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

        return await base.SaveChangesAsync(cancellationToken);
    }
}