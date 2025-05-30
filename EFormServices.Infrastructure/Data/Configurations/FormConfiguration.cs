// EFormServices.Infrastructure/Data/Configurations/FormConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EFormServices.Infrastructure.Data.Configurations;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.FormType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.FormKey)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(e => e.FormKey)
            .IsUnique();

        builder.Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<FormSettings>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<FormMetadata>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.Organization)
            .WithMany(o => o.Forms)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Forms)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany(u => u.CreatedForms)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovalWorkflow)
            .WithMany(w => w.Forms)
            .HasForeignKey(e => e.ApprovalWorkflowId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.OrganizationId, e.IsActive, e.IsPublic });
        builder.HasIndex(e => new { e.OrganizationId, e.FormType, e.IsActive });
    }
}