// EFormServices.Infrastructure/Data/Configurations/FormSubmissionConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("FormSubmissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.TrackingNumber)
            .IsUnique();

        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);

        builder.HasOne(e => e.Form)
            .WithMany(f => f.FormSubmissions)
            .HasForeignKey(e => e.FormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SubmittedByUser)
            .WithMany(u => u.FormSubmissions)
            .HasForeignKey(e => e.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FormId, e.Status, e.SubmittedAt });
        builder.HasIndex(e => new { e.SubmittedByUserId, e.SubmittedAt });
    }
}