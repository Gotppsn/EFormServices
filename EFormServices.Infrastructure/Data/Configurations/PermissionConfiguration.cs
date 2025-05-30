// EFormServices.Infrastructure/Data/Configurations/PermissionConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.HasData(
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
        );
    }
}