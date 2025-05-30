// EFormServices.Infrastructure/Data/Configurations/ApprovalActionConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class ApprovalActionConfiguration : IEntityTypeConfiguration<ApprovalAction>
{
    public void Configure(EntityTypeBuilder<ApprovalAction> builder)
    {
        builder.ToTable("ApprovalActions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Comments)
            .HasMaxLength(1000);

        builder.HasOne(e => e.ApprovalProcess)
            .WithMany(p => p.ApprovalActions)
            .HasForeignKey(e => e.ApprovalProcessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ApprovalStep)
            .WithMany()
            .HasForeignKey(e => e.ApprovalStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ActionByUser)
            .WithMany()
            .HasForeignKey(e => e.ActionByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ApprovalProcessId, e.ActionAt });
    }
}