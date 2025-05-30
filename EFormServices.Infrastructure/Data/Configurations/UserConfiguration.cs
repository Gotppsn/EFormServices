// EFormServices.Infrastructure/Data/Configurations/UserConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Salt)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ExternalId)
            .HasMaxLength(100);

        builder.HasOne(e => e.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.OrganizationId, e.Email })
            .IsUnique();
    }
}