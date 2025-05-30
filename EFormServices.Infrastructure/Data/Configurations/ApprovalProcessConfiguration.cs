// EFormServices.Infrastructure/Data/Configurations/ApprovalProcessConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class ApprovalProcessConfiguration : IEntityTypeConfiguration<ApprovalProcess>
{
    public void Configure(EntityTypeBuilder<ApprovalProcess> builder)
    {
        builder.ToTable("ApprovalProcesses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.HasOne(e => e.FormSubmission)
            .WithMany(s => s.ApprovalProcesses)
            .HasForeignKey(e => e.FormSubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovalWorkflow)
            .WithMany()
            .HasForeignKey(e => e.ApprovalWorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentStep)
            .WithMany(s => s.ApprovalProcesses)
            .HasForeignKey(e => e.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FormSubmissionId })
            .IsUnique();

        builder.HasIndex(e => new { e.Status, e.StartedAt });
    }
}