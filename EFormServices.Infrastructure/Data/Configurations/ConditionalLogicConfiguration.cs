// EFormServices.Infrastructure/Data/Configurations/ConditionalLogicConfiguration.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFormServices.Infrastructure.Data.Configurations;

public class ConditionalLogicConfiguration : IEntityTypeConfiguration<ConditionalLogic>
{
    public void Configure(EntityTypeBuilder<ConditionalLogic> builder)
    {
        builder.ToTable("ConditionalLogics");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Condition)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.TriggerValue)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(e => e.Form)
            .WithMany()
            .HasForeignKey(e => e.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TriggerField)
            .WithMany(f => f.ConditionalLogics)
            .HasForeignKey(e => e.TriggerFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetField)
            .WithMany()
            .HasForeignKey(e => e.TargetFieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FormId, e.TriggerFieldId, e.TargetFieldId })
            .IsUnique();
    }
}