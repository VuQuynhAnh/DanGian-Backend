using DanGian.Domain.Enums;
using DanGian.Domain.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Mission;

internal sealed class UserMissionProgressConfiguration : IEntityTypeConfiguration<UserMissionProgress>
{
    public void Configure(EntityTypeBuilder<UserMissionProgress> builder)
    {
        builder.ToTable("user_progress", "mission");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<MissionStatus>(v, true))
            .HasMaxLength(20)
            .HasDefaultValue(MissionStatus.Pending);

        builder.Property(p => p.Date)
            .HasDefaultValueSql("CURRENT_DATE");

        builder.Property(p => p.Progress)
            .HasDefaultValue(0);

        builder.HasIndex(p => new { p.UserId, p.Date })
            .HasDatabaseName("idx_user_progress_user_date");

        builder.HasIndex(p => new { p.UserId, p.DefinitionId, p.Date })
            .IsUnique()
            .HasDatabaseName("idx_user_progress_unique");

        builder.Ignore(p => p.DomainEvents);
    }
}
