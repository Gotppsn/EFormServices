// EFormServices.Infrastructure/Data/Configurations/FormFieldOptionConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class FormFieldOptionConfiguration : IEntityTypeConfiguration<FormFieldOption>
{
    public void Configure(EntityTypeBuilder<FormFieldOption> builder)
    {
        builder.ToTable("FormFieldOptions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(e => e.FormField)
            .WithMany(f => f.Options)
            .HasForeignKey(e => e.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FormFieldId, e.Value })
            .IsUnique();

        builder.HasIndex(e => new { e.FormFieldId, e.SortOrder });
    }
}
