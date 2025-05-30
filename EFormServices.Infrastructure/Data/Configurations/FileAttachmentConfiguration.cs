// EFormServices.Infrastructure/Data/Configurations/FileAttachmentConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.ToTable("FileAttachments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.FileHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(e => e.FormSubmission)
            .WithMany(s => s.FileAttachments)
            .HasForeignKey(e => e.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FormField)
            .WithMany()
            .HasForeignKey(e => e.FormFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FormSubmissionId, e.FormFieldId, e.FileName });
        builder.HasIndex(e => e.FileHash);
    }
}