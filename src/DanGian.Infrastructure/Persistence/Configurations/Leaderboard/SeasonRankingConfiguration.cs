using DanGian.Domain.Leaderboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DanGian.Infrastructure.Persistence.Configurations.Leaderboard;

internal sealed class SeasonRankingConfiguration : IEntityTypeConfiguration<SeasonRanking>
{
    public void Configure(EntityTypeBuilder<SeasonRanking> builder)
    {
        builder.ToTable("season_rankings", "leaderboard");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.SeasonId, r.Rank })
            .HasDatabaseName("idx_season_rankings_season_rank");

        builder.HasIndex(r => new { r.SeasonId, r.UserId })
            .IsUnique()
            .HasDatabaseName("idx_season_rankings_unique");
    }
}
