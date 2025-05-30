// EFormServices.Infrastructure/Data/Configurations/OrganizationConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EFormServices.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Subdomain)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.Subdomain)
            .IsUnique();

        builder.Property(e => e.TenantKey)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasIndex(e => e.TenantKey)
            .IsUnique();

        builder.Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<OrganizationSettings>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();
    }
}