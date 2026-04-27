using DanGian.Domain.Enums;
using DanGian.Domain.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Mission;

internal sealed class MissionDefinitionConfiguration : IEntityTypeConfiguration<MissionDefinition>
{
    public void Configure(EntityTypeBuilder<MissionDefinition> builder)
    {
        builder.ToTable("definitions", "mission");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasColumnType("text");

        builder.Property(m => m.ResetType)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResetType>(v, true))
            .HasMaxLength(20)
            .HasDefaultValue(ResetType.Daily);

        builder.Property(m => m.GameType)
            .HasMaxLength(50);

        builder.Property(m => m.Target)
            .HasDefaultValue(1);

        builder.Property(m => m.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(m => m.Type)
            .IsUnique()
            .HasDatabaseName("idx_mission_definitions_type");

        builder.Ignore(m => m.DomainEvents);
    }
}
