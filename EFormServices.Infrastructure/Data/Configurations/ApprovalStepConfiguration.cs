// EFormServices.Infrastructure/Data/Configurations/ApprovalStepConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StepName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.StepType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ApproverCriteria)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(e => e.ApprovalWorkflow)
            .WithMany(w => w.ApprovalSteps)
            .HasForeignKey(e => e.ApprovalWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ApprovalWorkflowId, e.StepOrder })
            .IsUnique();
    }
}