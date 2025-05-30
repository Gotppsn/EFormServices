// EFormServices.Infrastructure/Data/Configurations/SubmissionValueConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class SubmissionValueConfiguration : IEntityTypeConfiguration<SubmissionValue>
{
    public void Configure(EntityTypeBuilder<SubmissionValue> builder)
    {
        builder.ToTable("SubmissionValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FieldName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Value)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ValueType)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(e => e.FormSubmission)
            .WithMany(s => s.SubmissionValues)
            .HasForeignKey(e => e.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FormField)
            .WithMany()
            .HasForeignKey(e => e.FormFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FormSubmissionId, e.FormFieldId })
            .IsUnique();
    }
}