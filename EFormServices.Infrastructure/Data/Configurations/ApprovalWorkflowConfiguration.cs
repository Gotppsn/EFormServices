// EFormServices.Infrastructure/Data/Configurations/ApprovalWorkflowConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EFormServices.Infrastructure.Data.Configurations;

public class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("ApprovalWorkflows");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.WorkflowType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<WorkflowSettings>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.Organization)
            .WithMany(o => o.ApprovalWorkflows)
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.OrganizationId, e.IsActive });
    }
}