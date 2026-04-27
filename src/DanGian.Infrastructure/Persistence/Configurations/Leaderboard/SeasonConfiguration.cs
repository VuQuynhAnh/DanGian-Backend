using DanGian.Domain.Leaderboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Leaderboard;

internal sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("seasons", "leaderboard");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeasonNum)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(false);

        builder.HasIndex(s => s.SeasonNum)
            .IsUnique()
            .HasDatabaseName("idx_seasons_season_num");

        builder.HasMany(s => s.Rankings)
            .WithOne()
            .HasForeignKey(r => r.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
