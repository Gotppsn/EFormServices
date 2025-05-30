// EFormServices.Infrastructure/Data/Configurations/FormFieldConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace EFormServices.Infrastructure.Data.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("FormFields");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.FieldType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ValidationRules)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<ValidationRules>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Settings)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<FieldSettings>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.Form)
            .WithMany(f => f.FormFields)
            .HasForeignKey(e => e.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FormId, e.Name })
            .IsUnique();

        builder.HasIndex(e => new { e.FormId, e.SortOrder });
    }
}