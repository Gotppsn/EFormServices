// EFormServices.Infrastructure/Data/Configurations/DepartmentConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.HasOne(e => e.Organization)
            .WithMany(o => o.Departments)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(e => e.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.OrganizationId, e.Code })
            .IsUnique();

        builder.HasIndex(e => new { e.OrganizationId, e.IsActive });
    }
}